using Lucene.Net.Documents;
using Masuit.LuceneEFCore.SearchEngine.Extensions;
using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Newtonsoft.Json;
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
        /// 索引唯一id
        /// </summary>
        [LuceneIndex(Name = "IndexId", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED)]
        [NotMapped, JsonIgnore]
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
            var doc = new Document();
            var type = GetType();
            if (type.Assembly.IsDynamic && type.FullName.Contains("Prox"))
            {
                type = type.BaseType;
            }

            var classProperties = type.GetProperties();
            doc.Add(new Field("Type", type.AssemblyQualifiedName, Field.Store.YES, Field.Index.NOT_ANALYZED));
            foreach (var propertyInfo in classProperties)
            {
                var propertyValue = propertyInfo.GetValue(this);
                if (propertyValue == null)
                {
                    continue;
                }

                var attrs = propertyInfo.GetCustomAttributes<LuceneIndexAttribute>();
                foreach (var attr in attrs)
                {
                    string name = !string.IsNullOrEmpty(attr.Name) ? attr.Name : propertyInfo.Name;
                    string value = attr.IsHtml ? propertyValue.ToString().RemoveHtmlTag() : propertyValue.ToString();
                    doc.Add(new Field(name, value, attr.Store, attr.Index));
                }
            }

            return doc;
        }
    }
}