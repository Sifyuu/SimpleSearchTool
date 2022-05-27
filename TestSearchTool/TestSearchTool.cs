using NUnit.Framework;
using SearchTool;
using System.Collections.Generic; // Needed to avoid name collision between NUnit List and Generic List<>

namespace TestSearchTool
{
    /*
         [Test, Order()]
         public void _() 
         {

         }
     */

    [TestFixture]
    public class Search_TestContainsString
    {
        [Test]
        public void ContainsString_QueryNotContainedInSearchSpace()
        {
            var randomSearchSpace = "1idoieogi42hfwq9023u48t02843";
            var queryNotContainedInSearchSpace = " ";
            var query = new SearchQuery(randomSearchSpace, queryNotContainedInSearchSpace);

            bool result = Search.ContainsString(query);

            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsString_String_QueryContainedInSearchSpace()
        {
            var randomSearchSpace = "1idoieogi42hfwq9023u48t02843";
            var queryContainedInSearchSpace = "1i";
            var query = new SearchQuery(randomSearchSpace, queryContainedInSearchSpace);

            bool result = Search.ContainsString(query);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsString_SingleCharacter_QueryContainedInSearchSpace()
        {
            var randomSearchSpace = "1idoieogi42hfwq9023u48t02843";
            var queryContainedInSearchSpace = "1";
            var query = new SearchQuery(randomSearchSpace, queryContainedInSearchSpace);

            bool result = Search.ContainsString(query);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsString_String_QueryContainedInTheSearchSpace_PlacedAtTheEnd()
        {
            var randomSearchSpace = "1idoieogi42hfwq9023u48t02843";
            var queryContainedInSearchSpace = "02843";
            var query = new SearchQuery(randomSearchSpace, queryContainedInSearchSpace);

            bool result = Search.ContainsString(query);

            Assert.IsTrue(result);
        }
    }

    [TestFixture]
    public class Search_TestFindString
    {
        [Test]
        public void FindString_QueryNotContainedInSearchSpace()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var query = " ";
            var searchQuery = new SearchQuery(searchSpace, query);
            var expectedSearchResultList = new List<SearchRecord>();

            var result = Search.FindString(searchQuery);

            Assert.That(result, Is.EqualTo(expectedSearchResultList));

        }

        [Test]
        public void FindString_SingleCharacter_QueryContainedInSearchSpaceOnce()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var query = "f";
            var expectedSearchRecord1 = new SearchRecord(query, 13);
            var queryRecord = new SearchQuery(searchSpace, query);
            var expectedSearchResultList = new List<SearchRecord> {
                expectedSearchRecord1
            };

            var result = Search.FindString(queryRecord);

            Assert.That(expectedSearchResultList, Is.EqualTo(result));
        }

        [Test]
        public void FindString_SingleCharacter_QueryContainedInSearchSpaceMultipleTimes()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var query = "+";
            var expectedSearchRecord1 = new SearchRecord(query, 2);
            var expectedSearchRecord2 = new SearchRecord(query, 24);
            var queryRecord = new SearchQuery(searchSpace, query);
            var expectedSearchResultList = new List<SearchRecord> {
                expectedSearchRecord1, expectedSearchRecord2
            };

            var result = Search.FindString(queryRecord);

            Assert.That(expectedSearchResultList, Is.EqualTo(result));
        }

        [Test]
        public void FindString_String_QueryContainedInSearchSpaceOnce()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var query = "ur24";
            var expectedSearchRecord1 = new SearchRecord(query, 7);
            var searchQuery = new SearchQuery(searchSpace, query);
            var expectedSearchResultList = new List<SearchRecord> {
                expectedSearchRecord1
            };

            var result = Search.FindString(searchQuery);

            Assert.That(expectedSearchResultList, Is.EqualTo(result));
        }

        [Test]
        public void FindString_String_QueryContainedInSearchSpaceMultipleTimes()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var query = "21";
            var expectedSearchRecord1 = new SearchRecord(query, 0);
            var expectedSearchRecord2 = new SearchRecord(query, 5);
            var queryRecord = new SearchQuery(searchSpace, query);
            var expectedSearchResultList = new List<SearchRecord> {
                expectedSearchRecord1, expectedSearchRecord2
            };

            var result = Search.FindString(queryRecord);

            Assert.That(expectedSearchResultList, Is.EqualTo(result));
        }

        [Test]
        public void FindString_QueryPrefixFoundAtTheEndOfSearchSpace()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var query = "3472";
            var queryRecord = new SearchQuery(searchSpace, query);
            var expectedSearchResultList = new List<SearchRecord>();

            var result = Search.FindString(queryRecord);

            Assert.That(expectedSearchResultList, Is.EqualTo(result));
        }
    }

    [TestFixture]
    public class Search_TestGenericWildcardSearch
    {
        string wildcardString = Search.wildcardToken;

        [Test]
        public void GenericWildcardSearch_SearchWithWildcardOnly_SearchForSomeString()
        {
            var query = "someString";
            var searchSpace = wildcardString;
            var searchQuery = new SearchQuery(searchSpace, query);
            var expectedResult = new List<List<SearchRecord>>();

            var actualResult = Search.GenericWildcardSearch(searchQuery);

            Assert.That(expectedResult == actualResult);
        }

        [Test]
        public void GenericWildcardSearch_SearchWithWildcardOnly_SearchForEmptyString()
        {
            var searchQuery = wildcardString;

        }

        [Test]
        public void GenericWildcardSearch_SearchWithoutWildcard()
        {

        }

        [Test]
        public void GenericWildcardSearch_SearchWithSingleWildcard()
        {

        }

        [Test]
        public void GenericWildcardSearch_SearchWithMultipleWildcards()
        {

        }

        [Test]
        public void GenericWildcardSearch_WildcardPlacedAtTheStartOfSearchQuery()
        {

        }

        [Test]
        public void GenericWildcardSearch_WildcardPlacedAtTheEndOfSearchQuery()
        {

        }

        [Test]
        public void GenericWildcardSearch_TwoWildcardsPlacedConsecutively()
        {

        }
    }

    [TestFixture]
    public class Search_TestSemanticWildcardSearch
    {

    }
}