using System.Collections.Generic;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    /// <summary>
    /// 搜索结果集
    /// </summary>
    public interface ILuceneSearchResultCollection
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
        IList<ILuceneSearchResult> Results { get; set; }
    }
}