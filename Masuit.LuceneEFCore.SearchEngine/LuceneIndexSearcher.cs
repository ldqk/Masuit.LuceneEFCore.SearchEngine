using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.LuceneEFCore.SearchEngine.Linq;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TinyPinyin;

namespace Masuit.LuceneEFCore.SearchEngine
{
    public class LuceneIndexSearcher : ILuceneIndexSearcher
    {
        private readonly Directory _directory;
        private readonly Analyzer _analyzer;
        private readonly IMemoryCache _memoryCache;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="directory">索引目录</param>
        /// <param name="analyzer">索引分析器</param>
        /// <param name="memoryCache">内存缓存</param>
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
            if (keyword.Length <= 2)
            {
                return new List<string>
                {
                    keyword
                };
            }

            keyword = keyword.Replace("AND ", "+").Replace("NOT ", "-").Replace("OR ", " ");
            return _memoryCache.GetOrCreate(keyword, entry =>
            {
                entry.AbsoluteExpiration = DateTimeOffset.Now.AddHours(1);
                var list = new List<string>
                {
                    keyword
                };
                list.AddRange(Regex.Matches(keyword, @""".+""").Cast<Match>().Select(m =>
                {
                    keyword = keyword.Replace(m.Value, "");
                    return m.Value;
                }));//必须包含的
                list.AddRange(Regex.Matches(keyword, @"\s-.+\s?").Cast<Match>().Select(m =>
                {
                    keyword = keyword.Replace(m.Value, "");
                    return m.Value.Trim();
                }));//必须不包含的
                list.AddRange(Regex.Matches(keyword, @"[\u4e00-\u9fa5]+").Cast<Match>().Select(m => m.Value));//中文
                list.AddRange(Regex.Matches(keyword, @"\p{P}?[A-Z]*[a-z]*[\p{P}|\p{S}]*").Cast<Match>().Select(m => m.Value));//英文单词
                list.AddRange(Regex.Matches(keyword, "([A-z]+)([0-9.]+)").Cast<Match>().SelectMany(m => m.Groups.Cast<Group>().Select(g => g.Value)));//英文+数字
                list.AddRange(new JiebaSegmenter().Cut(keyword, true));//结巴分词
                list.RemoveAll(s => s.Length < 2);
                list.AddRange(KeywordsManager.SynonymWords.Where(t => list.Contains(t.key) || list.Contains(t.value)).SelectMany(t => new[] { t.key, t.value }));
                var pinyins = new List<string>();
                foreach (var s in list)
                {
                    pinyins.AddRange(KeywordsManager.Pinyins.ToLookup(t => t.value)[PinyinHelper.GetPinyin(Regex.Replace(s, @"\p{P}|\p{S}", ""))].Select(t => t.key));
                }

                return list.Union(pinyins).Distinct().OrderByDescending(s => s.Length).Take(10).Select(s => s.Trim('[', ']', '{', '}', '(', ')')).ToList();
            });
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
                try
                {
                    if (term.StartsWith("\""))
                    {
                        finalQuery.Add(parser.Parse(term.Trim('"')), Occur.MUST);
                    }
                    else if (term.StartsWith("-"))
                    {
                        finalQuery.Add(parser.Parse(term), Occur.MUST_NOT);
                    }
                    else
                    {
                        finalQuery.Add(parser.Parse(term.Replace("~", "") + "~"), Occur.SHOULD);
                    }
                }
                catch (ParseException)
                {
                    finalQuery.Add(parser.Parse(Regex.Replace(term, @"\p{P}|\p{S}", "")), Occur.SHOULD);
                }
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
            using var reader = DirectoryReader.Open(_directory);
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
                var queryParser = new QueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, options.Fields[0], _analyzer);
                query = queryParser.Parse(options.Keywords);
            }
            else
            {
                // 多字段搜索
                var queryParser = new MultiFieldQueryParser(Lucene.Net.Util.LuceneVersion.LUCENE_48, options.Fields.ToArray(), _analyzer, options.Boosts);
                query = GetFuzzyquery(queryParser, options.Keywords);
            }

            // 排序规则处理
            var sort = new Sort(options.OrderBy.ToArray());
            Expression<Func<ScoreDoc, bool>> where = m => m.Score >= options.Score;
            if (options.Type != null)
            {
                // 过滤掉已经设置了类型的对象
                where = where.And(m => options.Type.AssemblyQualifiedName == searcher.Doc(m.Doc).Get("Type"));
            }

            var matches = searcher.Search(query, options.Filter, options.MaximumNumberOfHits, sort, true, true).ScoreDocs.Where(where.Compile());
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
            ILuceneSearchResultCollection results;
            var sw = Stopwatch.StartNew();
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
        /// <param name="boosts">多字段搜索时，给字段的搜索加速</param>
        /// <param name="type">文档类型</param>
        /// <param name="sortBy">排序规则</param>
        /// <param name="skip">跳过多少条</param>
        /// <param name="take">取多少条</param>
        /// <returns></returns>
        public ILuceneSearchResultCollection ScoredSearch(string keywords, string fields, int maximumNumberOfHits, Dictionary<string, float> boosts, Type type, string sortBy, int? skip, int? take)
        {
            var options = new SearchOptions(keywords, fields, maximumNumberOfHits, boosts, type, sortBy, skip, take);
            return ScoredSearch(options);
        }
    }
}
