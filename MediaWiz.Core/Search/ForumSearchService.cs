using Examine;
using Examine.Lucene.Providers;
using Examine.Lucene.Search;
using Examine.Search;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Provides search and suggestion functionality for forum posts, including support for advanced query options, synonym
/// expansion, and analytics on search terms and zero-result queries.
/// </summary>
/// <remarks>The ForumSearchService enables searching forum posts by various criteria such as fields, author,
/// forum, phrase type, topic status, and date range. It also offers suggestions for forum post subjects based on
/// partial input. The service tracks analytics data, including frequently used search terms and queries that return no
/// results. Thread safety is not guaranteed; if used in a multi-threaded environment, external synchronization may be
/// required.</remarks>
public class ForumSearchService
{
    private readonly BaseLuceneSearcher _searcher;
    private readonly ISynonymProvider _synonyms;
    private readonly bool _debug;

    // Analytics
    private readonly Dictionary<string, int> _termCounts = new();
    private readonly List<string> _zeroResultQueries = new();

    public ForumSearchService(ISearcher searcher, ISynonymProvider synonyms, bool debugMode = false)
    {
        _searcher = (BaseLuceneSearcher)searcher;
        _synonyms = synonyms;
        _debug = debugMode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="queryTerms"></param>
    /// <param name="author"></param>
    /// <param name="forum"></param>
    /// <param name="phrasetype"></param>
    /// <param name="wheretopic"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public IEnumerable<ForumSearchResult> SearchPosts(
        string[] fields,
        string queryTerms,
        string author = null,
        string forum = "All",
        PhraseType phrasetype = PhraseType.Any, //Any, All,Exact
        WhereTopic? wheretopic = null, //Open, Closed, Solved
        DateTime? from = null, //after
        DateTime? to = null, //before
        int page = 1,
        int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(queryTerms))
            return Enumerable.Empty<ForumSearchResult>();

        TrackTermUsage(queryTerms);

        var terms = Split(queryTerms).ToArray();

        // Expand synonyms
        var allTerms = terms
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        switch (phrasetype)
        {
            case PhraseType.Phrase:
                allTerms = [queryTerms.Trim()];
                break;
            case PhraseType.All:
                allTerms = terms;
                break;
            case PhraseType.Any:
                allTerms = terms;
                break;
            default:
                allTerms = terms.Concat(terms.SelectMany(t => _synonyms.GetSynonyms(t))).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                break;
        }
        var lucene = BuildLuceneQuery(fields, allTerms, author, from, to,phrasetype,wheretopic,forum);

        if (_debug)
            Console.WriteLine($"[ForumSearch DEBUG] Lucene Query:\n{lucene}\n");
        var query = _searcher.CreateQuery("content", BooleanOperation.And, _searcher.LuceneAnalyzer, new LuceneSearchOptions() { AllowLeadingWildcard = true })
            .NativeQuery(lucene);

        var results = query.Execute();

        if (!results.Any())
            TrackZeroResultQuery(queryTerms);

