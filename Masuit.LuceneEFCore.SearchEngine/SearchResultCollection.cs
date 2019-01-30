using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using System.Collections.Generic;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 搜索结果集
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchResultCollection<T> : ISearchResultCollection<T>
    {
        /// <summary>
        /// 实体集
        /// </summary>
        public IList<T> Results { get; set; }

        /// <summary>
        /// 耗时
        /// </summary>
        public long Elapsed { get; set; }

        /// <summary>
        /// 总条数
        /// </summary>
        public int TotalHits { get; set; }

        public SearchResultCollection()
        {
            Results = new List<T>();
        }
    }
}