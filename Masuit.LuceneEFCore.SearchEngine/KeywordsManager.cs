using JiebaNet.Segmenter;
using Lucene.Net.Analysis.JieBa;
using Lucene.Net.Analysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TinyPinyin;

namespace Masuit.LuceneEFCore.SearchEngine
{
    public static class KeywordsManager
    {
        /// <summary>
        /// 近义词组
        /// </summary>
        internal static HashSet<(string key, string value)> SynonymWords { get; set; } = new();

        internal static HashSet<(string key, string value)> Pinyins { get; set; } = new();

        private static readonly JiebaSegmenter JiebaSegmenter = new();

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pair"></param>
        public static void AddSynonyms(KeyValuePair<string, string> pair)
        {
            SynonymWords.Add((pair.Key, pair.Value));
            AddWords(pair.Key, pair.Value);
        }

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pair"></param>
        public static void AddSynonyms((string, string) pair)
        {
            SynonymWords.Add((pair.Item1, pair.Item2));
            AddWords(pair.Item1, pair.Item2);
        }

        /// <summary>
        /// 添加近义词
        /// </summary>
        public static void AddSynonyms(string key, string value, params string[] values)
        {
            SynonymWords.Add((key, value));
            AddWords(key, value);
            foreach (var s in values)
            {
                SynonymWords.Add((key, s));
                AddWords(s);
            }
        }

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pairs"></param>
        public static void AddSynonyms(IEnumerable<(string key, string value)> pairs)
        {
            foreach (var t in pairs)
            {
                SynonymWords.Add(t);
                AddWords(t.key, t.value);
            }
        }

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pairs"></param>
        public static void AddSynonyms(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                AddWords(pair.Key, pair.Value);
                SynonymWords.Add((pair.Key, pair.Value));
            }
        }

        /// <summary>
        /// 添加关键词
        /// </summary>
        /// <param name="word"></param>
        public static void AddWords(string word)
        {
            JiebaSegmenter.AddWord(word);
            Pinyins.Add((word, PinyinHelper.GetPinyin(Regex.Replace(word, @"\p{P}|\p{S}", ""))));
        }

        /// <summary>
        /// 添加关键词
        /// </summary>
        /// <param name="words"></param>
        public static void AddWords(IEnumerable<string> words)
        {
            foreach (var s in words)
            {
                JiebaSegmenter.AddWord(s);
                Pinyins.Add((s, PinyinHelper.GetPinyin(Regex.Replace(s, @"\p{P}|\p{S}", ""))));
            }
        }

        /// <summary>
        /// 添加关键词
        /// </summary>
        /// <param name="word"></param>
        /// <param name="words"></param>
        public static void AddWords(string word, params string[] words)
        {
            JiebaSegmenter.AddWord(word);
            foreach (var s in words)
            {
                JiebaSegmenter.AddWord(s);
                Pinyins.Add((s, PinyinHelper.GetPinyin(Regex.Replace(s, @"\p{P}|\p{S}", ""))));
            }
        }
    }
}
