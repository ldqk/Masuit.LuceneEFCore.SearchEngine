namespace Masuit.LuceneEFCore.SearchEngine.Test.Models
{
    public class User : LuceneIndexableBaseEntity
    {
        [LuceneIndexable]
        public string FirstName { get; set; }

        [LuceneIndexable]
        public string Surname { get; set; }

        [LuceneIndexable]
        public string Email { get; set; }

        [LuceneIndexable]
        public string JobTitle { get; set; }

    }
}
