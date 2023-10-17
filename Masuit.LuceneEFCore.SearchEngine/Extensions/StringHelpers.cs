using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Masuit.LuceneEFCore.SearchEngine.Extensions
{
	public static class StringHelpers
	{
		/// <summary>
		/// 移除字符串的指定字符
		/// </summary>
		/// <param name="s"></param>
		/// <param name="chars"></param>
		/// <returns></returns>
		internal static string RemoveCharacters(this string s, IEnumerable<char> chars)
		{
			return string.IsNullOrEmpty(s) ? string.Empty : new string(s.Where(c => !chars.Contains(c)).ToArray());
		}

		/// <summary>
		/// 去除html标签后并截取字符串
		/// </summary>
		/// <param name="html">源html</param>
		/// <returns></returns>
		internal static string RemoveHtmlTag(this string html)
		{
			var strText = Regex.Replace(html, "<[^>]+>", "");
			strText = Regex.Replace(strText, "&[^;]+;", "");
			return strText;
		}

		/// <summary>
		/// 添加多个元素
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="this"></param>
		/// <param name="values"></param>
		public static void AddRange<T>(this ICollection<T> @this, IEnumerable<T> values)
		{
			foreach (var obj in values)
			{
				@this.Add(obj);
			}
		}

		/// <summary>
		/// 移除符合条件的元素
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="this"></param>
		/// <param name="where"></param>
		public static void RemoveWhere<T>(this ICollection<T> @this, Func<T, bool> @where)
		{
			foreach (var obj in @this.Where(where).ToList())
			{
				@this.Remove(obj);
			}
		}
	}
}
