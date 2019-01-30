namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 索引器选项
    /// </summary>
    public class LuceneIndexerOptions
    {
        /// <summary>
        /// 使用内存目录
        /// </summary>
        public bool UseRamDirectory { get; set; }

        /// <summary>
        /// 索引路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 最大字段数
        /// </summary>
        public int? MaximumFieldLength { get; set; }
    }
}