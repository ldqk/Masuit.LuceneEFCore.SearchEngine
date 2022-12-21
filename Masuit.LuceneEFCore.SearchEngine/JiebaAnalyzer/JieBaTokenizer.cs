using JiebaNet.Segmenter;
using JiebaNet.Segmenter.Common;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Token = JiebaNet.Segmenter.Token;

namespace Masuit.LuceneEFCore.SearchEngine;

public class JieBaTokenizer : Tokenizer
{
    private string _inputText;
    private readonly string _dictPath = "Resources/dict.txt";

    private readonly JiebaSegmenter _segmenter;
    private TokenizerMode _mode;
    private ICharTermAttribute _termAtt;
    private IOffsetAttribute _offsetAtt;
    //private IPositionIncrementAttribute _posIncrAtt;
    private ITypeAttribute _typeAtt;
    private readonly List<Token> _wordList = new List<Token>();

    private IEnumerator<Token> _iter;

    public List<string> StopWords { get; } = new List<string>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <param name="mode"></param>
    /// <param name="defaultUserDict">使用内置词库</param>
    public JieBaTokenizer(TextReader input, TokenizerMode mode, bool defaultUserDict = false) : base(AttributeFactory.DEFAULT_ATTRIBUTE_FACTORY, input)
    {
        _segmenter = new JiebaSegmenter();
        _mode = mode;
        if (defaultUserDict)
        {
            _segmenter.LoadUserDictForEmbedded(Assembly.GetCallingAssembly(), _dictPath);
        }

        if (!string.IsNullOrEmpty(Settings.IgnoreDictFile))
        {
            var list = FileExtension.ReadAllLines(Settings.IgnoreDictFile);
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (StopWords.Contains(item))
                    continue;
                StopWords.Add(item);
            }
        }

        if (!string.IsNullOrEmpty(Settings.UserDictFile))
        {
            _segmenter.LoadUserDict(Settings.UserDictFile);
        }

        Init();
    }

    #region private func
    private void Init()
    {
        _termAtt = AddAttribute<ICharTermAttribute>();
        _offsetAtt = AddAttribute<IOffsetAttribute>();
        //_posIncrAtt = AddAttribute<IPositionIncrementAttribute>();
        _typeAtt = AddAttribute<ITypeAttribute>();
        AddAttribute<IPositionIncrementAttribute>();
    }

    private string ReadToEnd(TextReader input)
    {
        return input.ReadToEnd();
    }


    private Lucene.Net.Analysis.Token Next()
    {
        var res = _iter.MoveNext();
        if (res)
        {
            var word = _iter.Current;
            var token = new Lucene.Net.Analysis.Token(word.Word, word.StartIndex, word.EndIndex);
            if (Settings.Log)
            {
                //chinese char
                var zh = new Regex(@"[\u4e00-\u9fa5]|[^\x00-\xff]");
                var offset = zh.Matches(word.Word).Count;
                var len = 10;
                offset = offset > len ? 0 : offset;
                Console.WriteLine($"==分词：{word.Word.PadRight(len - offset, '=')}==起始位置：{word.StartIndex.ToString().PadLeft(3, '=')}==结束位置{word.EndIndex.ToString().PadLeft(3, '=')}");
            }
            return token;
        }
        return null;
    }
    #endregion

    public sealed override bool IncrementToken()
    {
        ClearAttributes();

        var word = Next();
        if (word != null)
        {
            var buffer = word.ToString();
            _termAtt.SetEmpty().Append(buffer);
            _offsetAtt.SetOffset(CorrectOffset(word.StartOffset), CorrectOffset(word.EndOffset));
            _typeAtt.Type = word.Type;
            return true;
        }

        End();
        Dispose();
        return false;
    }


    public override void Reset()
    {
        base.Reset();

        _inputText = ReadToEnd(m_input);
        RemoveStopWords(_segmenter.Tokenize(_inputText, _mode));

        _iter = _wordList.GetEnumerator();
    }

    private void RemoveStopWords(IEnumerable<Token> words)
    {
        _wordList.Clear();

        foreach (var x in words)
        {
            if (!StopWords.Contains(x.Word))
            {
                _wordList.Add(x);
            }
        }
    }
}