using Masuit.LuceneEFCore.SearchEngine.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Masuit.LuceneEFCore.SearchEngine
{
    /// <summary>
    /// 搜索选项
    /// </summary>
    public class SearchOptions
    {
        /// <summary>
        /// 关键词
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// 限定搜索字段
        /// </summary>
        public List<string> Fields { get; set; }

        /// <summary>
        /// 最大检索量
        /// </summary>
        public int MaximumNumberOfHits { get; set; }

        /// <summary>
        /// 多字段搜索时，给字段的搜索加速
        /// </summary>
        private Dictionary<string, float> boosts;

        /// <summary>
        /// 多字段搜索时，给字段的搜索加速
        /// </summary>
        public Dictionary<string, float> Boosts
        {
            get
            {
                foreach (var field in Fields)
                {
                    if (boosts.All(x => x.Key.ToUpper() != field.ToUpper()))
                    {
                        boosts.Add(field, 1.0f);
                    }
                }

                return boosts;
            }
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public List<string> OrderBy { get; set; }

        /// <summary>
        /// 跳过多少条
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// 取多少条
        /// </summary>
        public int? Take { get; set; }

        /// <summary>
        /// 文档类型
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 清除多字段搜索时，给字段的搜索加速
        /// </summary>
        public void ClearBoosts()
        {
            boosts.Clear();
        }

        /// <summary>
        /// 添加多字段搜索时，给字段的搜索加速
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBoost(string key, float value)
        {
            boosts[key] = value;
        }

        /// <summary>
        /// 设置多字段搜索时，给字段的搜索加速
        /// </summary>
        /// <param name="boosts"></param>
        public void SetBoosts(Dictionary<string, float> boosts)
        {
            this.boosts = boosts;
        }

        /// <summary>
        /// 搜索选项
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fields">限定检索字段</param>
        /// <param name="maximumNumberOfHits">最大检索量</param>
        /// <param name="boosts">多字段搜索时，给字段的搜索加速</param>
        /// <param name="type">文档类型</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="skip">跳过多少条</param>
        /// <param name="take">取多少条</param>
        public SearchOptions(string keywords, string fields, int maximumNumberOfHits = 10000, Dictionary<string, float> boosts = null, Type type = null, string orderBy = null, int? skip = null, int? take = null)
        {
            if (string.IsNullOrWhiteSpace(keywords))
            {
                throw new ArgumentException("搜索关键词不能为空！");
            }
            Keywords = keywords;
            MaximumNumberOfHits = maximumNumberOfHits;
            Skip = skip;
            Take = take;
            this.boosts = boosts ?? new Dictionary<string, float>();

            Type = type;

            Fields = new List<string>();
            OrderBy = new List<string>();

            // 添加被检索字段
            if (!string.IsNullOrEmpty(fields))
            {
                fields = fields.RemoveCharacters(" ");
                Fields.AddRange(fields.Split(',').ToList());
            }

            // 添加排序规则
            if (!string.IsNullOrEmpty(orderBy))
            {
                orderBy = orderBy.RemoveCharacters(" ");
                OrderBy.AddRange(orderBy.Split(',').ToList());
            }
        }

        /// <summary>
        /// 搜索选项
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="size">页大小</param>
        /// <param name="fields">限定检索字段</param>
        /// <param name="page">第几页</param>
        public SearchOptions(string keywords, int page, int size, string fields) : this(keywords, fields, int.MaxValue, null, null, null, (page - 1) * size, size)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (size < 1)
            {
                size = 1;
            }
            Skip = (page - 1) * size;
            Take = size;
        }

        /// <summary>
        /// 搜索选项
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="size">页大小</param>
        /// <param name="page">第几页</param>
        /// <param name="t">需要被全文检索的类型</param>
        public SearchOptions(string keywords, int page, int size, Type t) : this(keywords, string.Join(",", t.GetProperties().Where(p => p.GetCustomAttributes<LuceneIndexAttribute>().Any()).Select(p => p.Name)), int.MaxValue, null, null, null, (page - 1) * size, size)
        {
            if (page < 1)
            {
                page = 1;
            }
            if (size < 1)
            {
                size = 1;
            }
            Skip = (page - 1) * size;
            Take = size;
        }
    }
}