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
        long Id { get; set; }

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