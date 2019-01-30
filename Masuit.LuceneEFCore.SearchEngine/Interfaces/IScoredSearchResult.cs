namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    /// <summary>
    /// 结果项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IScoredSearchResult<T>
    {
        /// <summary>
        /// 匹配度
        /// </summary>
        float Score { get; set; }

        /// <summary>
        /// 实体
        /// </summary>
        T Entity { get; set; }
    }
}