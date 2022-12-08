using JiebaNet.Segmenter;
using System.Collections.Generic;
using System.Linq;
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

		private static HashSet<(string key, string value)> Pinyins { get; set; } = new();
		private static ILookup<string, string> _pinyinsLookup;

		internal static ILookup<string, string> PinyinsLookup => _pinyinsLookup ??= Pinyins.ToLookup(t => t.key, t => t.value);

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
			var pinyin = PinyinHelper.GetPinyin(Regex.Replace(word, @"[^\u4e00-\u9fa5]", ""));
			if (!string.IsNullOrEmpty(pinyin))
			{
				var key = pinyin.ToLower();
				Pinyins.Add((key.Replace(" ", ""), word));
				Pinyins.Add((new string(key.Split(' ').Select(s => s[0]).ToArray()), word));
			}
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
				var pinyin = PinyinHelper.GetPinyin(Regex.Replace(s, @"[^\u4e00-\u9fa5]", ""));
				if (!string.IsNullOrEmpty(pinyin))
				{
					var key = pinyin.ToLower();
					Pinyins.Add((key.Replace(" ", ""), s));
					Pinyins.Add((new string(key.Split(' ').Select(ss => ss[0]).ToArray()), s));
				}
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
				var pinyin = PinyinHelper.GetPinyin(Regex.Replace(s, @"[^\u4e00-\u9fa5]", ""));
				if (!string.IsNullOrEmpty(pinyin))
				{
					var key = pinyin.ToLower();
					Pinyins.Add((key.Replace(" ", ""), s));
					Pinyins.Add((new string(key.Split(' ').Select(ss => ss[0]).ToArray()), s));
				}
			}
		}
	}
}