        return results
            .OrderByDescending(hit=>hit.Score)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(hit => new ForumSearchResult
            {
                NodeId = int.TryParse(hit.Id, out var id) ? id : 0,
                Url = hit.Values.TryGetValue("url", out var url) ? url : "",
                Subject = hit.Values.TryGetValue("subject", out var subject) ? subject : "",
                MessageSnippet = HighlightTerms(hit.Values.TryGetValue("message", out var msg) ? MessageParser.ExtractMerged(MessageParser.StripMarkup(msg)) : "", allTerms),
                Score = hit.Score,
                ScoreExplanation = ExplainScore(hit, allTerms)
            });
    }

    /// <summary>
    /// Retrieves a collection of forum post suggestions that match the specified search term.
    /// </summary>
    /// <remarks>The returned suggestions are ordered by relevance and contain distinct subjects. This method
    /// performs a wildcard search on forum post subjects and is intended for use in autocomplete or search suggestion
    /// scenarios.</remarks>
    /// <param name="term">The search term to use for finding relevant forum post suggestions. Cannot be null or whitespace.</param>
    /// <param name="max">The maximum number of suggestions to return. Must be greater than zero. The default value is 10.</param>
    /// <returns>An enumerable collection of <see cref="ForumSuggestion"/> objects whose subjects match the search term. Returns
    /// an empty collection if no matches are found or if <paramref name="term"/> is null or whitespace.</returns>
    public IEnumerable<ForumSuggestion> GetSuggestions(string term, int max = 10)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Enumerable.Empty<ForumSuggestion>();

        var escaped = Escape(term);

        var lucene = $@"
            +(__NodeTypeAlias:forumPost)
            (subject:*{escaped}*)
            ";

        if (_debug)
            Console.WriteLine($"[ForumSearch DEBUG] Suggest Query:\n{lucene}\n");

        var query = _searcher.CreateQuery("content", BooleanOperation.And, _searcher.LuceneAnalyzer, new LuceneSearchOptions() { AllowLeadingWildcard = true })
            .NativeQuery(lucene);

        var results = query.Execute();

        return results
            .OrderByDescending(r => r.Score)
            .Select(r => new ForumSuggestion
            {
                Subject = r.Values.TryGetValue("subject", out var subject) ? subject : "",
                Url = r.Values.TryGetValue("url", out var url) ? url : ""
            })
            .Where(s => !string.IsNullOrWhiteSpace(s.Subject))
            .DistinctBy(s => s.Subject)
            .Take(max);
    }


    public IEnumerable<ForumSearchResult> GetForumsAllowingPosts()
    {
        var query = _searcher.CreateQuery("content", BooleanOperation.And, _searcher.LuceneAnalyzer, new LuceneSearchOptions() { AllowLeadingWildcard = true })
            .NativeQuery("+(__NodeTypeAlias:forum) AND (isActive:1) AND (postAtRoot:1)");

        return query.Execute().Select(hit => new ForumSearchResult
        {
            NodeId = int.TryParse(hit.Id, out var id) ? id : 0
        });
    }

    /// <summary>
    /// Builds a Lucene query string for searching forum posts based on the specified search terms, filters, and
    /// matching options.
    /// </summary>
    /// <remarks>The generated query string includes filters for author, forum, date range, and topic status
    /// if the corresponding parameters are provided. Search terms are matched using exact, wildcard, fuzzy, and phrase
    /// queries across the specified fields. The phrase matching behavior is determined by the value of the phrasetype
    /// parameter.</remarks>
    /// <param name="fields">The collection of field names to include in the search (for example, "subject" or "message").</param>
    /// <param name="terms">The array of search terms to match within the specified fields. Cannot be null or empty.</param>
    /// <param name="author">The author name to filter results by. If null or empty, no author filter is applied.</param>
    /// <param name="from">The earliest post date to include in the results, or null to include posts from any date.</param>
    /// <param name="to">The latest post date to include in the results, or null to include posts up to any date.</param>
    /// <param name="phrasetype">The phrase matching strategy to use when combining search terms. Determines whether all terms must match or any
    /// term is sufficient.</param>
    /// <param name="whereTopic">An optional topic status filter. If specified, restricts results to topics with the given status (such as open,
    /// closed, or solved).</param>
    /// <param name="forum">The forum identifier to filter results by. If null, empty, or set to "All", no forum filter is applied.</param>
    /// <returns>A Lucene query string representing the specified search criteria, suitable for use with a Lucene-based search
    /// engine.</returns>
    private string BuildLuceneQuery(string[] fields, IEnumerable<string> terms, string author, DateTime? from, DateTime? to, 
        PhraseType phrasetype, WhereTopic? whereTopic, string forum)
    {
        const int primaryBoost = 10;
        const int secondaryBoost = 5;

        var clauses = new List<string>();

        foreach (var term in terms)
        {
            var esc = Escape(term);

            clauses.Add($"{fields[0]}:{esc}^{primaryBoost}");// Exact
            clauses.Add($"{fields[0]}:*{esc}*^{primaryBoost - 1}");// Wildcard
            clauses.Add($"{fields[0]}:{esc}~2^{primaryBoost - 2}");// Fuzzy
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                
                clauses.Add($"{field}:{esc}^{secondaryBoost}");// Exact
                clauses.Add($"{field}:*{esc}*^{secondaryBoost - 1}");// Wildcard
                clauses.Add($"{field}:{esc}~2^{secondaryBoost - 2}");// Fuzzy
            }
        }

        // Phrase match
        var joined = string.Join(" ", terms.Select(Escape));
        if (terms.Count() > 1)
        {
            clauses.Add($"{fields[0]}:\"{joined}\"~2^{primaryBoost + 2}");// Exact
            for (var i = 0; i < fields.Length; i++)
            {
                clauses.Add($"{fields[i]}:\"{joined}\"~2^{secondaryBoost + 1}");
            }
        }

        string OrFields(string[] fields, IEnumerable<string> values)
        {
            return string.Join(" OR ",
                from field in fields
                from val in values
                select $"{field}:{val}");
        }
        string AndFields(string[] fields, IEnumerable<string> values)
        {
            //if we are doing an all then we need to OR subject and message but then AND the terms
            //(subject:wiz AND subject:forum) OR (message:wiz AND message:forum)

            var test = "(";
            for(int i = 0; i < fields.Length; i++)
            {
                var boost = i == 0 ? $"^{primaryBoost}"  : $"^{secondaryBoost}";
                test = test + string.Join(" AND ",
                from val in values
                select $"{fields[i]}:{val}{boost}");

                if (i < fields.Length-1)
                {
                    test = test + ") OR (";
                }
                else
                {
                    test = test + ")";
                }
            }
            return test;
        }


        var test = OrFields(fields, terms.ToArray());

        var orBlock = string.Join(" OR ", clauses.Select(c => $"({c})"));


        // Author filter
        var authorClause = !string.IsNullOrWhiteSpace(author)
            ? $"+(author:{Escape(author)})"
            : "";
        // Forum filter
        var forumClause = "";
        if (forum.HasValue() && forum != "All")
        {
            if (int.TryParse(forum, out var forumIdInt))
            {
                forumClause = $"+(forumid:[{forumIdInt} TO {forumIdInt}])";
            }
        }
        // Date range filter
        var dateClause = "";
        if (from.HasValue || to.HasValue)
        {
            if (from.HasValue)//before
            {
                var fromStr = ((DateTimeOffset)from).Ticks;// ?? long.MinValue;
                var toStr = long.MaxValue;

                dateClause = $"+(lastTicks:[{fromStr} TO {toStr}])";
            }
            if(to.HasValue)
            {
                var fromStr = long.MinValue;
                var toStr = ((DateTimeOffset)to).Ticks; //?? long.MaxValue;
                dateClause = $"+(lastTicks:[{fromStr} TO {toStr}])";
            }

        }
        var topicClause = "";
        if (whereTopic.HasValue)
        {
            if (whereTopic == WhereTopic.Open)
            {
                topicClause += "+(status:[1 TO 1])";
            }
            else if (whereTopic == WhereTopic.Closed)
            {
                topicClause += "+(status:[0 TO 0])";
            }
            else if (whereTopic == WhereTopic.Solved)
            {
                topicClause += "+(answered:[1 TO 1])";
            }
        }

        return $@"
        +(__NodeTypeAlias:forumPost)
        {topicClause}
        {forumClause}
        {authorClause}
        {dateClause}
        +({(phrasetype == PhraseType.All ? AndFields(fields, terms.ToArray()) : orBlock)})
        ";
    }

    // -------------------------
    // Analytics
    // -------------------------

    /// <summary>
    /// Tracks the usage frequency of each term found in the specified query string.
    /// </summary>
    /// <param name="query">The query string to analyze for term usage. Cannot be null.</param>
    private void TrackTermUsage(string query)
    {
        foreach (var term in Split(query))
        {
            if (_termCounts.ContainsKey(term))
                _termCounts[term]++;
            else
                _termCounts[term] = 1;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    private void TrackZeroResultQuery(string query)
    {
        _zeroResultQueries.Add(query);
    }

    /// <summary>
    /// Retrieves the most frequently occurring terms and their counts.
    /// </summary>
    /// <param name="count">The maximum number of top terms to return. Must be greater than zero. The default value is 20.</param>
    /// <returns>A read-only dictionary containing up to the specified number of terms and their corresponding counts, ordered by
    /// descending frequency. If there are fewer terms than requested, all available terms are returned.</returns>
    public IReadOnlyDictionary<string, int> GetTopTerms(int count = 20) =>
        _termCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(count)
            .ToDictionary(k => k.Key, v => v.Value);

    /// <summary>
    /// Gets a read-only list of queries that previously returned no results.
    /// </summary>
    /// <returns>A read-only list of strings containing queries that did not produce any results. The list is empty if no such
    /// queries have been recorded.</returns>
    public IReadOnlyList<string> GetZeroResultQueries() =>
        _zeroResultQueries.AsReadOnly();

    // -------------------------
    // Helpers
    // -------------------------

    /// <summary>
    /// Splits the specified string into an array of substrings based on spaces, removing empty entries and trimming
    /// whitespace from each substring.
    /// </summary>
    /// <param name="input">The string to split into substrings. If null or empty, returns an empty array.</param>
    /// <returns>An array of substrings from the input string, separated by spaces. The array does not contain empty entries, and
    /// each substring is trimmed of leading and trailing whitespace.</returns>
    private static string[] Split(string input) =>
        input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// Escapes special characters in the specified search term for use in a regular expression pattern.
    /// </summary>
    /// <param name="term">The input string containing the search term to be escaped. Cannot be null.</param>
    /// <returns>A string in which all special characters in the input term are escaped for safe use in a regular expression.</returns>
    private static string Escape(string term) =>
        Regex.Replace(term, @"([+\-!(){}\[\]^""~*?:\\])", @"\$1");

    /// <summary>
    /// Highlights all occurrences of the specified terms in the input text by wrapping them with <mark> tags.
    /// </summary>
    /// <remarks>Terms are matched as whole words using case-insensitive comparison. The method truncates the
    /// result to a maximum of 300 characters, appending an ellipsis if truncation occurs.</remarks>
    /// <param name="text">The input text in which to highlight terms. Cannot be null.</param>
    /// <param name="terms">A collection of terms to highlight within the text. Each term will be matched as a whole word,
    /// case-insensitively. Cannot be null.</param>
    /// <returns>A string with all occurrences of the specified terms wrapped in <mark> tags. If the resulting string exceeds 300
    /// characters, it is truncated and an ellipsis is appended.</returns>
    public static string HighlightTerms(string text, IEnumerable<string> terms, string tag = "mark", int maxLength = 300)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var raw = text;
        foreach (var term in terms.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            var pattern = Regex.Escape(term);
            raw = Regex.Replace(
                raw,
                $@"(\b{pattern}\b|{pattern})",
                $"<{tag}>$1</{tag}>",
                RegexOptions.IgnoreCase
            );
        }

        if (raw.Length <= maxLength)
            return raw;

        return raw.Substring(0, maxLength) + "...";
    }
    /// <summary>
    /// Generates a human-readable explanation of which search terms directly match values in the specified search
    /// result.
    /// </summary>
    /// <param name="hit">The search result to analyze for matching terms. Cannot be null.</param>
    /// <param name="terms">The collection of search terms to check for direct matches within the search result values. Cannot be null.</param>
    /// <returns>A string describing the matched terms. Returns a comma-separated list of matched terms if any are found;
    /// otherwise, returns a message indicating that no direct term matches were found.</returns>
    private static string ExplainScore(ISearchResult hit, IEnumerable<string> terms)
    {
        var matched = terms
            .Where(t => hit.Values.Values.Any(v => v.Contains(t, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        return matched.Any()
            ? $"Matched terms: {string.Join(", ", matched)}"
            : "No direct term matches found";
    }

}
