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
        public const string DefaultKeyFormat = "yyyyMMddHHmmssfff";

        [NonSerialized]
        private Regex _regex;

        [NonSerialized]
        private bool isValidated = false;

        [NonSerialized]
        private bool isValid = false;

        public Rule()
        {
            Pattern = string.Empty;
            Name = string.Empty;
        }

        public Rule(string rule)
            : this()
        {
            Pattern = rule;
            ValidatePattern();
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("pattern")]
        public string Pattern { get; set; }

        [XmlAttribute("clean")]
        public string Clean { get; set; }

        [NonSerialized]
        private bool? actionOnly;

        [XmlAttribute("actiononly")]
        public bool ActionOnly
        {
            get { return actionOnly.HasValue ? actionOnly.Value : false; }
            set { actionOnly = value; }
        }

        [XmlIgnore]
        public bool IsValid
        {
            get
            {
                if (!isValidated)
                {
                    ValidatePattern();
                }
                return isValid;
            }
        }

        [XmlIgnore]
        public string ErrorMessage { get; private set; }

        public bool IsMatching(string line)
        {
            return !IsValid ? false : _regex.IsMatch(line); // don't simplify this piece of code
        }

        private void ValidatePattern()
        {
            try
            {
                _regex = new Regex(Pattern);
                isValid = true;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                isValid = false;
                ErrorMessage = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);
            }
            isValidated = true;
        }

        public IDictionary<string, string> Parse(string line)
        {
            var result = new ConcurrentDictionary<string, string>();
            if (!IsMatching(line)) return result;

            var groups = _regex.Match(line).Groups;
            var groupnames = _regex.GetGroupNames().Where(n => n != "0").ToList();
            if (groupnames.Count > 0)
            {
                foreach (var field in groupnames)
                {
                    while (!result.TryAdd(field, groups[field].Value))
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            else
            {
                result.TryAdd(DateTime.Now.ToString(DefaultKeyFormat) + "_" + Guid.NewGuid(), _regex.Match(line).Value);
            }

            return result;
        }

    }
}
