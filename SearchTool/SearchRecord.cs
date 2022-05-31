namespace SearchTool;

public record SearchRecord
{
    public SearchRecord(string searchQuery)
    {
        this.SearchQuery = searchQuery;
    }

    public SearchRecord(string searchQuery, int initialPosition)
    {
        this.SearchQuery = searchQuery;
        this.InitialPosition = initialPosition;
    }

    public int InitialPosition { get; set; }
    public string SearchQuery { get; init; }
}