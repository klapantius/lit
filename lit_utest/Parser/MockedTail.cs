using System.Linq;
using lit;
using System.Collections.Generic;

namespace lit_utest.ParserTests
{
    class MockedTail : ITail
    {
        private readonly List<string> myLines;

        public MockedTail(params string[] initialLines)
        {
            myLines = initialLines.ToList();
        }

        public List<string> GetNewLines()
        {
            return myLines;
        }

        public event ChangedEventHandler Changed;

        protected virtual void OnChanged(TailEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        internal void AddLines(params string[] newLines)
        {
            OnChanged(new TailEventArgs(newLines.ToList()));
        }

        public void Watch()
        {
            OnChanged(new TailEventArgs(myLines.ToList()));
        }

        public void StopWatching()
        {
        }

        public bool IsWaitingForChanges { get; internal set; }

    }
}
