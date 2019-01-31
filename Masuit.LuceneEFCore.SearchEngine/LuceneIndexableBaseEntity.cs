using Lucene.Net.Documents;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [LuceneIndex(Name = "Id", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED), Key]
        public int Id { get; set; }

        /// <summary>
        /// 索引唯一id
        /// </summary>
        [LuceneIndex(Name = "IndexId", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED)]
        [NotMapped]
        public string IndexId
        {
            get => GetType().Name + ":" + Id;
            set
            {
            }
        }

        /// <summary>
        /// 转换成Lucene文档
        /// </summary>
        /// <returns></returns>
        public virtual Document ToDocument()
        {
            Document doc = new Document();
            Type type = GetType();
            if (type.Assembly.IsDynamic && type.FullName.Contains("Prox"))
            {
                type = type.BaseType;
            }
            PropertyInfo[] classProperties = type.GetProperties();
            doc.Add(new Field("Type", type.AssemblyQualifiedName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            foreach (PropertyInfo propertyInfo in classProperties)
            {
                object propertyValue = propertyInfo.GetValue(this);
                if (propertyValue != null)
                {
                    IEnumerable<LuceneIndexAttribute> attrs = propertyInfo.GetCustomAttributes<LuceneIndexAttribute>();
                    foreach (LuceneIndexAttribute attr in attrs)
                    {
                        string name = !string.IsNullOrEmpty(attr.Name) ? attr.Name : propertyInfo.Name;
                        string value = attr.IsHtml ? StringHelpers.RemoveUnwantedTags(propertyValue.ToString()) : propertyValue.ToString();
                        doc.Add(new Field(name, value, attr.Store, attr.Index));
                    }
                }
            }

            return doc;
        }
    }
}