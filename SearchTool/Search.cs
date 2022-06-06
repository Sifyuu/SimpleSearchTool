namespace SearchTool
{
    // REFACTOR: Use string streams instead of strings (don't know if possible (should certainly be))
    // REFACTOR: Use tokens for quaries
    // TODO: Standardise the naming scheme for readability
    // TODO: Refactor til at bruge enumerators og movenext?
    // TODO: Make it possible to search on wildcards found:
    //      Given a search query with a wildcard: "query\?string{wildcard0}" then
    //      it should be possible to use the wildcards found value in the search following it.

    /*
     * TODO Construct a more streamlined-for-use featureset
     * 
     * searchspace = Stream|Iterator|String;
     * searchQuery = String;
     * searchResult = Search.ProvideSearchspace(searchSpace).FindString(searchQuery);
     * or
     * searchResult = Search.ProvideSearchSpace(searchSpace).Search(options => {
     *      options.UseFindFirst() | options.UseFindEvery() | options.UseFindEvery(Force);
     *      options.GenericWildCardSearch(searchQuery);
     * });
     */

    public record Query
    {
        public Query(string searchSpace, string searchQuery)
        {
            this.SearchSpace = searchSpace;
            this.searchQuery = searchQuery;
        }

        public string SearchSpace { get; init; }
        public string searchQuery { get; init; }
    }

    public abstract record Token
    {
        public Token(string value, int position)
        {
            this.value = value;
            this.position = position;
        }

        public string value { get; init; }
        public int position { get; init; }
    }

    public record RecordToken : Token
    {
        public RecordToken(string value, int position) : base(value, position) { }
    }

    public record WildcardToken : Token
    {
        public WildcardToken(string value, int position) : base(value, position) { }
    }

    public static class Search
    {
        public static readonly string wildcardToken = @"\?";

        // Returns a list of every occurrence of the search query that was found in the search space
        public static List<Token> FindString(Query query)
        {
            var recordList = new List<Token>();

            for (int index = 0; index < query.SearchSpace.Length; ++index)
            {
                // If we find the search queries first char in the search space
                var foundFirstCharOfSearchQuery = query.SearchSpace[index] == query.searchQuery[0];
                if (foundFirstCharOfSearchQuery)
                {
                    // Assert whether the current substring@Index matches the full search query
                    if (AssertSearchInput(query.searchQuery, query.SearchSpace, index))
                    {
                        recordList.Add(new RecordToken(query.searchQuery, index));
                    }
                }
            }

            return recordList;
        }

        public static List<List<Token>> GenericWildcardSearch(Query query)
        {
            /* Making a generic wildcard search over [n] substrings and [n-1] wildcards requires that:
             *  - Rule 1: Each querying substring must exist for the complete search to be viable.
             *  - Rule 2: Each substring i in n must be found in order. (0 < i < n)
             *  
             *  Structure:
             *  Part 1: Split the query up into substringQueries delimited by the wildcard token.
             *  Part 2: Construct a matrix containing every occurence of every substring in the searchSpace.
             *  Part 3: Find as many full search queries from the matrix as possible.
             */
            // ============================ PART 1 ============================
            var listofQuerySubstrings = CutoutWildcard(query.searchQuery);
            var searchSpace = query.SearchSpace;
            var querySubstringCount = listofQuerySubstrings.Count();

            // Bail out fast on an empty search
            bool bailoutOnEmptySearchSpace = querySubstringCount == 0;
            bool bailoutOnEmptyQuery = query.SearchSpace.Length == 0;
            if (bailoutOnEmptySearchSpace || bailoutOnEmptyQuery) return new List<List<Token>>();

            // ============================ PART 2 ============================
            // 
            var substringSearchRecords = PopulateSearchRecords(searchSpace, listofQuerySubstrings);

            // Should this be moved into the function
            // Assert that each query substring was found in the search space at least once.
            foreach (var sr in substringSearchRecords)
            {
                bool foundNoViableQueryResult = substringSearchRecords[0].Count == 0;
                if (foundNoViableQueryResult) return new();
            }

            // ============================ PART 3 ============================
            // Given an array of lists of searchRecords hf. substringSearchRecords and
            // given a list of lists of searchRecords hf. searchResults then:
            //      Add the first viable substring record, based on initial position, to a new list in searchRecords.
            //      Iterate through searchRecords starting from index 1 where at each iteration do:
            //          Add the first substring record which initial position is:
            //              1. The lowest initial position which,
            //              2. is still greater than the (initial position of the previous search record
            //                  + size of the substringQuery of the previous record), and
            //              3. The substring has not been used before.
            // NOTE: substringSearchRecord has the property that the occurences of each substring will do ordered in terms
            //       position in the searchSpaces as encountered.
            // NOTE: This could be made much more efficent with another matrix data structure by minimizing for-loops.
            var visitedSubstringTable = new bool[querySubstringCount][];
            for (int listIndex = 0; listIndex < querySubstringCount; listIndex++)
            {
                visitedSubstringTable[listIndex] = new bool[substringSearchRecords[listIndex].Count()];
            }
            var searchResults = new List<List<Token>>();

            // NOTE: is it smarter to do a conditional-and-continue or a conditional-to-codeblock
            // Worst case each initial substring constructs a result so interate over all of them
            for (int initialSubstringIndex = 0; initialSubstringIndex < substringSearchRecords[0].Count(); initialSubstringIndex++)
            {
                bool currentInitialSubstringHasBeenVisited = visitedSubstringTable[0][initialSubstringIndex];
                if (currentInitialSubstringHasBeenVisited)
                    continue;

                var previousRecord = substringSearchRecords[0][initialSubstringIndex];
                var tempResult = new List<Token> { substringSearchRecords[0][initialSubstringIndex] };
                visitedSubstringTable[0][initialSubstringIndex] = true;

                for (int substringIndex = 1; substringIndex < querySubstringCount; substringIndex++)
                {
                    for (int occurenceIndex = 0; occurenceIndex < substringSearchRecords[substringIndex].Count(); occurenceIndex++)
                    {
                        bool currentSubstringHasBeenVisited = visitedSubstringTable[substringIndex][occurenceIndex];
                        if (currentSubstringHasBeenVisited)
                            continue;
                        visitedSubstringTable[substringIndex][occurenceIndex] = true;

                        var currentSubstringRecord = substringSearchRecords[substringIndex][occurenceIndex];
                        bool currentSubstringIsViable = previousRecord.position + previousRecord.value.Length < currentSubstringRecord.position;
                        if (currentSubstringIsViable)
                        {
                            var previousRecordEndPosition = previousRecord.position + previousRecord.value.Length;
                            bool thereExistAWildcard = currentSubstringRecord.position - previousRecordEndPosition > 0;
                            if (thereExistAWildcard)
                            {
                                var wildcard = new WildcardToken(
                                    searchSpace.Substring(previousRecordEndPosition, currentSubstringRecord.position - previousRecordEndPosition),
                                    previousRecordEndPosition);
                                tempResult.Add(wildcard);
                            }
                            tempResult.Add(currentSubstringRecord);

                            previousRecord = currentSubstringRecord;
                            UpdateVisitedTable(ref visitedSubstringTable, ref substringSearchRecords, currentSubstringRecord.position);
                            break;
                        }
                    }
                }

                // Assert that finding the last query substring is equivalent to finding a complete result
                bool aCompleteResultWasFound = tempResult.Last().value == listofQuerySubstrings.Last();
                if (aCompleteResultWasFound)
                    searchResults.Add(tempResult);
            }

            return searchResults;
        }

        // Since A new record has been visited then it's possible that other substring occurences have been made unviable.
        private static void UpdateVisitedTable(ref bool[][] visitedSubstringTable, ref List<Token>[] searchRecords, int updatedPosition)
        {
            for (int substringIndex = 0; substringIndex < visitedSubstringTable.Count(); substringIndex++)
            {
                for (int occurenceIndex = 0; occurenceIndex < visitedSubstringTable[substringIndex].Count(); occurenceIndex++)
                {
                    // This makes a lot of wasted work, but a viable improvement requires an improved matrix datastructure.
                    bool substringNoLongerViable = searchRecords[substringIndex][occurenceIndex].position < updatedPosition;
                    visitedSubstringTable[substringIndex][occurenceIndex] = substringNoLongerViable;
                }
            }
        }

        private static List<Token>[] PopulateSearchRecords(string searchSpace, List<string> listofQuerySubstrings)
        {
            var firstCharOfEachQuery = (from queries in listofQuerySubstrings select queries[0]).ToArray();
            var substringSearchRecords = new List<Token>[listofQuerySubstrings.Count()];
            substringSearchRecords.Distinct<List<Token>>();
            for (int index = 0; index < listofQuerySubstrings.Count(); index++)
            {
                substringSearchRecords[index] = new List<Token>();
            }

            for (int searchSpaceIndex = 0; searchSpaceIndex < searchSpace.Length; searchSpaceIndex++)
            {
                for (int querySubstringListIndex = 0; querySubstringListIndex < firstCharOfEachQuery.Count(); querySubstringListIndex++)
                {
                    bool foundFirstCharOfSearchQuery = searchSpace[searchSpaceIndex] == firstCharOfEachQuery[querySubstringListIndex];
                    if (foundFirstCharOfSearchQuery)
                    {
                        // Assert whether the current substringQuery@searchSpaceIndex matches the full search query
                        // TODO: Needs refactoring, the functions return value dictates flow directly. Look for a fix in the books.
                        // TODO: This opens the door for 2 viable near-identical strings, where one is concatenated with a suffix, to both be accepted at the same positions.
                        if (AssertSearchInput(listofQuerySubstrings[querySubstringListIndex], searchSpace, searchSpaceIndex))
                        {
                            substringSearchRecords[querySubstringListIndex].Add(new RecordToken(listofQuerySubstrings[querySubstringListIndex], searchSpaceIndex));
                        }
                    }
                }
            }

            return substringSearchRecords;
        }
        private static List<string> CutoutWildcard(string searchQuery)
        {
            // Find every wildcard in the searchQuery
            var wildcardSearchRecord = new Query(searchQuery, wildcardToken);
            var wildcardTokens = FindString(wildcardSearchRecord);

            // Bailout fast in case there isn't a wildcard
            bool thereIsNoWildcard = wildcardTokens.Count == 0;
            if (thereIsNoWildcard) return new() { searchQuery };

            // Split the searchQuery up using the wildcard(s) position(s).
            int position = 0;
            var wildcardTokenLength = wildcardToken.Length;
            var result = new List<string>();
            foreach (var wildcardToken in wildcardTokens)
            {
                result.Add(searchQuery.Substring(position, wildcardToken.position - position));
                position += wildcardToken.position - position + wildcardTokenLength;
            }

            // Edgecase: catching the last substring
            var lastWildcardToken = wildcardTokens.LastOrDefault();
            bool isThereALastSubstring = position <= searchQuery.Length;
            if (isThereALastSubstring)
            {
                result.Add(searchQuery.Substring(position, searchQuery.Length - position));
            }

            // Clean up empty substrings
            for (int index = 0; index < result.Count(); index++)
            {
                bool substringIsEmpty = result[index].Length == 0;
                if (substringIsEmpty) result.RemoveAt(index);
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