using Masuit.LuceneEFCore.SearchEngine.Test.Models;
using Microsoft.EntityFrameworkCore;

namespace Masuit.LuceneEFCore.SearchEngine.Test.Helpers
{
    public class MockContext : DbContext
    {
        public virtual DbSet<User> Users { get; set; }
    }
}