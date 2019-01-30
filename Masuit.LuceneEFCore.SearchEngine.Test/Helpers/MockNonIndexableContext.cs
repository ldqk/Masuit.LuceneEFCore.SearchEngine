using Masuit.LuceneEFCore.SearchEngine.Test.Models;
using Microsoft.EntityFrameworkCore;

namespace Masuit.LuceneEFCore.SearchEngine.Test.Helpers
{
    public class MockNonIndexableContext : DbContext
    {
        public virtual DbSet<NonIndexable> NonIndexables { get; set; }
    }
}