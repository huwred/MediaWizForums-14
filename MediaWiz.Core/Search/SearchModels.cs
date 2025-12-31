/// <summary>
/// Represents a single result item returned from a forum search operation, including metadata and relevance
/// information.
/// </summary>
/// <remarks>This class encapsulates details about a forum post that matches a search query, such as its
/// identifier, subject, a snippet of its content, and its relevance score. Instances of this class are typically used
/// to display search results to users or to process search matches programmatically.</remarks>
public class ForumSearchResult
{
    public int NodeId { get; set; }
    public string Url { get; set; }
    public string Subject { get; set; }
    public string MessageSnippet { get; set; }
    public float Score { get; set; }
    public string ScoreExplanation { get; internal set; }
}
/// <summary>
/// Represents a suggestion for a forum topic, including its subject and associated URL.
/// </summary>
public class ForumSuggestion
{
    public string Subject { get; set; }
    public string Url { get; set; }
}
/// <summary>
/// Specifies the type of phrase matching to use when evaluating search queries or filters.
/// </summary>
/// <remarks>Use this enumeration to indicate whether a search should match an exact phrase, all terms, or any
/// term within the input. The meaning of each value is as follows: - Phrase: Matches the exact phrase as provided. -
/// All: Matches only if all terms are present, regardless of order. - Any: Matches if any term is present.  This
/// enumeration is commonly used in search or filtering scenarios to control how input text is interpreted.</remarks>
public enum PhraseType
{
    Phrase,
    All,
    Any
}
/// <summary>
/// Specifies the available options for sorting a collection of items, such as by relevance or chronological order.
/// </summary>
/// <remarks>Use this enumeration to indicate the desired sort order when retrieving or displaying items. The
/// meaning of each value depends on the context in which it is used; for example, 'Relevance' may be determined by a
/// search algorithm, while 'Newest' and 'Oldest' refer to the chronological order of items.</remarks>
public enum SortOrder
{
    Relevance,
    Newest,
    Oldest
}
/// <summary>
/// Specifies the scope of a search operation, indicating which types of content to include in search results.
/// </summary>
/// <remarks>Use this enumeration to control whether a search should include all content, only topics, or only
/// replies. The selected value determines the filtering applied during the search process.</remarks>
public enum SearchScope
{
    All,
    TopicsOnly,
    RepliesOnly
}
/// <summary>
/// Specifies the topic status used for filtering or categorizing items, such as open, closed, or solved topics.
/// </summary>
/// <remarks>Use this enumeration to indicate the current state of a topic when performing queries or applying
/// filters. The values represent common states that a topic may have in discussion or issue tracking systems.</remarks>
public enum WhereTopic
{
    Open,
    Closed,
    Solved
}
