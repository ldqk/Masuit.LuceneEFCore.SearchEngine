using Masuit.LuceneEFCore.SearchEngine;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSearchDemo.Database
{
    /// <summary>
    /// 文章
    /// </summary>
    [Table("Post")]
    public class Post : LuceneIndexableBaseEntity
    {
        public Post()
        {
            PostDate = DateTime.Now;
        }

        /// <summary>
        /// 标题
        /// </summary>
        [Required(ErrorMessage = "文章标题不能为空！"), LuceneIndex]
        public string Title { get; set; }

        /// <summary>
        /// 作者
        /// </summary>
        [Required, MaxLength(24, ErrorMessage = "作者名最长支持24个字符！"), LuceneIndex]
        public string Author { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        [Required(ErrorMessage = "文章内容不能为空！"), LuceneIndex(IsHtml = true)]
        public string Content { get; set; }

        /// <summary>
        /// 发表时间
        /// </summary>
        public DateTime PostDate { get; set; }

        /// <summary>
        /// 作者邮箱
        /// </summary>
        [Required(ErrorMessage = "作者邮箱不能为空！"), LuceneIndex]
        public string Email { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        [StringLength(256, ErrorMessage = "标签最大允许255个字符"), LuceneIndex]
        public string Label { get; set; }

        /// <summary>
        /// 文章关键词
        /// </summary>
        [StringLength(256, ErrorMessage = "文章关键词最大允许255个字符"), LuceneIndex]
        public string Keyword { get; set; }

    }
}