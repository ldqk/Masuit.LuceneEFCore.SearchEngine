using JiebaNet.Segmenter;
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

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pair"></param>
        public static void AddSynonyms(KeyValuePair<string, string> pair)
        {
            SynonymWords.Add((pair.Key, pair.Value));
        }

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pair"></param>
        public static void AddSynonyms((string, string) pair)
        {
            SynonymWords.Add((pair.Item1, pair.Item2));
        }

        /// <summary>
        /// 添加近义词
        /// </summary>
        /// <param name="pair"></param>
        public static void AddSynonyms(IEnumerable<(string key, string value)> pairs)
        {
            foreach (var t in pairs)
            {
                SynonymWords.Add(t);
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
                SynonymWords.Add((pair.Key, pair.Value));
            }
        }

        /// <summary>
        /// 添加关键词
        /// </summary>
        /// <param name="word"></param>
        public static void AddWords(string word)
        {
            new JiebaSegmenter().AddWord(word);
        }

        /// <summary>
        /// 添加关键词
        /// </summary>
        /// <param name="word"></param>
        public static void AddWords(IEnumerable<string> word)
        {
            var js = new JiebaSegmenter();
            foreach (var s in word)
            {
                js.AddWord(s);
                Pinyins.Add((s, PinyinHelper.GetPinyin(Regex.Replace(s, @"\p{P}|\p{S}", ""))));
            }
        }
    }
}
