using System.Collections.Generic;

namespace lit
{
    interface ILogParserRule
    {
        string Rule { get; }

        bool IsMatching(string line);
        IDictionary<string, string> Parse(string line);
    }
}
