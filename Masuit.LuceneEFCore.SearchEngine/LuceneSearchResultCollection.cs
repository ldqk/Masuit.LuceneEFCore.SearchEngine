using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using System.Collections.Generic;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 搜索结果集
    /// </summary>
    public class LuceneSearchResultCollection : ILuceneSearchResultCollection
    {
        /// <summary>
        /// 结果集
        /// </summary>
        public IList<ILuceneSearchResult> Results { get; set; } = new List<ILuceneSearchResult>();

        /// <summary>
        /// 耗时
        /// </summary>
        public long Elapsed { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalHits { get; set; }
    }
}