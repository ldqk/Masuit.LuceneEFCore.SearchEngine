using JiebaNet.Segmenter;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.JieBa;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 搜索引擎
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class SearchEngine<TContext> : ISearchEngine<TContext> where TContext : DbContext
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        public TContext Context { get; protected set; }

        private static Directory _directory;
        private static Analyzer _analyzer;
        private static LuceneIndexer _indexer;
        private static LuceneIndexSearcher _searcher;
        private static bool isInitialized = false;
        private readonly IMemoryCache _memoryCache;
        /// <summary>
        /// 索引条数
        /// </summary>
        public int IndexCount => _indexer.Count();

        /// <summary>
        /// 搜索引擎
        /// </summary>
        /// <param name="indexerOptions">索引选项</param>
        /// <param name="context">数据库上下文</param>
        /// <param name="overrideIfExists">是否被覆盖</param>
        public SearchEngine(LuceneIndexerOptions indexerOptions, TContext context, IMemoryCache memoryCache, bool overrideIfExists = false)
        {
            Context = context;
            _memoryCache = memoryCache;
            if (isInitialized == false || overrideIfExists)
            {
                InitializeLucene(indexerOptions);
                isInitialized = true;
            }

        }

        /// <summary>
        /// 初始化索引库
        /// </summary>
        /// <param name="options"></param>
        private void InitializeLucene(LuceneIndexerOptions options)
        {
            if (_directory == null)
            {
                _directory = FSDirectory.Open(options.Path);
            }

            _analyzer = new JieBaAnalyzer(TokenizerMode.Search);
            _indexer = new LuceneIndexer(_directory, _analyzer);
            _searcher = new LuceneIndexSearcher(_directory, _analyzer, _memoryCache);

        }

        /// <summary>
        /// 检查数据库上下文更改，并返回LuceneIndexChanges类型的集合
        /// </summary>
        /// <returns> LuceneIndexChangeset  - 转换为LuceneIndexChanges类型的实体更改集合</returns>
        private LuceneIndexChangeset GetChangeset()
        {
            LuceneIndexChangeset changes = new LuceneIndexChangeset();

            foreach (var entity in Context.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged))
            {
                Type entityType = entity.Entity.GetType();
                bool implementsILuceneIndexable = typeof(ILuceneIndexable).IsAssignableFrom(entityType);
                if (implementsILuceneIndexable)
                {
                    MethodInfo method = entityType.GetMethod("ToDocument");
                    if (method != null)
                    {
                        LuceneIndexChange change = new LuceneIndexChange(entity.Entity as ILuceneIndexable);

                        switch (entity.State)
                        {
                            case EntityState.Added:
                                change.State = LuceneIndexState.Added;
                                break;
                            case EntityState.Deleted:
                                change.State = LuceneIndexState.Removed;
                                break;
                            case EntityState.Modified:
                                change.State = LuceneIndexState.Updated;
                                break;
                            default:
                                change.State = LuceneIndexState.Unchanged;
                                break;
                        }
                        changes.Entries.Add(change);
                    }
                }
            }

            return changes;
        }

        /// <summary>
        ///获取文档的具体版本
        /// </summary>
        /// <param name ="doc">要转换的文档</param>
        /// <returns></returns>
        private ILuceneIndexable GetConcreteFromDocument(Document doc)
        {
            Type t = Type.GetType(doc.Get("Type"));
            var obj = t.Assembly.CreateInstance(t.FullName, true) as ILuceneIndexable;
            foreach (var p in t.GetProperties().Where(p => p.GetCustomAttributes<LuceneIndexAttribute>().Any()))
            {
                p.SetValue(obj, doc.Get(p.Name, p.PropertyType));
            }
            return obj;
        }

        /// <summary>
        /// 保存数据更改并同步索引
        /// </summary>
        /// <returns></returns>
        public int SaveChanges(bool index = true)
        {
            int result = 0;

            if (Context.ChangeTracker.HasChanges())
            {
                // 获取要变更的实体集
                LuceneIndexChangeset changes = GetChangeset();
                result = Context.SaveChanges();
                if (changes.HasChanges && index)
                {
                    _indexer.Update(changes);
                }
            }

            return result;
        }

        /// <summary>
        /// 保存数据更改并同步索引
        /// </summary>
        /// <param name="index">是否需要被重新索引</param>
        /// <returns></returns>
        public async Task<int> SaveChangesAsync(bool index = true)
        {
            int result = 0;

            if (Context.ChangeTracker.HasChanges())
            {
                // 获取要变更的结果集
                LuceneIndexChangeset changes = GetChangeset();
                result = await Context.SaveChangesAsync();
                if (changes.HasChanges && index)
                {
                    _indexer.Update(changes);
                }
            }

            return result;
        }

        /// <summary>
        /// 扫描数据库上下文并对所有已实现ILuceneIndexable的对象，并创建索引
        /// </summary>
        public void CreateIndex()
        {
            if (_indexer != null)
            {
                List<ILuceneIndexable> index = new List<ILuceneIndexable>();
                PropertyInfo[] properties = Context.GetType().GetProperties();
                foreach (PropertyInfo pi in properties)
                {
                    if (typeof(IEnumerable<ILuceneIndexable>).IsAssignableFrom(pi.PropertyType))
                    {
                        var entities = Context.GetType().GetProperty(pi.Name).GetValue(Context, null);
                        index.AddRange(entities as IEnumerable<ILuceneIndexable>);
                    }
                }

                if (index.Any())
                {
                    _indexer.CreateIndex(index);
                }
            }
        }

        /// <summary>
        /// 创建指定数据表的索引
        /// </summary>
        public void CreateIndex(List<string> tables)
        {
            if (_indexer != null)
            {
                List<ILuceneIndexable> index = new List<ILuceneIndexable>();
                PropertyInfo[] properties = Context.GetType().GetProperties();
                foreach (PropertyInfo pi in properties)
                {
                    if (typeof(IEnumerable<ILuceneIndexable>).IsAssignableFrom(pi.PropertyType) && tables.Contains(pi.Name))
                    {
                        var entities = Context.GetType().GetProperty(pi.Name).GetValue(Context, null);
                        index.AddRange(entities as IEnumerable<ILuceneIndexable>);
                    }
                }

                if (index.Any())
                {
                    _indexer.CreateIndex(index);
                }
            }
        }

        /// <summary>
        /// 删除索引
        /// </summary>
        public void DeleteIndex()
        {
            _indexer?.DeleteAll();
        }

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型，但不返回任何评分信息
        /// </summary>
        /// <typeparam name ="T">要搜索的实体类型 - 注意：必须实现ILuceneIndexable </typeparam>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        public ISearchResultCollection<T> Search<T>(SearchOptions options)
        {
            options.Type = typeof(T);
            var indexResults = _searcher.ScoredSearch(options);

            ISearchResultCollection<T> resultSet = new SearchResultCollection<T>()
            {
                TotalHits = indexResults.TotalHits
            };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var indexResult in indexResults.Results)
            {
                T entity = (T)GetConcreteFromDocument(indexResult.Document);
                resultSet.Results.Add(entity);
            }
            sw.Stop();
            resultSet.Elapsed = indexResults.Elapsed + sw.ElapsedMilliseconds;

            return resultSet;
        }

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型，但不返回任何评分信息
        /// </summary>
        /// <typeparam name ="T">要搜索的实体类型 - 注意：必须实现ILuceneIndexable </typeparam>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        public IScoredSearchResultCollection<T> ScoredSearch<T>(SearchOptions options)
        {
            // 确保类型匹配
            if (typeof(T) != typeof(ILuceneIndexable))
            {
                options.Type = typeof(T);
            }

            var indexResults = _searcher.ScoredSearch(options);

            IScoredSearchResultCollection<T> results = new ScoredSearchResultCollection<T>();
            results.TotalHits = indexResults.TotalHits;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (var indexResult in indexResults.Results)
            {
                IScoredSearchResult<T> result = new ScoredSearchResult<T>();
                result.Score = indexResult.Score;
                result.Entity = (T)GetConcreteFromDocument(indexResult.Document);
                results.Results.Add(result);
            }
            sw.Stop();
            results.Elapsed = indexResults.Elapsed + sw.ElapsedMilliseconds;

            return results;
        }

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型
        /// </summary>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        public IScoredSearchResultCollection<ILuceneIndexable> ScoredSearch(SearchOptions options)
        {
            return ScoredSearch<ILuceneIndexable>(options);
        }

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型
        /// </summary>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        public ISearchResultCollection<ILuceneIndexable> Search(SearchOptions options)
        {
            return Search<ILuceneIndexable>(options);
        }

        /// <summary>
        /// 搜索一条匹配度最高的记录
        /// </summary>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        public ILuceneIndexable SearchOne(SearchOptions options)
        {
            return GetConcreteFromDocument(_searcher.ScoredSearchSingle(options));
        }

        /// <summary>
        /// 搜索一条匹配度最高的记录
        /// </summary>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        public T SearchOne<T>(SearchOptions options) where T : class
        {
            return GetConcreteFromDocument(_searcher.ScoredSearchSingle(options)) as T;
        }
    }
}