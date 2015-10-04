using System.Collections.Generic;

namespace lit
{
    interface IRule
    {
        string Pattern { get; }
        bool IsValid { get; }
        bool IsMatching(string line);
        string ErrorMessage { get; }
        IDictionary<string, string> Parse(string line);
    }
}
