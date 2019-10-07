using System.Collections.Generic;
using System.Linq;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 索引变更集
    /// </summary>
    public class LuceneIndexChangeset
    {
        /// <summary>
        /// 实体集
        /// </summary>
        public IList<LuceneIndexChange> Entries { get; set; }

        /// <summary>
        /// 实体是否有某种状态
        /// </summary>
        /// <param name="state">状态</param>
        /// <returns></returns>
        private bool EntriesHaveState(LuceneIndexState state)
        {
            return Entries.Any(x => x.State == state);
        }

        /// <summary>
        /// 已经被添加？
        /// </summary>
        public bool HasAdds => EntriesHaveState(LuceneIndexState.Added);

        /// <summary>
        /// 已经被更新？
        /// </summary>
        public bool HasUpdates => EntriesHaveState(LuceneIndexState.Updated);

        /// <summary>
        /// 已经被删除？
        /// </summary>
        public bool HasDeletes => EntriesHaveState(LuceneIndexState.Removed);

        /// <summary>
        /// 已经被修改
        /// </summary>
        public bool HasChanges => Entries.Any() && (HasAdds || HasUpdates || HasDeletes);

        /// <summary>
        /// 构造函数
        /// </summary>
        public LuceneIndexChangeset()
        {
            Entries = new List<LuceneIndexChange>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="change">被修改的实体</param>
        public LuceneIndexChangeset(LuceneIndexChange change) => Entries = new List<LuceneIndexChange>
        {
            change
        };
    }
}