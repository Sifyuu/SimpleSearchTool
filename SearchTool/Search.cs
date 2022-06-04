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
    // TODO: Standardise the naming scheme for readability
    // TODO: Refactor til at bruge enumerators og movenext?

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
        public static bool ContainsString(SearchQuery query)
        {
            for (int index = 0; index < query.searchSpace.Length; ++index)
            {
                var foundFirstCharOfSearchQuery = query.searchSpace[index] == query.searchQuery[0];
                if (foundFirstCharOfSearchQuery)
                {
                    // Assert whether the current substring@Index matches the full search query
                    if (AssertSearchInput(query.searchQuery, query.searchSpace, index))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // Returns a list of every occurrence of the search query that was found in the search space
        public static List<SearchRecord> FindString(SearchQuery query)
        {
            var recordList = new List<SearchRecord>();

            for (int index = 0; index < query.searchSpace.Length; ++index)
            {
                // If we find the search queries first char in the search space
                var foundFirstCharOfSearchQuery = query.searchSpace[index] == query.searchQuery[0];
                if (foundFirstCharOfSearchQuery)
                {
                    // Assert whether the current substring@Index matches the full search query
                    if (AssertSearchInput(query.searchQuery, query.searchSpace, index))
                    {
                        recordList.Add(new SearchRecord(query.searchQuery, index));
                    }
                }
            }

            return recordList;
        }

        public static bool ContainsInstance(SearchQuery query)
        {
            return false;
        }

        public static SearchRecord FindFirstInstance(SearchQuery query)
        {
            return new SearchRecord("");
        }

        public static List<SearchRecord> FindEveryInstance(SearchQuery query)
        {
            return new List<SearchRecord>();
        }

        public static List<List<SearchRecord>> GenericWildcardSearch(SearchQuery query)
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
            var searchSpace = query.searchSpace;
            var querySubstringCount = listofQuerySubstrings.Count();

            // Bail out fast on an empty search
            bool bailoutOnEmptySearchSpace = querySubstringCount == 0;
            bool bailoutOnEmptyQuery = query.searchSpace.Length == 0;
            if (bailoutOnEmptySearchSpace || bailoutOnEmptyQuery) return new List<List<SearchRecord>>();

            // ===================== DEBUG =====================
            Console.WriteLine("Part 1: found {0} query substrings:", querySubstringCount);
            listofQuerySubstrings.ForEach(querySubstring =>
            {
                Console.Write("{0} ", querySubstring);
            });
            Console.Write('\n');

            // ============================ PART 2 ============================
            // 
            var substringSearchRecords = PopulateSearchRecords(searchSpace, listofQuerySubstrings);

            // Should this be moved into the function
            // Assert that each query substring was found in the search space at least once.
            foreach (var sr in substringSearchRecords)
            {
                bool foundNoViableQueryResult = substringSearchRecords[0].Count == 0;
                if (foundNoViableQueryResult) return new List<List<SearchRecord>>();
            }

            // ===================== DEBUG =====================
            Console.WriteLine("Part 2: found {0} substrings occurences:", substringSearchRecords.ToList().ConvertAll(list => list.Count()).Sum());
            var occurenceCount = substringSearchRecords.ToList().ConvertAll(list => list.Count());
            var largestAmountOfOccurences = occurenceCount.Max();
            var listOfLatestOccurences = new int[substringSearchRecords.Count()];
            for (int substringIndex = 0; substringIndex < querySubstringCount; substringIndex++)
            {
                for (int occurenceIndex = 0; occurenceIndex < substringSearchRecords[substringIndex].Count(); occurenceIndex++)
                {
                    var resultString = String.Format("{{\"{0}\", {1}}} ",
                                        substringSearchRecords[substringIndex][occurenceIndex].searchQuery,
                                        substringSearchRecords[substringIndex][occurenceIndex].initialPosition);
                    bool currentResultIsLarger = resultString.Length > listOfLatestOccurences[substringIndex];
                    if (currentResultIsLarger)
                        listOfLatestOccurences[substringIndex] = resultString.Length;
                }
            }

            for (int occurenceIndex = 0; occurenceIndex < largestAmountOfOccurences; occurenceIndex++)
            {
                for (int substringIndex = 0; substringIndex < querySubstringCount; substringIndex++)
                {
                    bool thereIsAnOccurenceToPrint = occurenceIndex < occurenceCount[substringIndex];
                    if (thereIsAnOccurenceToPrint)
                    {
                        var resultString = String.Format("{{\"{0}\", {1}}} ",
                                        substringSearchRecords[substringIndex][occurenceIndex].searchQuery,
                                        substringSearchRecords[substringIndex][occurenceIndex].initialPosition);
                        var padding = new String(' ', listOfLatestOccurences[substringIndex] - resultString.Length);
                        Console.Write(padding + resultString);
                    }
                    else
                    {
                        Console.Write(new String(' ', listOfLatestOccurences[substringIndex]));
                    }
                }
                Console.Write('\n');
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
            var searchResults = new List<List<SearchRecord>>();

            // NOTE: is it smarter to do a conditional-and-continue or a conditional-to-codeblock
            // Worst case each initial substring constructs a result so interate over all of them
            for (int initialSubstringIndex = 0; initialSubstringIndex < substringSearchRecords[0].Count(); initialSubstringIndex++)
            {
                bool currentInitialSubstringHasBeenVisited = visitedSubstringTable[0][initialSubstringIndex];
                if (currentInitialSubstringHasBeenVisited)
                    continue;

                var previousRecord = substringSearchRecords[0][initialSubstringIndex];
                var tempResult = new List<SearchRecord> { substringSearchRecords[0][initialSubstringIndex] };
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
                        Console.WriteLine($"assert for {{{previousRecord.searchQuery}, {currentSubstringRecord.searchQuery}}} | prev pos:{previousRecord.initialPosition}, prev len:{previousRecord.searchQuery.Length}, curr pos:{currentSubstringRecord.initialPosition}");
                        bool currentSubstringIsViable = previousRecord.initialPosition + previousRecord.searchQuery.Length < currentSubstringRecord.initialPosition;
                        if (currentSubstringIsViable)
                        {
                            tempResult.Add(currentSubstringRecord);
                            previousRecord = currentSubstringRecord;
                            UpdateVisitedTable(ref visitedSubstringTable, ref substringSearchRecords, currentSubstringRecord.initialPosition);
                            break;
                        }
                    }
                }

                bool aCompleteResultWasFound = tempResult.Count() == querySubstringCount;
                if (aCompleteResultWasFound)
                    searchResults.Add(tempResult);
            }

            /* ============================================= Structure ================================================
             * Given:
             *     - substringList:   An ordered list of query substrings.
             *     - arrayList:       An array of lists where each array index coincides with a substring and where the 
             *                        list contains every occurence of the substring ordered after when it's found in the 
             *                        searchSpace.
             *     - arrayArray:      An arrar of arrays containing bools where each index corresponds with a substring 
             *                        denothing whether, during the running of the algorithm, a substring was visited.
             *     - initialPosition: The position of each substring in the SearchSpace.
             * 
             * |Iterate over each index of the subtringList as substringListIndex and
             * |    Iterate over each index of the substringList[substringListindex] as substringOccurenceIndex and
             * |        Find the first substringOccurence where:
             * |            - It hasen't been recorded as visited in arrayArray,
             * |            - It's initial position is greater than the initial position of the previous index +
             * |              the length of the previously chosen substring.
             * 
             * 
             * ========================================================================================================
             */

            // ===================== DEBUG =====================
            Console.WriteLine("Part 3: found {0} results:", searchResults.Count());
            searchResults.ForEach(searchList =>
            {
                var accumulator = "";
                searchList.ForEach(element => accumulator += $"{{{element.searchQuery}, {element.initialPosition}}}");
                Console.WriteLine(accumulator);
            });

            return searchResults;
        }

        // Since A new record has been visited then it's possible that other substring occurences have been made unviable.
        private static void UpdateVisitedTable(ref bool[][] visitedSubstringTable, ref List<SearchRecord>[] searchRecords, int updatedPosition)
        {
            for (int substringIndex = 0; substringIndex < visitedSubstringTable.Count(); substringIndex++)
            {
                for (int occurenceIndex = 0; occurenceIndex < visitedSubstringTable[substringIndex].Count(); occurenceIndex++)
                {
                    // This makes a lot of wasted work, but a viable improvement requires an improved matrix datastructure.
                    bool substringNoLongerViable = searchRecords[substringIndex][occurenceIndex].initialPosition < updatedPosition;
                    visitedSubstringTable[substringIndex][occurenceIndex] = substringNoLongerViable;
                }
            }
        }

        private static List<SearchRecord>[] PopulateSearchRecords(string searchSpace, List<string> listofQuerySubstrings)
        {
            var firstCharOfEachQuery = (from queries in listofQuerySubstrings select queries[0]).ToArray();
            var substringSearchRecords = new List<SearchRecord>[listofQuerySubstrings.Count()];
            substringSearchRecords.Distinct<List<SearchRecord>>();
            for (int index = 0; index < listofQuerySubstrings.Count(); index++)
            {
                substringSearchRecords[index] = new List<SearchRecord>();
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
                            substringSearchRecords[querySubstringListIndex].Add(new SearchRecord(listofQuerySubstrings[querySubstringListIndex], searchSpaceIndex));
                        }
                    }
                }
            }

            return substringSearchRecords;
        }

        public static List<SearchRecord> SemanticWildcardSearch(SearchQuery search)
        {
            return new List<SearchRecord>();
        }

        private static List<string> CutoutWildcard(string searchQuery)
        {
            // Find every wildcard in the searchQuery
            var wildcardSearchRecord = new SearchQuery(searchQuery, wildcardToken);
            var wildcardTokens = FindString(wildcardSearchRecord);

            // Bailout fast in case there isn't a wildcard
            bool thereIsNoWildcard = wildcardTokens.Count == 0;
            if (thereIsNoWildcard) return new List<string> { searchQuery };

            // Split the searchQuery up using the wildcard(s) position(s).
            int position = 0;
            var wildcardTokenLength = wildcardToken.Length;
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