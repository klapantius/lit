using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace lit
{
    internal class LogParserRule
    {
        private readonly Regex regex;

        public LogParserRule(string rule)
        {
            Rule = rule;
            regex = new Regex(Rule);
        }

        public string Rule { get; set; }

        public bool IsMatching(string line)
        {
            return regex.IsMatch(line);
        }

        public IDictionary<string, string> Parse(string line)
        {
            if (!IsMatching(line)) return null;

            var result = new ConcurrentDictionary<string, string>();
            var groups = regex.Match(line).Groups;
            foreach (var field in regex.GetGroupNames().Where(n => n != "0"))
            {
                while (!result.TryAdd(field, groups[field].Value))
                {
                    Thread.Sleep(1000);
                }
            }

            return result;
        }

    }
}
