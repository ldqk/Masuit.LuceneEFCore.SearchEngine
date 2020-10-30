using Lucene.Net.Documents;
using System;

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
        public static object Get(this Document doc, string key, Type t)
        {
            string value = doc.Get(key);
            return t switch
            {
                _ when t.IsAssignableFrom(typeof(string)) => value,
                _ when t.IsAssignableFrom(typeof(int)) => int.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(long)) => long.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(double)) => double.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(float)) => float.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(decimal)) => decimal.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(char)) => char.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(byte)) => byte.TryParse(value, out var v) ? v : 0,
                _ when t.IsAssignableFrom(typeof(bool)) => bool.TryParse(value, out var v) && v,
                _ when t.IsAssignableFrom(typeof(DateTime)) => DateTime.TryParse(value, out var v) ? v : throw new Exception(value + "日期时间格式不正确"),
                _ when t.BaseType == typeof(Enum) => Enum.Parse(t, value),
                _ => Convert.ChangeType(value, t)
            };
        }

        /// <summary>
        /// 获取文档的值
        /// </summary>
        /// <param name="doc">Lucene文档</param>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static object Get<T>(this Document doc, string key)
        {
            string value = doc.Get(key);
            return typeof(T) switch
            {
                _ when typeof(T).IsAssignableFrom(typeof(string)) => value,
                _ when typeof(T).IsAssignableFrom(typeof(int)) => int.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(long)) => long.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(double)) => double.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(float)) => float.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(decimal)) => decimal.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(char)) => char.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(byte)) => byte.TryParse(value, out var v) ? v : 0,
                _ when typeof(T).IsAssignableFrom(typeof(bool)) => bool.TryParse(value, out var v) && v,
                _ when typeof(T).IsAssignableFrom(typeof(DateTime)) => DateTime.TryParse(value, out var v) ? v : throw new Exception(value + "日期时间格式不正确"),
                _ when typeof(T).BaseType == typeof(Enum) => Enum.Parse(typeof(T), value),
                _ => Convert.ChangeType(value, typeof(T))
            };
        }
    }
}