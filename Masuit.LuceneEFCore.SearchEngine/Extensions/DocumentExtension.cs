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
            switch (t)
            {
                case Type _ when t.IsAssignableFrom(typeof(int)):
                    return int.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(DateTime)):
                    return DateTime.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(double)):
                    return double.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(long)):
                    return long.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(decimal)):
                    return decimal.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(char)):
                    return char.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(byte)):
                    return byte.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(float)):
                    return float.Parse(value);
                case Type _ when t.IsAssignableFrom(typeof(bool)):
                    return bool.Parse(value);
                case Type _ when t.BaseType == typeof(Enum):
                    return Enum.Parse(t, value);
                case Type _ when t.IsAssignableFrom(typeof(string)):
                    return value;
                default:
                    return Convert.ChangeType(value, t);
            }
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
            switch (typeof(T))
            {
                case Type _ when typeof(T).IsAssignableFrom(typeof(int)):
                    return int.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(DateTime)):
                    return DateTime.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(double)):
                    return double.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(long)):
                    return long.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(decimal)):
                    return decimal.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(char)):
                    return char.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(byte)):
                    return byte.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(float)):
                    return float.Parse(value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(bool)):
                    return bool.Parse(value);
                case Type _ when typeof(T).BaseType == typeof(Enum):
                    return Enum.Parse(typeof(T), value);
                case Type _ when typeof(T).IsAssignableFrom(typeof(string)):
                    return value;
                default:
                    return Convert.ChangeType(value, typeof(T));
            }
        }
    }
}