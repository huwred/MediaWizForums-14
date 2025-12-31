using System.Collections.Generic;

public interface ISynonymProvider
{
    IEnumerable<string> GetSynonyms(string term);
}

