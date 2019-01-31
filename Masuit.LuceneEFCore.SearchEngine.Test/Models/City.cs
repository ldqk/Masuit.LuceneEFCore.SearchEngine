namespace Masuit.LuceneEFCore.SearchEngine.Test.Models
{
    public class City : LuceneIndexableBaseEntity
    {
        [LuceneIndex]
        public string Name { get; set; }

        public string Code { get; set; }

        [LuceneIndex]
        public string Country { get; set; }

    }
}
