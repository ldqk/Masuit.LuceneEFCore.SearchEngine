namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 索引状态枚举
    /// </summary>
    public enum LuceneIndexState
    {
        /// <summary>
        /// 已添加
        /// </summary>
        Added,

        /// <summary>
        /// 被删除
        /// </summary>
        Removed,

        /// <summary>
        /// 被更新
        /// </summary>
        Updated,

        /// <summary>
        /// 未作修改
        /// </summary>
        Unchanged,

        /// <summary>
        /// 不需要修改
        /// </summary>
        NotSet
    }
}