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
        internal static object Get(this Document doc, string key, Type t)
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
                _ when t.IsAssignableFrom(typeof(DateTime)) => DateTime.Parse(value),
                _ when t.BaseType == typeof(Enum) => Enum.Parse(t, value),
                _ => Convert.ChangeType(value, t)
            };
        }
    }
}