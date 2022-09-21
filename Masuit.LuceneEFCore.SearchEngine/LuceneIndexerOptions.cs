using System;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 索引器选项
    /// </summary>
    public class LuceneIndexerOptions
    {
        /// <summary>
        /// 索引路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 索引列IndexId的生成函数，(Type EntityType, any IdValue) => string IndexId
        /// </summary>
        public static Func<Type, object, string> IndexIdGenerator = (type, id) => $"{type.Name}:{id}";

    }
}