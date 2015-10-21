using System;
using System.Collections.Generic;
using System.IO;
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
            public List<IRule> Rules;

        }

        private ConfigurationData configuration;

        public string InputFile { get { return configuration.InputFile; } }

        public TransferOptions Transfer { get { return configuration.Transfer; } }

        public List<IRule> Rules { get { return configuration.Rules; } }

        public Configuration(string configFile)
        {
            var serializer = new XmlSerializer(typeof(ConfigurationData));
            configuration = (ConfigurationData)serializer.Deserialize(new FileStream(configFile, FileMode.Open));
            throw new NotImplementedException();
        }

    }
}
