using Masuit.LuceneEFCore.SearchEngine.Interfaces;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 索引修改实体
    /// </summary>
    public class LuceneIndexChange
    {
        /// <summary>
        /// 实体类
        /// </summary>
        public ILuceneIndexable Entity { get; set; }

        /// <summary>
        /// 变更状态
        /// </summary>
        public LuceneIndexState State { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entity">实体</param>
        public LuceneIndexChange(ILuceneIndexable entity)
        {
            Entity = entity;
            State = LuceneIndexState.NotSet;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="state">变更状态</param>
        public LuceneIndexChange(ILuceneIndexable entity, LuceneIndexState state)
        {
            Entity = entity;
            State = state;
        }
    }
}