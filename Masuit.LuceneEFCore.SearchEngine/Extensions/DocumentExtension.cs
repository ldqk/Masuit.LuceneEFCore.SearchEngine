﻿using Lucene.Net.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Masuit.LuceneEFCore.SearchEngine.Extensions
{
	public static class DocumentExtension
	{
		/// <summary>
		/// 获取文档的值
		/// </summary>
		/// <param name="doc">Lucene文档</param>
		/// <param name="key">键</param>
		/// <param name="t">类型</param>
		/// <returns></returns>
		internal static object Get(this Document doc, string key, Type t)
		{
			string value = doc.Get(key);
			return t switch
			{
				_ when t.IsAssignableFrom(typeof(string)) => value,
				_ when t.IsValueType => ConvertTo(value, t),
				_ => JsonConvert.DeserializeObject(value, t)
			};
		}

		/// <summary>
		/// 类型直转
		/// </summary>
		/// <param name="value"></param>
		/// <param name="type">目标类型</param>
		/// <returns></returns>
		private static object ConvertTo(string value, Type type)
		{
			if (value == null)
			{
				return default;
			}

			if (value.GetType() == type)
			{
				return value;
			}

			if (type.IsEnum)
			{
				return Enum.Parse(type, value.ToString(CultureInfo.InvariantCulture));
			}

			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				var underlyingType = Nullable.GetUnderlyingType(type);
				return underlyingType!.IsEnum ? Enum.Parse(underlyingType, value.ToString(CultureInfo.CurrentCulture)) : Convert.ChangeType(value, underlyingType);
			}

			var converter = TypeDescriptor.GetConverter(value);
			if (converter != null)
			{
				if (converter.CanConvertTo(type))
				{
					return converter.ConvertTo(value, type);
				}
			}

			converter = TypeDescriptor.GetConverter(type);
			if (converter != null)
			{
				if (converter.CanConvertFrom(value.GetType()))
				{
					return converter.ConvertFrom(value);
				}
			}

			return Convert.ChangeType(value, type);
		}

#if NET6_0_OR_GREATER
#else

		/// <summary>
		/// 按字段去重
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="source"></param>
		/// <param name="keySelector"></param>
		/// <returns></returns>
		public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));
			var set = new HashSet<TKey>();
			return source.Where(item => set.Add(keySelector(item)));
		}

#endif
	}
}