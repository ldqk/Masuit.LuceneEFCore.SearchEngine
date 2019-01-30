using Lucene.Net.Documents;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    /// <summary>
    /// 搜索结果
    /// </summary>
    public interface ILuceneSearchResult
    {
        /// <summary>
        /// 匹配度
        /// </summary>
        float Score { get; set; }

        /// <summary>
        /// 文档
        /// </summary>
        Document Document { get; set; }
    }
}