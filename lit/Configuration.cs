using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace lit
{
    public class Configuration : IConfiguration
    {
        [Serializable, XmlRoot("configuration")]
        public struct ConfigurationData
        {
            [XmlElement("file")]
            public string InputFile;

            [XmlElement("transfer")]
            public TransferOptions Transfer;

            [XmlArray("rules")]
            [XmlArrayItem("rule")]
            public List<Rule> Rules;

        }

        private ConfigurationData configuration;

        public string InputFile { get { return configuration.InputFile; } }

        public TransferOptions Transfer { get { return configuration.Transfer; } }

        public List<IRule> Rules { get { return configuration.Rules.Select(r => (IRule)r).ToList(); } }

        public Configuration(string configFile)
        {
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            using (var stream = new FileStream(configFile, FileMode.Open))
            {
                configuration = (ConfigurationData)serializer.Deserialize(stream);
            }
        }

    }
}
