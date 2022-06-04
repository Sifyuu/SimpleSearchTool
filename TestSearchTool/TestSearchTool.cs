using NUnit.Framework;
using SearchTool;

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
            var searchSpace = "1idoieogi42hfwq9023u48t02843";
            var searchQuery = " ";
            var query = new SearchQuery(searchSpace, searchQuery);

            bool result = Search.ContainsString(query);

            Assert.IsFalse(result);
        }

        [Test]
        public void ContainsString_String_QueryContainedInSearchSpace()
        {
            var searchSpace = "1idoieogi42hfwq9023u48t02843";
            var searchQuery = "1i";
            var query = new SearchQuery(searchSpace, searchQuery);

            bool result = Search.ContainsString(query);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsString_SingleCharacter_QueryContainedInSearchSpace()
        {
            var searchSpace = "1idoieogi42hfwq9023u48t02843";
            var searchQuery = "1";
            var query = new SearchQuery(searchSpace, searchQuery);

            bool result = Search.ContainsString(query);

            Assert.IsTrue(result);
        }

        [Test]
        public void ContainsString_String_QueryContainedInTheSearchSpace_PlacedAtTheEnd()
        {
            var searchSpace = "1idoieogi42hfwq9023u48t02843";
            var searchQuery = "02843";
            var query = new SearchQuery(searchSpace, searchQuery);

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
            var searchQuery = " ";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedSearchResultList = new List<SearchRecord>();

            var actualResult = Search.FindString(query);

            Assert.That(actualResult, Is.EqualTo(expectedSearchResultList));
        }

        [Test]
        public void FindString_SingleCharacter_QueryContainedInSearchSpaceOnce()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var searchQuery = "f";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<SearchRecord> {
                new SearchRecord(searchQuery, 13)
            };

            var actualResult = Search.FindString(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void FindString_SingleCharacter_QueryContainedInSearchSpaceMultipleTimes()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var searchQuery = "+";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<SearchRecord> {
                new SearchRecord(searchQuery, 2),
                new SearchRecord(searchQuery, 24)
            };

            var actualResult = Search.FindString(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void FindString_String_QueryContainedInSearchSpaceOnce()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var searchQuery = "ur24";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<SearchRecord> {
                 new SearchRecord(searchQuery, 7)
            };

            var actualResult = Search.FindString(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void FindString_String_QueryContainedInSearchSpaceMultipleTimes()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var searchQuery = "21";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<SearchRecord> {
                new SearchRecord(searchQuery, 0),
                new SearchRecord(searchQuery, 5)
            };

            var actualResult = Search.FindString(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void FindString_QueryPrefixFoundAtTheEndOfSearchSpace()
        {
            var searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
            var searchQuery = "3472";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<SearchRecord>();

            var actualResult = Search.FindString(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
    }

    [TestFixture]
    public class Search_TestGenericWildcardSearch
    {
        string wildcardString = Search.wildcardToken;

        [Test]
        public void GenericWildcardSearch_SearchForFullTheSearchSpace_WithoutWildcard()
        {
            var searchSpace = "searchSpaceString";
            var searchQuery = searchSpace;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(searchSpace, 0) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchForASubstringInSearchSpace_WithoutWildcard()
        {
            var searchSpace = "searchSpaceString";
            var searchQuery = "Space";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(searchQuery, 6) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchForSomeString_SearchspaceContainsWildcardOnly()
        {
            var searchSpace = wildcardString;
            var searchQuery = "someString";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>>();

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchWithWildcardOnly_EmptySearchspace()
        {
            var searchSpace = "";
            var searchQuery = wildcardString;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>>();

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchWithoutWildcard_FindString()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var searchQuery = "ipsum";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(searchQuery, 6) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchWithoutWildcard_DontFindString()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var searchQuery = "adipiscing";
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>>();

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchWithSingleWildcard()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var querySubstring0 = "m i";
            var querySubstring1 = "et. ";
            var searchQuery = querySubstring0 + wildcardString + querySubstring1;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                 new List<SearchRecord> { new SearchRecord(querySubstring0, 4), new SearchRecord("et. ", 24) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_SearchWithMultipleWildcards()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var querySubstring0 = "lo";
            var querySubstring1 = "i";
            var querySubstring2 = "et";
            var searchQuery = querySubstring0 + wildcardString + querySubstring1 + wildcardString + querySubstring2;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(querySubstring0, 0), new SearchRecord(querySubstring1, 6), new SearchRecord(querySubstring2, 24) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_WildcardPlacedAtTheStartOfSearchQuery()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var querySubstring = "dolor";
            var searchQuery = wildcardString + querySubstring;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(querySubstring, 12) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_WildcardPlacedAtTheEndOfSearchQuery()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var querySubstring = "dolor";
            var searchQuery = querySubstring + wildcardString;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(querySubstring, 12) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void GenericWildcardSearch_TwoWildcardsPlacedConsecutively()
        {
            var searchSpace = "lorem ipsum dolor sit amet. Consectetuloer";
            var querySubstring0 = "lorem";
            var querySubstring1 = "dolor";
            var searchQuery = querySubstring0 + wildcardString + wildcardString + querySubstring1;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(querySubstring0, 0), new SearchRecord(querySubstring1, 12) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        
        [Test]
        public void GenericWildcardSearch_SearchqueryOverlapsInSearchSpace()
        {
            var searchSpace = "someaningmeaningoodgood";
            var querySubstring0 = "some";
            var querySubstring1 = "meaning";
            var querySubstring2 = "good";
            var searchQuery = querySubstring0 + wildcardString + querySubstring1 + wildcardString + querySubstring2;
            var query = new SearchQuery(searchSpace, searchQuery);
            var expectedResult = new List<List<SearchRecord>> {
                new List<SearchRecord> { new SearchRecord(querySubstring0, 0), new SearchRecord(querySubstring1, 9), 
                                         new SearchRecord(querySubstring2, 19) }
            };

            var actualResult = Search.GenericWildcardSearch(query);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }
        
    }

    [TestFixture]
    public class Search_TestSemanticWildcardSearch
    {

    }
}