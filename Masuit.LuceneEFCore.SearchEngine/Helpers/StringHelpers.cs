using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace Masuit.LuceneEFCore.SearchEngine.Helpers
{
    public static class StringHelpers
    {
        /// <summary>
        /// 移除字符串的指定字符
        /// </summary>
        /// <param name="s"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        public static string RemoveCharacters(this string s, IEnumerable<char> chars)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            return new string(s.Where(c => !chars.Contains(c)).ToArray());
        }

        /// <summary>
        /// 移除html标签
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string RemoveUnwantedTags(this string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return string.Empty;
            }

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));

            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;

                if (node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");

                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }

                    parentNode.RemoveChild(node);
                }
            }

            return document.DocumentNode.InnerHtml;
        }
    }
}