using System;
using System.Collections.Generic;
using System.Linq;

public class InMemorySynonymProvider : ISynonymProvider
{
    private readonly Dictionary<string, string[]> _map =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["wiz"] = new[] { "wizard", "wizzy", "mediawiz", "mediawizards" },
            ["bug"] = new[] { "issue", "problem" }
        };

    public IEnumerable<string> GetSynonyms(string term) =>
        _map.TryGetValue(term, out var synonyms)
            ? synonyms
            : Enumerable.Empty<string>();
}

