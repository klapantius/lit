using System.Collections.Generic;

namespace lit
{
    internal delegate void ChangedEventHandler(object sender, TailEventArgs e);

    internal interface ITail
    {
        event ChangedEventHandler Changed;
        List<string> GetNewLines();
        void Watch();
        void StopWatching();
        bool IsWaitingForChanges { get; }
    }
}
