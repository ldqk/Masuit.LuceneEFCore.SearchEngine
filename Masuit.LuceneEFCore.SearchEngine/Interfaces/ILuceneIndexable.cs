#if Guid
using System; 
#endif
using Lucene.Net.Documents;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    /// <summary>
    /// 需要被索引的实体基类
    /// </summary>
    public interface ILuceneIndexable
    {
        /// <summary>
        /// 主键id
        /// </summary>
#if Int
        int Id { get; set; }
#endif
#if Long
        long Id { get; set; } 
#endif
#if String
        string Id { get; set; } 
#endif
#if Guid
        Guid Id { get; set; } 
#endif

        /// <summary>
        /// 索引id
        /// </summary>
        string IndexId { get; set; }

        /// <summary>
        /// 转换成Lucene文档
        /// </summary>
        /// <returns></returns>
        Document ToDocument();
    }
}