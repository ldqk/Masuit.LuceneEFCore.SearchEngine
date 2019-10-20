using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    public interface ISearchEngine<out TContext> where TContext : DbContext
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// 索引器
        /// </summary>
        ILuceneIndexer LuceneIndexer { get; }

        /// <summary>
        /// 索引搜索器
        /// </summary>
        ILuceneIndexSearcher LuceneIndexSearcher { get; }

        /// <summary>
        /// 索引总数
        /// </summary>
        int IndexCount { get; }

        /// <summary>
        /// 创建索引
        /// </summary>
        void CreateIndex();

        /// <summary>
        /// 创建指定数据表的索引
        /// </summary>
        void CreateIndex(List<string> tables);

        /// <summary>
        /// 删除索引
        /// </summary>
        void DeleteIndex();

        /// <summary>
        /// 保存数据更改并同步索引
        /// </summary>
        /// <param name="index">创建索引</param>
        /// <returns></returns>
        int SaveChanges(bool index = true);

        /// <summary>
        /// 保存数据更改并同步索引
        /// </summary>
        /// <param name="index">创建索引</param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(bool index = true);

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型
        /// </summary>
        /// <param name ="options">搜索选项</param>
        IScoredSearchResultCollection<ILuceneIndexable> ScoredSearch(SearchOptions options);

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型
        /// </summary>
        /// <typeparam name ="T">要搜索的实体类型 - 注意：必须实现ILuceneIndexable </typeparam>
        /// <param name ="options">搜索选项</param>
        IScoredSearchResultCollection<T> ScoredSearch<T>(SearchOptions options);

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型，但不返回任何评分信息
        /// </summary>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        ISearchResultCollection<ILuceneIndexable> Search(SearchOptions options);

        /// <summary>
        /// 执行搜索并将结果限制为特定类型，在返回之前，搜索结果将转换为相关类型，但不返回任何评分信息
        /// </summary>
        /// <typeparam name ="T">要搜索的实体类型 - 注意：必须实现ILuceneIndexable </typeparam>
        /// <param name ="options">搜索选项</param>
        /// <returns></returns>
        ISearchResultCollection<T> Search<T>(SearchOptions options);

        /// <summary>
        /// 搜索一条匹配度最高的记录
        /// </summary>
        /// <param name ="options">搜索选项</param>
        ILuceneIndexable SearchOne(SearchOptions options);

        /// <summary>
        /// 搜索一条匹配度最高的记录
        /// </summary>
        /// <param name ="options">搜索选项</param>
        T SearchOne<T>(SearchOptions options) where T : class;
    }
}