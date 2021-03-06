﻿using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace lit
{
    public delegate void LogParserEventHandler(IDictionary<string, string> record);

    interface IParser : IDisposable
    {
        void Run();
        void Stop();

        event LogParserEventHandler Changed;
    }
}
