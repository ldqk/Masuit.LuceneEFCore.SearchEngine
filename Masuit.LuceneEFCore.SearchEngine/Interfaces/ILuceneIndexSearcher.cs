using Lucene.Net.Documents;
using System;
using System.Collections.Generic;

namespace Masuit.LuceneEFCore.SearchEngine.Interfaces
{
    /// <summary>
    /// 搜索引擎
    /// </summary>
    public interface ILuceneIndexSearcher
    {
        /// <summary>
        /// 分词
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        List<string> CutKeywords(string keyword);

        /// <summary>
        /// 搜索单条记录
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Document ScoredSearchSingle(SearchOptions options);

        /// <summary>
        /// 按权重搜索
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        ILuceneSearchResultCollection ScoredSearch(SearchOptions options);

        /// <summary>
        /// 按权重搜索
        /// </summary>
        /// <param name="keywords">关键词</param>
        /// <param name="fields">限定检索字段</param>
        /// <param name="maximumNumberOfHits">最大检索量</param>
        /// <param name="boosts">多字段搜索时，给字段的搜索加速</param>
        /// <param name="type">文档类型</param>
        /// <param name="sortBy">排序字段</param>
        /// <param name="skip">跳过多少条</param>
        /// <param name="take">取多少条</param>
        /// <returns></returns>
        ILuceneSearchResultCollection ScoredSearch(string keywords, string fields, int maximumNumberOfHits, Dictionary<string, float> boosts, Type type, string sortBy, int? skip, int? take);
    }
}