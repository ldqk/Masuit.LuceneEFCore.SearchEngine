namespace Masuit.LuceneEFCore.SearchEngine.Test.Models
{
    public class User : LuceneIndexableBaseEntity
    {
        [LuceneIndex]
        public string FirstName { get; set; }

        [LuceneIndex]
        public string Surname { get; set; }

        [LuceneIndex]
        public string Email { get; set; }

        [LuceneIndex]
        public string JobTitle { get; set; }

    }
}
