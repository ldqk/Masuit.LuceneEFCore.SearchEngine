using Lucene.Net.Documents;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 需要被索引的实体基类
    /// </summary>
    public abstract class LuceneIndexableBaseEntity : ILuceneIndexable
    {
        /// <summary>
        /// 主键id
        /// </summary>
        [LuceneIndex(Name = nameof(Id), Store = Field.Store.YES), Key]
#if Int
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

#endif
#if Long
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
#endif
#if String
        public string Id { get; set; }
#endif
#if Guid
        public Guid Id { get; set; }
#endif

        /// <summary>
        /// 转换成Lucene文档
        /// </summary>
        /// <returns></returns>
        public virtual Document ToDocument()
        {
            var doc = new Document();
            var type = GetType();
            if (type.Assembly.IsDynamic && type.FullName.Contains("Prox"))
            {
                type = type.BaseType;
            }

            var classProperties = type.GetProperties();
            doc.Add(new StringField("Type", type.AssemblyQualifiedName, Field.Store.YES));
            doc.Add(new StringField("IndexId", type.FullName + Id, Field.Store.YES));
            foreach (var propertyInfo in classProperties)
            {
                var attrs = propertyInfo.GetCustomAttributes<LuceneIndexAttribute>().ToList();
                if (attrs.Count==0)
                {
                    continue;
                }

                var propertyValue = propertyInfo.GetValue(this);
                if (propertyValue == null)
                {
                    continue;
                }

                foreach (var attr in attrs)
                {
                    string name = !string.IsNullOrEmpty(attr.Name) ? attr.Name : propertyInfo.Name;
                    switch (propertyValue)
                    {
                        case DateTime time:
                            doc.Add(new StringField(name, time.ToString("yyyy-MM-dd HH:mm:ss"), attr.Store));
                            break;

                        case int num:
                            doc.Add(new Int32Field(name, num, attr.Store));
                            break;

                        case long num:
                            doc.Add(new Int64Field(name, num, attr.Store));
                            break;

                        case float num:
                            doc.Add(new SingleField(name, num, attr.Store));
                            break;

                        case double num:
                            doc.Add(new DoubleField(name, num, attr.Store));
                            break;

                        case Guid guid:
                            doc.Add(new StringField(name, guid.ToString(), attr.Store));
                            break;

                        default:
                            string value = attr.IsHtml ? propertyValue.ToString().RemoveHtmlTag() : propertyValue.ToString();
                            doc.Add(new TextField(name, value, attr.Store));
                            break;
                    }
                }
            }

            return doc;
        }
    }
}