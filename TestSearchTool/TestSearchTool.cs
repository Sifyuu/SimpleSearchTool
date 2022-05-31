using NUnit.Framework;
using SearchTool;
using System.Collections.Generic; // Needed to avoid name collision between NUnit List and Generic List<>

namespace TestSearchTool;
/*
     [Test, Order()]
     public void _() 
     {

     }
 */

[TestFixture]
public class TestSearchQuery
{
    private readonly string WildcardString = Search.WildcardToken;

    [Test]
    public void FindString_QueryNotContainedInSearchSpace()
    {
        string searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
        string query = " ";
        var searchQuery = new SearchQuery(searchSpace, query);
        List<SearchRecord> expectedSearchResultList = new List<SearchRecord>();

        List<SearchRecord> result = Search.FindString(searchQuery);

        Assert.That(result, Is.EqualTo(expectedSearchResultList));
    }

    [Test]
    public void FindString_SingleCharacter_QueryContainedInSearchSpaceOnce()
    {
        string searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
        string query = "f";
        var expectedSearchRecord1 = new SearchRecord(query, 13);
        var queryRecord = new SearchQuery(searchSpace, query);
        List<SearchRecord> expectedSearchResultList = new List<SearchRecord>
        {
            expectedSearchRecord1
        };

        List<SearchRecord> result = Search.FindString(queryRecord);

        Assert.That(expectedSearchResultList, Is.EqualTo(result));
    }

    [Test]
    public void FindString_SingleCharacter_QueryContainedInSearchSpaceMultipleTimes()
    {
        string searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
        string query = "+";
        var expectedSearchRecord1 = new SearchRecord(query, 2);
        var expectedSearchRecord2 = new SearchRecord(query, 24);
        var queryRecord = new SearchQuery(searchSpace, query);
        List<SearchRecord> expectedSearchResultList = new List<SearchRecord>
        {
            expectedSearchRecord1, expectedSearchRecord2
        };

        List<SearchRecord> result = Search.FindString(queryRecord);

        Assert.That(expectedSearchResultList, Is.EqualTo(result));
    }

    [Test]
    public void FindString_String_QueryContainedInSearchSpaceOnce()
    {
        string searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
        string query = "ur24";
        var expectedSearchRecord1 = new SearchRecord(query, 7);
        var searchQuery = new SearchQuery(searchSpace, query);
        List<SearchRecord> expectedSearchResultList = new List<SearchRecord>
        {
            expectedSearchRecord1
        };

        List<SearchRecord> result = Search.FindString(searchQuery);

        Assert.That(expectedSearchResultList, Is.EqualTo(result));
    }

    [Test]
    public void FindString_String_QueryContainedInSearchSpaceMultipleTimes()
    {
        string searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
        string query = "21";
        var expectedSearchRecord1 = new SearchRecord(query, 0);
        var expectedSearchRecord2 = new SearchRecord(query, 5);
        var queryRecord = new SearchQuery(searchSpace, query);
        List<SearchRecord> expectedSearchResultList = new List<SearchRecord>
        {
            expectedSearchRecord1, expectedSearchRecord2
        };

        List<SearchRecord> result = Search.FindString(queryRecord);

        Assert.That(expectedSearchResultList, Is.EqualTo(result));
    }

    [Test]
    public void FindString_QueryPrefixFoundAtTheEndOfSearchSpace()
    {
        string searchSpace = "21+e921ur24hgfwneodjqlnv+r4039t530yt834";
        string query = "3472";
        var queryRecord = new SearchQuery(searchSpace, query);
        List<SearchRecord> expectedSearchResultList = new List<SearchRecord>();

        List<SearchRecord> result = Search.FindString(queryRecord);

        Assert.That(expectedSearchResultList, Is.EqualTo(result));
    }

    [Test]
    public void GenericWildcardSearch_SearchWithWildcardOnly_SearchForSomeString()
    {
        string query = "someString";
        string searchSpace = WildcardString;
        var searchQuery = new SearchQuery(searchSpace, query);
        List<List<SearchRecord>> expectedResult = new List<List<SearchRecord>>();

        List<List<SearchRecord>> actualResult = Search.GenericWildcardSearch(searchQuery);

        Assert.That(expectedResult == actualResult);
    }

    [Test]
    public void GenericWildcardSearch_SearchWithWildcardOnly_SearchForEmptyString()
    {
        string searchQuery = WildcardString;
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