using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace lit
{
    [Serializable]
    public struct TransferOptions
    {
        [XmlElement("prefix")]
        public string Prefix;
    }

    interface IConfiguration
    {
        string InputFile { get; }
        TransferOptions Transfer { get; }
        List<IRule> Rules { get; }
    }
}
