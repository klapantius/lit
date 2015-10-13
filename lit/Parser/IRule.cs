using System.Collections.Generic;

namespace lit
{
    interface IRule
    {
        string Name { get; }
        string Pattern { get; }
        string Clean { get; }
        bool ActionOnly { get; }
        bool IsValid { get; }
        bool IsMatching(string line);
        string ErrorMessage { get; }
        IDictionary<string, string> Parse(string line);
    }
}
