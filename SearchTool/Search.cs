namespace SearchTool
{
    // The search algorithms should make use of tree search paradimes rather than a
    // recursive approach.

    // DFS would probably work best to minimize overhead
    // Sparse matrix representation for exploration and storage.

    // REFACTOR: Use string streams instead of strings (don't know if possible (should certainly be))
    // REFACTOR: Use tokens for quaries
    /*
    public record Query
    {
        public List<Token> Tokens { get; set; }
        public string SearchSpace1 { get; set; }
        public string SearchQuery1 { get; set; }
    }

    public record Token
    {
        public int InitialPosition { get; set; }
        public string StringToken { get; set; }
    }

    public record StringToken : Token {}

    public record WildcardToken : Token 
    {
        public int FinalPosition { get; set; }
    }

    // ------------------------------------------
    */


    // TODO: Standardise the naming scheme for readability
    public record SearchRecord
    {
        public SearchRecord(string searchQuery)
        {
            this.searchQuery = searchQuery;
        }

        public SearchRecord(string searchQuery, int initialPosition)
        {
            this.searchQuery = searchQuery;
            this.initialPosition = initialPosition;
        }

        public int initialPosition { get; set; }
        public string searchQuery { get; init; }
    }

    public record SearchQuery
    {
        public SearchQuery(string searchSpace, string searchQuery)
        {
            this.searchSpace = searchSpace;
            this.searchQuery = searchQuery;
        }

        public SearchQuery(string searchSpace, string searchQuery,
                            int initialPosition, int finalPosition)
        {
            this.searchSpace = searchSpace;
            this.searchQuery = searchQuery;
            this.initialPosition = initialPosition;
            this.finalPosition = finalPosition;
        }

        public int initialPosition { get; set; }
        public int finalPosition { get; set; }
        public string searchSpace { get; init; }
        public string searchQuery { get; init; }
    }

    // ======================================================================

    public static class Search
    {
        public static readonly string wildcardToken = @"\?";

        // Returs a bool denoting whether the search query is contained in the search space
        public static bool ContainsString(SearchQuery search)
        {
            for (int index = 0; index < search.searchSpace.Length; ++index)
            {
                var foundFirstCharOfSearchQuery = search.searchSpace[index] == search.searchQuery[0];
                if (foundFirstCharOfSearchQuery)
                {
                    // Assert whether the current substring@Index matches the full search query
                    if (AssertSearchInput(search.searchQuery, search.searchSpace, index))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Returns a list of every occurrence of the search query that was found in the 'search space'
        public static List<SearchRecord> FindString(SearchQuery search)
        {
            var recordList = new List<SearchRecord>();

            for (int index = 0; index < search.searchSpace.Length; ++index)
            {
                // If we find the search queries first char in the search space
                var foundFirstCharOfSearchQuery = search.searchSpace[index] == search.searchQuery[0];
                if (foundFirstCharOfSearchQuery)
                {
                    // Assert whether the current substring@Index matches the full search query
                    if (AssertSearchInput(search.searchQuery, search.searchSpace, index))
                    {
                        recordList.Add(new SearchRecord(search.searchQuery, index));
                    }
                }
            }

            return recordList;
        }
        public static List<List<SearchRecord>> GenericWildcardSearch(SearchQuery search)
        {
            // Making a generic wildcard search over [n] substrings and [n-1] wildcards requires that:
            //   - Rule 1: Each querying substring must exist for the complete search to be viable.
            //   - Rule 2: Each substring i in n must be found in order. (0 < i < n)
            // NOTE: There can be multiple results in one searchSpace which fulfill these rules.
            // PROBLEM: Should there be multiple instances (say n instances) of strings satisfying rule 1 at nearly the same position then
            //          it spawns multiple nearly identical results. If this occurs multiple times (say m times) for what would otherwise
            //          be a single result; then it would result in the query returning a crossproduct of n^m nearly identical results.
            var listofQuerySubstrings = CutoutWildcard(search.searchQuery);
            var searchSpace = search.searchSpace;
            var querySubstringCount = listofQuerySubstrings.Count();

            // Bail out fast on an empty search
            bool bailoutOnEmptySearchSpace = querySubstringCount == 0;
            bool bailoutOnEmptyQuery = search.searchSpace.Length == 0;
            if (bailoutOnEmptySearchSpace || bailoutOnEmptyQuery) return new List<List<SearchRecord>>();

            // Get the first char of every substring query to simplify the search
            var firstCharOfEachQuery = (from query in listofQuerySubstrings select query[0]).ToArray();

            // Construct an array containing every instance of every substring in the query
            // Where:
            //      - The graph is separated into layers, with each layer corresponding
            //          to a substring,
            //      - Each instance of a substring spawns a new node,
            //      - every node connects to the next layer only if the next
            //          search query can be found in the text.
            var substringSearchRecords = new List<SearchRecord>[querySubstringCount];

            for (int searchSpaceIndex = 0; searchSpaceIndex < searchSpace.Length; searchSpaceIndex++)
            {
                for (int searchQueryListIndex = 0; searchQueryListIndex < firstCharOfEachQuery.Count(); searchQueryListIndex++)
                {
                    var foundFirstCharOfSearchQuery = searchSpace[searchSpaceIndex] == firstCharOfEachQuery[searchQueryListIndex];
                    if (foundFirstCharOfSearchQuery)
                    {
                        // Assert whether the current substringQuery@searchSpaceIndex matches the full search query
                        if (AssertSearchInput(listofQuerySubstrings[searchQueryListIndex], searchSpace, searchSpaceIndex))
                        {
                            substringSearchRecords[searchQueryListIndex].Add(
                            new SearchRecord(search.searchQuery, searchSpaceIndex));
                        }
                    }
                }
            }

            // Assert that each query substring was found in the search space at least once.
            foreach (var sr in substringSearchRecords)
            {
                bool foundNoViableQueryResult = substringSearchRecords[0].Count == 0;
                if (foundNoViableQueryResult) return new List<List<SearchRecord>>();
            }

            // Given an array of lists of searchRecords hf. substringSearchRecords and
            // given a list of lists of searchRecords hf. searchResults then:
            //      Add the first viable substring record, based on initial position, to a new list in searchRecords.
            //      Iterate through searchRecords starting from index 1 where at each iteration do:
            //          Add the first substring record which initial position is:
            //              1. The lowest initial position which,
            //              2. is still greater than the (initial position of the previous search record
            //                  + size of the substringQuery of the previous record), and
            //              3. The substring has not been used before.

            /* TODO: How should the method return if there are multiple possible paths through the array?
             * TODO: Should this return a list of lists where every possible extra substring query
             * TODO: just the first complete instance
             * Example:
             *      Given searchSpace = "1223", and searchQuery = "1\?2\?3", then
             *      the returned value could either be ["1", "2", "3"] or
             *                                         [["1", "2", "3"], ["1", "2", "3"]]
             *      Where the multiple lists come from the 2 instances of the '2' char.
             *      Should an error be thrown?
             *      
             * For now it returns the most immediate list of substrings that conforms to the rules.
             */

            var listOfInitialSubstrings = substringSearchRecords[0];
            var searchResults = new List<List<SearchRecord>>();
            var visitedSubstringRecordTable = new List<bool>[querySubstringCount];

            // NOTE: I think initializing each list in the array with a length equal to the size in substringSearchRecord
            // NOTE: is neccesary because array indexing into the list could otherwise lead to indexing out of bounds.
            // Initialize every array index in the visited table to a size equal to the corresponding list in substringSearchRecord.
            for (int visitedTableIndex = 0; visitedTableIndex < querySubstringCount; visitedTableIndex++)
            {
                visitedSubstringRecordTable[visitedTableIndex] = new List<bool>(substringSearchRecords.Length);
            }

            var tempSearchResult = new List<SearchRecord>(querySubstringCount);
            // Iterate over every substring which can be used as a starting point for a viable search result
            foreach (bool viableStartingSubstring in visitedSubstringRecordTable[0])
            {
                // Bailout if the current starting substring has been visited already.
                if (!viableStartingSubstring) continue;

                for (int visitedRecordIndex = 0; visitedRecordIndex < querySubstringCount; visitedRecordIndex++)
                {
                    bool hasVisitedAllSubstrings = true;
                    visitedSubstringRecordTable[visitedRecordIndex].ForEach(visit => hasVisitedAllSubstrings = hasVisitedAllSubstrings && visit);
                    if (hasVisitedAllSubstrings) return searchResults;

                    int highestSubstring = substringSearchRecords[visitedRecordIndex].Min(val => val.initialPosition);
                    var leastViableSubstring = substringSearchRecords[visitedRecordIndex].Where(val => val.initialPosition == highestSubstring).ToArray()[0]; // Cursed

                    // The branching statements below initialized this variable at every path so it will never be null.
                    SearchRecord previousSubstringRecord = leastViableSubstring;

                    bool firstSearchResult = searchResults.Count() == 0;
                    bool firstSubstring = tempSearchResult.Count() == 0;
                    if (!firstSubstring) previousSubstringRecord = tempSearchResult[visitedRecordIndex - 1];
                    if (!firstSearchResult && firstSubstring) previousSubstringRecord = searchResults.Last()[querySubstringCount];
                    if (firstSearchResult && firstSubstring) previousSubstringRecord = leastViableSubstring;


                    SearchRecord bestSubstring = leastViableSubstring;

                    for (int visitedSubstringIndex = 0;
                         visitedSubstringIndex < visitedSubstringRecordTable[visitedRecordIndex].Count();
                         visitedSubstringIndex++)
                    {
                        var currentRecord = substringSearchRecords[visitedRecordIndex][visitedSubstringIndex];
                        bool substringHasBeenVisited = visitedSubstringRecordTable[visitedRecordIndex][visitedSubstringIndex];
                        bool substringRuleTwo = currentRecord.initialPosition >= previousSubstringRecord.initialPosition + previousSubstringRecord.searchQuery.Length;
                        if (substringHasBeenVisited) continue;
                        if (substringRuleTwo) bestSubstring = currentRecord;

                        // substring has been visited
                        visitedSubstringRecordTable[visitedRecordIndex][visitedSubstringIndex] = true;
                    }

                    tempSearchResult.Add(bestSubstring);
                }

                searchResults.Add(tempSearchResult); // Pass by value hopefully
            }

            return searchResults;
        }

        public static List<SearchRecord> SemanticWildcardSearch(SearchQuery search)
        {
            return new List<SearchRecord>();
        }

        private static List<string> CutoutWildcard(string searchQuery)
        {
            var wildcardTokenLength = wildcardToken.Length;

            // Find every wildcard in the searchQuery
            var wildcardSearchRecord = new SearchQuery(searchQuery, wildcardToken);
            var wildcardTokens = FindString(wildcardSearchRecord);

            // Bailout fast in case there isn't a wildcard
            bool thereIsNoWildcard = wildcardTokens.Count == 0;
            if (thereIsNoWildcard) return new List<string> { searchQuery };

            // Split the searchQuery up using the wildcard(s) position(s).
            int position = 0;
            var result = new List<string>();
            foreach (var wildcardToken in wildcardTokens)
            {
                result.Add(searchQuery.Substring(position, wildcardToken.initialPosition - position));
                position += wildcardToken.initialPosition - position + wildcardTokenLength;
            }

            // Edgecase: catching the last substring
            var lastWildcardToken = wildcardTokens.LastOrDefault();
            bool isThereALastSubstring = position <= searchQuery.Length;
            if (isThereALastSubstring)
            {
                result.Add(searchQuery.Substring(position, searchQuery.Length - position));
            }

            return result;
        }

        private static bool AssertSearchInput(string query, string searchSpace, int searchSpaceIndexPosition)
        {
            // Iterate over the searchspace at the current position and assert whether the current string matches 'quary'
            for (int searchSpaceIndex = searchSpaceIndexPosition, queryIndex = 0;
                     queryIndex < query.Length;
                   ++searchSpaceIndex, ++queryIndex)
            {
                // Merging the if statements could lead to indexing outside the array, which would throw an exception.
                var reachedEndOfSearchSpace = searchSpaceIndex >= searchSpace.Length;
                if (reachedEndOfSearchSpace) return false;

                bool missmatchBetweenSearchQueryAndSearchSpaceSubstring = searchSpace[searchSpaceIndex] != query[queryIndex];
                if (missmatchBetweenSearchQueryAndSearchSpaceSubstring) return false;
            }

            return true;
        }
    }
}