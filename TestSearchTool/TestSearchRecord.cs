using NUnit.Framework;
using SearchTool;

namespace TestSearchTool;

[TestFixture]
public class TestSearchRecord
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