namespace Masuit.LuceneEFCore.SearchEngine.Test.Models
{
    public class City : LuceneIndexableBaseEntity
    {
        [LuceneIndexable]
        public string Name { get; set; }

        public string Code { get; set; }

        [LuceneIndexable]
        public string Country { get; set; }

    }
}
