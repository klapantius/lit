using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace lit
{
    [Serializable]
    public struct TransferOptions
    {
        [XmlElement("port")]
        public string Port;
    }

    interface IConfiguration
    {
        string InputFile { get; }
        TransferOptions Transfer { get; }
        List<IRule> Rules { get; }
    }
}
