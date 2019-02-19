#if Guid
using System; 
#endif
using Lucene.Net.Documents;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [LuceneIndex(Name = "Id", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED), Key]
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
        [LuceneIndex(Name = "IndexId", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED)]
        [JsonIgnore, NotMapped]
        string IndexId { get; set; }

        /// <summary>
        /// 转换成Lucene文档
        /// </summary>
        /// <returns></returns>
        Document ToDocument();
    }
}