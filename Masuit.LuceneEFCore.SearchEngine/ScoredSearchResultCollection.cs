using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using System.Collections.Generic;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 搜索结果集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScoredSearchResultCollection<T> : IScoredSearchResultCollection<T>
    {
        /// <summary>
        /// 结果集
        /// </summary>
        public IList<IScoredSearchResult<T>> Results { get; set; } = new List<IScoredSearchResult<T>>();

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