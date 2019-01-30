using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using Masuit.LuceneEFCore.SearchEngine.Test.Helpers;
using Masuit.LuceneEFCore.SearchEngine.Test.Models;
using System.Linq;
using Xunit;

namespace Masuit.LuceneEFCore.SearchEngine.Test
{
    public class LuceneIndexSearcherTests : IClassFixture<TestDataGenerator>
    {
        // class setup
        static readonly Directory Directory = new RAMDirectory();
        static readonly Analyzer Analyzer = new StandardAnalyzer(Lucene.Net.Util.LuceneVersion.LUCENE_48);

        // create an index
        static readonly LuceneIndexer Indexer = new LuceneIndexer(Directory, Analyzer);
        static readonly LuceneIndexSearcher Searcher = new LuceneIndexSearcher(Directory, Analyzer);

        public LuceneIndexSearcherTests(TestDataGenerator tdg)
        {
            Indexer.CreateIndex(tdg.AllData);
        }

        [Fact]
        public void AnIndexCanBeSearched()
        {
            SearchOptions options = new SearchOptions("John", "FirstName");

            var results = Searcher.ScoredSearch(options);

            Assert.Equal(5, results.TotalHits);
        }

        //[Fact]
        //public void SearchesCanBeDoneAcrossMultipleTypes()
        //{
        //    SearchOptions options = new SearchOptions("John China", "FirstName, Country");
        //    var results = searcher.ScoredSearch(options);

        //    var firstType = results.Results.First().Document.GetField("Type");
        //    var lastType = results.Results[results.TotalHits - 1].Document.GetField("Type");

        //    Assert.NotEqual(firstType, lastType);
        //}

        [Fact]
        public void TopNNumberOfResultsCanBeReturned()
        {
            SearchOptions options = new SearchOptions("China", "Country", 1000, null, typeof(City));

            var allResults = Searcher.ScoredSearch(options);

            options.Take = 10;

            var subSet = Searcher.ScoredSearch(options);

            for (var index = 0; index < 10; index++)
            {
                Assert.Equal(allResults.Results[index].Document.Get("IndexId"), subSet.Results[index].Document.Get("IndexId"));
            }

            Assert.Equal(10, subSet.Results.Count);
            Assert.Equal(allResults.TotalHits, subSet.TotalHits);
        }

        [Fact]
        public void ResultsetCanBeSkippedAndTaken()
        {
            SearchOptions options = new SearchOptions("China", "Country", 1000, null, typeof(City));

            var allResults = Searcher.ScoredSearch(options);

            options.Take = 10;
            options.Skip = 10;

            var subSet = Searcher.ScoredSearch(options);

            for (var index = 0; index < 10; index++)
            {
                Assert.Equal(allResults.Results[index + 10].Document.Get("IndexId"), subSet.Results[index].Document.Get("IndexId"));
            }

            Assert.Equal(10, subSet.Results.Count);
            Assert.Equal(allResults.TotalHits, subSet.TotalHits);
        }

        [Fact]
        public void ResultsetCanBeOrdered()
        {
            SearchOptions options = new SearchOptions("John", "FirstName", 1000, null, typeof(User));

            var unordered = Searcher.ScoredSearch(options);

            options.OrderBy.Add("Surname");

            var ordered = Searcher.ScoredSearch(options);

            Assert.Equal(ordered.TotalHits, unordered.TotalHits);
            Assert.NotEqual(ordered.Results.First().Document.Get("Id"), unordered.Results.First().Document.Get("Id"));
        }

        [Fact]
        public void ASingleDocumentIsReturnedFromScoredSearchSingle()
        {
            SearchOptions options = new SearchOptions("jfisherj@alexa.com", "Email");

            var result = Searcher.ScoredSearchSingle(options);

            Assert.NotNull(result);
            // Assert.InstanceOfType(result, typeof(Document));
            Assert.IsType<Document>(result);
            Assert.IsAssignableFrom<Document>(result);
            Assert.Equal("jfisherj@alexa.com", result.Get("Email"));
        }

        [Fact]
        public void MultipleResultsIsNotAProblemFromScoredSearchSingle()
        {
            SearchOptions options = new SearchOptions("John", "FirstName");

            var result = Searcher.ScoredSearchSingle(options);

            Assert.NotNull(result);
            Assert.IsType<Document>(result);
            Assert.Equal("John", result.Get("FirstName"));
        }
    }
}