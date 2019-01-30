using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    public interface ISearchContextProvider<out TContext> where TContext : DbContext
    {
        /// <summary>
        /// 数据库上下文
        /// </summary>
        TContext Context { get; }

        /// <summary>
        /// 索引总数
        /// </summary>
        int IndexCount { get; }

        /// <summary>
        /// 创建索引
        /// </summary>
        void CreateIndex();

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
    }
}