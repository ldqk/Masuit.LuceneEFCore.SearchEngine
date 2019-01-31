using Lucene.Net.Documents;
using System;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 标记该字段可被索引
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LuceneIndexAttribute : Attribute
    {
        public LuceneIndexAttribute()
        {
            Index = Field.Index.ANALYZED;
            Store = Field.Store.YES;
            IsHtml = false;
        }

        /// <summary>
        /// 索引字段名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 索引行为
        /// </summary>
        public Field.Index Index { get; set; }

        /// <summary>
        /// 是否被存储到索引库
        /// </summary>
        public Field.Store Store { get; set; }

        /// <summary>
        /// 是否是html
        /// </summary>
        public bool IsHtml { get; set; }
    }
}