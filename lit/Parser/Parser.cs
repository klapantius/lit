using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace lit
{
    public class Parser : IParser
    {
        internal readonly List<IRule> rules;
        private readonly ITail tail;
        private readonly ConcurrentDictionary<string, string> record = new ConcurrentDictionary<string, string>();

        internal Parser(ITail logTail, IConfiguration configuration)
        {
            tail = logTail;
            rules = new List<IRule>();
            if (configuration != null)
            {
                rules = configuration.Rules;
            }
        }

        public void Run()
        {
            if (null == tail) return;
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
                matching = rules.LastOrDefault(r => r.IsMatching(line));
                if (null == matching) continue;
                var lineFields = matching.Parse(line);
                if (!matching.ActionOnly)
                {
                    foreach (var field in lineFields)
                    {
                        changed = !record.ContainsKey(field.Key) ||
                                  (record.ContainsKey(field.Key) && record[field.Key] != field.Value);
                        //Console.WriteLine("{2} {{([{0}]: \"{1}\"}} to record", field.Key, field.Value, record.ContainsKey(field.Key) ? "changing" : "adding");
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
        }


        public void Dispose()
        {
            Stop();
        }
    }

}
