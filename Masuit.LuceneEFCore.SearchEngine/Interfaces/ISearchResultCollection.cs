using System.Collections.Generic;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    /// <summary>
    /// 搜索结果集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISearchResultCollection<T>
    {
        /// <summary>
        /// 总条数
        /// </summary>
        int TotalHits { get; set; }

        /// <summary>
        /// 耗时
        /// </summary>
        long Elapsed { get; set; }

        /// <summary>
        /// 结果集
        /// </summary>
        IList<T> Results { get; set; }
    }
}