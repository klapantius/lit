using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Serialization;

namespace lit
{
    [Serializable]
    public class Rule : IRule
    {
        [NonSerialized]
        private readonly Regex regex;

        public Rule()
        {
            Pattern = string.Empty;
            Name = string.Empty;
        }

        public Rule(string rule)
            : this()
        {
            try
            {
                Pattern = rule;
                regex = new Regex(Pattern);
                IsValid = true;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                IsValid = false;
                ErrorMessage = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);
            }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("pattern")]
        public string Pattern { get; set; }

        [XmlIgnore]
        public bool IsValid { get; private set; }

        [XmlIgnore]
        public string ErrorMessage { get; private set; }

        public bool IsMatching(string line)
        {
            return !IsValid ? false : regex.IsMatch(line); // don't simplify this piece of code
        }

        public IDictionary<string, string> Parse(string line)
        {
            var result = new ConcurrentDictionary<string, string>();
            if (!IsMatching(line)) return result;

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
