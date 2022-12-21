namespace Masuit.LuceneEFCore.SearchEngine;

/// <summary>
/// JieBaAnalyzer 实例化之前使用
/// </summary>
public static class Settings
{
    /// <summary>
    /// show log
    /// </summary>
    public static bool Log { get; set; } = false;

    /// <summary>
    /// 忽略词典,每行一词
    /// </summary>
    public static string IgnoreDictFile { get; set; }
    /// <summary>
    ///自定义词典,每行一词
    /// </summary>
    public static string UserDictFile { get; set; }
}