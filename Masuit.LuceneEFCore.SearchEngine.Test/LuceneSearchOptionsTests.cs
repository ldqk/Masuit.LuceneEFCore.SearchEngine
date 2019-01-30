using System.Collections.Generic;
using Xunit;

namespace Masuit.LuceneEFCore.SearchEngine.Test
{
    public class LuceneSearchOptionsTests
    {
        [Fact]
        public void LuceneSearchOptionsCanBeConstructedWithMultipleFields()
        {
            SearchOptions options = new SearchOptions("Test", "one,two,three,four");

            Assert.Equal(4, options.Fields.Count);
            Assert.Equal("one", options.Fields[0]);
            Assert.Equal("two", options.Fields[1]);
        }

        [Fact]
        public void LuceneSearchOptionsParsesFieldsAndOrderBy()
        {
            SearchOptions options = new SearchOptions("Test", "one, two  ,  three", 1000, null, null, "test, another test");

            Assert.Equal(3, options.Fields.Count);
            Assert.Equal("one", options.Fields[0]);
            Assert.Equal("two", options.Fields[1]);
            Assert.Equal("three", options.Fields[2]);

            Assert.Equal(2, options.OrderBy.Count);
            Assert.Equal("test", options.OrderBy[0]);
            Assert.Equal("anothertest", options.OrderBy[1]);
        }

        [Fact]
        public void GetBoostsWillReturnAValidSetOfBoostsForGivenOptions()
        {
            SearchOptions options = new SearchOptions("John Developer", "FirstName,JobTitle");

            Assert.Equal(2, options.Boosts.Count);
            Assert.Equal(1, options.Boosts["FirstName"]);
            Assert.Equal(1, options.Boosts["JobTitle"]);
        }

        [Fact]
        public void ABoostCanBeAdded()
        {
            SearchOptions options = new SearchOptions("Test", "One,Two,Three");

            options.SetBoost("Two", 2f);

            Assert.Equal(3, options.Boosts.Count);
            Assert.Equal(2, options.Boosts["Two"]);
        }

        [Fact]
        public void ClearingBoostsWillReturnDefaultValues()
        {
            Dictionary<string, float> boosts = new Dictionary<string, float>
            {
                { "One", 1.1f },
                { "Two", 9.1f }
            };


            SearchOptions options = new SearchOptions("Test", "One,Two", 1000, boosts);

            Assert.Equal(2, options.Boosts.Count);
            Assert.Equal(1.1f, options.Boosts["One"]);
            Assert.Equal(9.1f, options.Boosts["Two"]);

            options.ClearBoosts();

            Assert.Equal(2, options.Boosts.Count);
            Assert.Equal(1, options.Boosts["One"]);
            Assert.Equal(1, options.Boosts["Two"]);
        }
    }
}