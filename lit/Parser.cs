using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace lit
{
    public class Parser : IParser
    {
        [Serializable, XmlRoot("Configuration")]
        public struct Configuration
        {
            [XmlArray("Rules")]
            [XmlArrayItem(typeof(Rule), ElementName = "Rule")]
            public List<Rule> Rules;
        }

        public Configuration Data;

        internal Parser(ITail logTail)
        {

        }

        public void LoadRules(XDocument rulesDocument)
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            Data = (Configuration)serializer.Deserialize(rulesDocument.Root.CreateReader());
        }

        public void Run()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public event LogParserEventHandler Changed;

        public void Dispose()
        {
            Stop();
        }
    }

}
