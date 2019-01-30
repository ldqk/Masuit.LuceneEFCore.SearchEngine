using Masuit.LuceneEFCore.SearchEngine.Interfaces;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 搜索结果项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScoredSearchResult<T> : IScoredSearchResult<T>
    {
        /// <summary>
        /// 匹配度
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// 物理实体
        /// </summary>
        public T Entity { get; set; }
    }
}