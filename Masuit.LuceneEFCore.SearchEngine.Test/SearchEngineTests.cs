using Masuit.LuceneEFCore.SearchEngine.Interfaces;
using Masuit.LuceneEFCore.SearchEngine.Test.Helpers;
using Masuit.LuceneEFCore.SearchEngine.Test.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Masuit.LuceneEFCore.SearchEngine.Test
{
    [Trait("Category", "SearchContext")]
    public class SearchEngineTests
    {
        private TestDbContext _context;
        private readonly ITestOutputHelper _output;
        string _tempName;

        public SearchEngineTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private void InitializeContext()
        {
            _tempName = System.IO.Path.GetTempFileName();
            var dboptions = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase(databaseName: _tempName).Options;
            _context = new TestDbContext(dboptions);
        }

        [Fact]
        public void AContextProviderCanIndexADatabase()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));

            searchProvider.CreateIndex();

            Assert.Equal(2000, searchProvider.IndexCount);
            searchProvider.DeleteIndex();
        }

        [Fact]
        public void AContextCanBeSearchedUsingAContextProvider()
        {
            InitializeContext();

            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            SearchOptions searchOptions = new SearchOptions("John", "FirstName");

            var results = searchProvider.ScoredSearch<User>(searchOptions);

            Assert.Equal(5, results.TotalHits);
            searchProvider.DeleteIndex();
        }

        [Fact]
        public void SkipAndTakeWorkWhenSearchingUsingAContextProvider()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            SearchOptions searchOptions = new SearchOptions("John", "FirstName");

            var initialResults = searchProvider.ScoredSearch<User>(searchOptions);
            var lastId = initialResults.Results[4].Entity.Id;

            Assert.Equal(5, initialResults.TotalHits);
            Assert.Equal(5, initialResults.Results.Count);

            searchOptions.Skip = 4;
            searchOptions.Take = 1;
            var subResults = searchProvider.ScoredSearch<User>(searchOptions);

            Assert.Equal(5, subResults.TotalHits);
            Assert.Equal(1, subResults.Results.Count);
            Assert.Equal(lastId, subResults.Results.First().Entity.Id);

            searchProvider.DeleteIndex();
        }

        [Fact]
        public void AContextCanBeSearchedUsingAWildCard()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            SearchOptions searchOptions = new SearchOptions("Joh*", "FirstName");

            // test
            var results = searchProvider.ScoredSearch<User>(searchOptions);
            PrintResult(results);

            Assert.Equal(10, results.TotalHits);

            searchProvider.DeleteIndex();
        }


        [Fact]
        public void ASearchWillReturnTheSameResultsAsAScoredSearch()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            SearchOptions searchOptions = new SearchOptions("Joh*", "FirstName");

            // test
            var results = searchProvider.Search<User>(searchOptions);

            Assert.Equal(10, results.TotalHits);

            searchProvider.DeleteIndex();
        }

        [Fact]
        public void AScoredSearchWillOrderByRelevence()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            SearchOptions searchOptions = new SearchOptions("Burns", "FirstName,Surname");

            var results = searchProvider.ScoredSearch<User>(searchOptions);
            var first = results.Results.First().Entity;
            var highest = results.Results.First().Score;
            var lowest = results.Results.Last().Score;

            Assert.True(highest > lowest);
            Assert.Equal("Jeremy", first.FirstName);
            Assert.Equal("Burns", first.Surname);

            searchProvider.DeleteIndex();
        }

        [Fact]
        public void ASearchWillStillOrderByRelevence()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            SearchOptions searchOptions = new SearchOptions("Jeremy Burns", "FirstName,Surname");

            var results = searchProvider.Search<User>(searchOptions);
            var first = results.Results.First();

            Assert.Equal("Jeremy", first.FirstName);
            Assert.Equal("Burns", first.Surname);

            searchProvider.DeleteIndex();
        }

        [Fact]
        public void ASearchCanOrderByMultipleFields()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));

            User jc = new User()
            {
                FirstName = "John",
                Surname = "Chapman",
                JobTitle = "Test Engineer",
                Email = "john.chapman@test.com"
            };
            _context.Users.Add(jc);
            _context.SaveChanges();
            searchProvider.CreateIndex();
            SearchOptions search = new SearchOptions("John", "FirstName", 1000, null, null, "Surname,JobTitle");

            var results = searchProvider.ScoredSearch<User>(search);
            var topResult = results.Results[0];
            var secondResult = results.Results[1];

            Assert.Equal("Sales Associate", topResult.Entity.JobTitle);
            Assert.Equal("Test Engineer", secondResult.Entity.JobTitle);

            searchProvider.DeleteIndex();
        }

        [Fact]
        public void SaveChangesUpdatesEntitiesAddedToTheIndex()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));
            searchProvider.CreateIndex();
            var newUser = new User()
            {
                FirstName = "Duke",
                Surname = "Nukem",
                Email = "duke.nukem@test.com",
                JobTitle = "Shooty Man"
            };
            var search = new SearchOptions("Nukem", "Surname");

            var initialResults = searchProvider.Search<User>(search);
            searchProvider.Context.Users.Add(newUser);
            searchProvider.SaveChanges();
            var newResults = searchProvider.Search<User>(search);

            Assert.Equal(0, initialResults.TotalHits);
            Assert.Equal(1, newResults.TotalHits);
            Assert.Equal(newUser.Id, newResults.Results[0].Id);
        }

        [Fact]
        public void NonValidEntitiesAreIgnored()
        {
            InitializeContext();
            LuceneIndexerOptions options = new LuceneIndexerOptions()
            {
                Path = "lucene"
            };
            SearchEngine<TestDbContext> searchProvider = new SearchEngine<TestDbContext>(options, _context, new MemoryCache(new MemoryCacheOptions()));

            searchProvider.CreateIndex();

            Assert.True(searchProvider.IndexCount > 0);
        }

        private void PrintResult(IScoredSearchResultCollection<User> results)
        {
            _output.WriteLine($"总条数: {results.TotalHits}\t耗时: {results.Elapsed}");
            foreach (IScoredSearchResult<User> item in results.Results)
            {
                _output.WriteLine($"匹配度: {item.Score}\tName:{item.Entity.FirstName}\tSurname: {item.Entity.Surname}\tEmail: {item.Entity.Email}");
            }
        }
    }
}