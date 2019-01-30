using Masuit.LuceneEFCore.SearchEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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
        /// 加速器
        /// </summary>
        private Dictionary<string, float> boosts;

        /// <summary>
        /// 加速器
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
        /// 清除加速器
        /// </summary>
        public void ClearBoosts()
        {
            boosts.Clear();
        }

        /// <summary>
        /// 添加加速器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetBoost(string key, float value)
        {
            boosts[key] = value;
        }

        /// <summary>
        /// 设置加速器
        /// </summary>
        /// <param name="boosts"></param>
        public void SetBoosts(Dictionary<string, float> boosts)
        {
            this.boosts = boosts;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SearchOptions()
        {
            Fields = new List<string>();
            OrderBy = new List<string>();
            MaximumNumberOfHits = 10000;
            boosts = new Dictionary<string, float>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fields">限定检索字段</param>
        /// <param name="maximumNumberOfHits">最大检索量</param>
        /// <param name="boosts">加速器</param>
        /// <param name="type">文档类型</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="skip">跳过多少条</param>
        /// <param name="take">取多少条</param>
        public SearchOptions(string keywords, string fields, int maximumNumberOfHits = 10000, Dictionary<string, float> boosts = null, Type type = null, string orderBy = null, int? skip = null, int? take = null)
        {
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
    }
}