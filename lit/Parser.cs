using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace lit
{
    public class Parser : IParser
    {
        [Serializable, XmlRoot("Configuration")]
        public struct ConfigurationData
        {
            [XmlArray("Rules")]
            [XmlArrayItem(typeof(Rule), ElementName = "Rule")]
            public List<Rule> Rules;
        }

        public ConfigurationData Configuration;
        private readonly ITail tail;
        private readonly ConcurrentDictionary<string, string> record = new ConcurrentDictionary<string, string>();
        //private bool init = true;

        internal Parser(ITail logTail)
        {
            tail = logTail;
        }

        public void LoadRules(XDocument rulesDocument)
        {
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            Configuration = (ConfigurationData)serializer.Deserialize(rulesDocument.Root.CreateReader());
        }

        public void Run()
        {
            tail.Changed += TailUpdateHandler;
            tail.Watch();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        #region changed event
        public event LogParserEventHandler Changed;

        protected virtual void OnChanged(IDictionary<string, string> rec)
        {
            if (Changed != null) Changed(rec);
        }

        #endregion

        public void TailUpdateHandler(object o, TailEventArgs e)
        {
            bool changed = false;
            foreach (var line in e.NewLines)
            {
                IRule matching;
                try
                {
                    matching = Configuration.Rules.SingleOrDefault(r => r.IsMatching(line));
                }
                catch (InvalidOperationException exception)
                {
                    if (exception.Message.Contains("more than one"))
                    {
                        var rules = Configuration.Rules.Where(r => r.IsMatching(line)).Select(r => string.IsNullOrEmpty(r.Name) ? r.Pattern : r.Name);
                        Console.WriteLine("Rule conflict!!! These rules: {0}{1}{0}are all matching to this line:{0}{2}",
                            Environment.NewLine, string.Join(Environment.NewLine, rules), line);
                    }
                    else
                    {
                        Console.WriteLine("{0} caught (\"{1}\") while tryint to parser line: \"{2}\"", exception.GetType().Name, exception.Message, line);
                    }
                    matching = Configuration.Rules.FirstOrDefault(r => r.IsMatching(line));
                }
                if (null == matching) continue;
                var lineFields = matching.Parse(line);
                if (!matching.ActionOnly)
                {
                    foreach (var field in lineFields)
                    {
                        changed = !record.ContainsKey(field.Key) ||
                                  (record.ContainsKey(field.Key) && record[field.Key] != field.Value);
                        Console.WriteLine("{2} {{([{0}]: \"{1}\"}} to record", field.Key, field.Value, record.ContainsKey(field.Key) ? "changing" : "adding");
                        record[field.Key] = field.Value;
                    }
                    
                }
                if (!string.IsNullOrEmpty(matching.Clean))
                {
                    matching.Clean.Split(',').Select(f => f.Trim()).ToList().ForEach(field =>
                    {
                        if (record.ContainsKey(field))
                        {
                            record[field] = string.Empty;
                        }
                    });
                }
            }
            if (changed)
            {
                OnChanged(record);
            }
            //if (init)
            //{
            //    init = false;
            //    if (null != result) OnChanged(result);
            //}
        }


        public void Dispose()
        {
            Stop();
        }
    }

}
