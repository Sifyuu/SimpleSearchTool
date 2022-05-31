namespace SearchTool;

public record SearchQuery:SearchRecord
{
    public SearchQuery(string searchSpace, string searchQuery) : base(searchQuery)
    {
        this.SearchSpace = searchSpace;
    }

    public SearchQuery(string searchSpace, string searchQuery,
        int initialPosition, int finalPosition) : base(searchQuery, initialPosition)
    {
        this.SearchSpace = searchSpace;
        this.FinalPosition = finalPosition;
    }

    public int FinalPosition { get; set; }
    public string SearchSpace { get; init; }

}