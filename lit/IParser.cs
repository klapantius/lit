using System;
using System.Xml.Linq;

namespace lit
{
    public delegate void LogParserEventHandler(string record);

    interface IParser : IDisposable
    {
        void LoadRules(XDocument rulesDocument);
        void Run();
        void Stop();

        event LogParserEventHandler Changed;
    }
}
