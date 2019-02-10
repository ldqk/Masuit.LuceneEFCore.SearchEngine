using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Masuit.LuceneEFCore.SearchEngine
{
    public class LuceneIndexSearcher : ILuceneIndexSearcher
    {
        private static Directory _directory;
        private static Analyzer _analyzer;
        private readonly IMemoryCache _memoryCache;

        private static readonly HttpClient HttpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://zhannei.baidu.com")
        };

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="directory">索引目录</param>
        /// <param name="analyzer">索引分析器</param>
        public LuceneIndexSearcher(Directory directory, Analyzer analyzer, IMemoryCache memoryCache)
        {
            _directory = directory;
            _analyzer = analyzer;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public List<string> CutKeywords(string keyword)
        {
            if (_memoryCache.TryGetValue(keyword, out List<string> list))
            {
                return list;
            }
            var set = new HashSet<string>
            {
                keyword
            };
            var mc = Regex.Matches(keyword, @"(([A-Z]*[a-z]*)[\d]*)([\u4E00-\u9FA5]+)*((?!\p{P}).)*");
            foreach (Match m in mc)
            {
                set.Add(m.Value);
                foreach (Group g in m.Groups)
                {
                    set.Add(g.Value);
                }
            }
            if (keyword.Length >= 6)
            {
                try
                {
                    var res = HttpClient.GetAsync($"/api/customsearch/keywords?title={keyword}").Result;
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        BaiduAnalysisModel model = JsonConvert.DeserializeObject<BaiduAnalysisModel>(res.Content.ReadAsStringAsync().Result);
                        model.Result.Res.KeywordList?.ForEach(s => set.Add(s));
                    }
                }
                catch
                {
                    // ignored
                }
            }
            set.RemoveWhere(s => s.Length < 2 || Regex.IsMatch(s, @"^\p{P}.*"));
            list = set.OrderByDescending(s => s.Length).ToList();
            _memoryCache.Set(keyword, list, TimeSpan.FromHours(1));
            return list;
        }

        /// <summary>
        /// 分词模糊查询
        /// </summary>
        /// <param name="parser">条件</param>
        /// <param name="keywords">关键词</param>
        /// <returns></returns>
        private BooleanQuery GetFuzzyquery(MultiFieldQueryParser parser, string keywords)
        {
            var finalQuery = new BooleanQuery();
            var terms = CutKeywords(keywords);
            foreach (var term in terms)
            {
                finalQuery.Add(parser.Parse(term.Replace("~", "") + "~"), Occur.SHOULD);
            }
            return finalQuery;
        }

        /// <summary>
        /// 执行搜索
        /// </summary>
        /// <param name="options">搜索选项</param>
        /// <param name="safeSearch">启用安全搜索</param>
        /// <returns></returns>
        private ILuceneSearchResultCollection PerformSearch(SearchOptions options, bool safeSearch)
        {
            // 结果集
            ILuceneSearchResultCollection results = new LuceneSearchResultCollection();
            using (var reader = DirectoryReader.Open(_directory))
            {
                var searcher = new IndexSearcher(reader);
                Query query;

                // 启用安全搜索
                if (safeSearch)
                {
                    options.Keywords = QueryParserBase.Escape(options.Keywords);
                }

                if (options.Fields.Count == 1)
                {
                    // 单字段搜索
                    QueryParser queryParser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, options.Fields[0], _analyzer);
                    query = queryParser.Parse(options.Keywords);
                }
                else
                {
                    // 多字段搜索
                    MultiFieldQueryParser multiFieldQueryParser = new MultiFieldQueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, options.Fields.ToArray(), _analyzer, options.Boosts);
                    query = GetFuzzyquery(multiFieldQueryParser, options.Keywords);
                }

                List<SortField> sortFields = new List<SortField>
                {
                    SortField.FIELD_SCORE
                };

                // 排序规则处理
                foreach (var sortField in options.OrderBy)
                {
                    sortFields.Add(new SortField(sortField, SortFieldType.STRING));
                }

                Sort sort = new Sort(sortFields.ToArray());
                Expression<Func<ScoreDoc, bool>> where = _ => true;
                if (options.Type != null)
                {
                    // 过滤掉已经设置了类型的对象
                    where = where.And(m => options.Type.AssemblyQualifiedName == searcher.Doc(m.Doc).Get("Type"));
                }
                var matches = searcher.Search(query, null, options.MaximumNumberOfHits, sort, true, true).ScoreDocs.Where(where.Compile());
                results.TotalHits = matches.Count();

                // 分页处理
                if (options.Skip.HasValue)
                {
                    matches = matches.Skip(options.Skip.Value);
                }
                if (options.Take.HasValue)
                {
                    matches = matches.Take(options.Take.Value);
                }

                var docs = matches.ToList();
                // 创建结果集
                foreach (var match in docs)
                {
                    var doc = searcher.Doc(match.Doc);
                    results.Results.Add(new LuceneSearchResult()
                    {
                        Score = match.Score,
                        Document = doc
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 搜索单条记录
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public Document ScoredSearchSingle(SearchOptions options)
        {
            options.MaximumNumberOfHits = 1;
            var results = ScoredSearch(options);
            return results.TotalHits > 0 ? results.Results.First().Document : null;
        }

        /// <summary>
        /// 按权重搜索
        /// </summary>
        /// <param name="options">搜索选项</param>
        /// <returns></returns>
        public ILuceneSearchResultCollection ScoredSearch(SearchOptions options)
        {
            Stopwatch sw = new Stopwatch();
            ILuceneSearchResultCollection results;
            sw.Start();
            try
            {
                results = PerformSearch(options, false);
            }
            catch (ParseException)
            {
                results = PerformSearch(options, true);
            }

            sw.Stop();
            results.Elapsed = sw.ElapsedMilliseconds;
            return results;
        }

        /// <summary>
        /// 按权重搜索
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fields">限定检索字段</param>
        /// <param name="maximumNumberOfHits">最大检索量</param>
        /// <param name="boosts">加速器</param>
        /// <param name="type">文档类型</param>
        /// <param name="sortBy">排序规则</param>
        /// <param name="skip">跳过多少条</param>
        /// <param name="take">取多少条</param>
        /// <returns></returns>
        public ILuceneSearchResultCollection ScoredSearch(string keywords, string fields, int maximumNumberOfHits, Dictionary<string, float> boosts, Type type, string sortBy, int? skip, int? take)
        {
            SearchOptions options = new SearchOptions(keywords, fields, maximumNumberOfHits, boosts, type, sortBy, skip, take);
            return ScoredSearch(options);
        }
    }
}