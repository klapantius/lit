using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace lit
{
    internal class Tail : ITail
    {
        internal IFileReader myFileReader;
        internal Encoding myFileType = Encoding.Default;
        internal bool encodingDetected = false;
        private bool isWaiting = false;
        public bool IsWaitingForChanges
        {
            get
            {
                isWaiting = isWaiting || (myFileReader != null && myFileReader.IsAtTheEof);
                return isWaiting;
            }
        }

        internal Tail(string fileName, Encoding fileType, IFileReader fileReader)
        {
            myFileReader = fileReader;
            Init(fileName, fileType);
        }

        public Tail(string fileName, Encoding fileType)
        {
            Init(fileName, fileType);
        }

        private void Init(string fileName, Encoding fileType)
        {
            if (null == myFileReader) myFileReader = new FileReader(fileName);
            if (!Equals(fileType, Encoding.Default))
            {
                myFileType = fileType;
            }
            else
            {
                try
                {
                    DetectEncoding();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myFileType = Encoding.ASCII;
                }
            }
        }

        private void DetectEncoding()
        {
            var b = myFileReader.ReadNewBytes(2);
            if (b == null || b.Length < 2)
            {
                myFileReader.ResetPosition();
                return;
            }

            if (b[0] == 255 && b[1] == 254)
            {
                myFileType = Encoding.Unicode;
            }
            else if (b[0] == 239 && b[1] == 187)
            {
                myFileType = Encoding.UTF8;
            }
            else if (b[0] == 254 && b[1] == 255)
            {
                myFileType = Encoding.BigEndianUnicode;
            }
            else
            {
                myFileType = Encoding.ASCII;
                myFileReader.ResetPosition();
            }
            encodingDetected = true;
        }

        public List<string> GetNewLines()
        {
            var buf = myFileReader.ReadNewBytes();
            if (buf == null || buf.Length == 0) return new List<string>();
            buf = Encoding.Convert(myFileType, Encoding.ASCII, buf);
            var sbOutstring = new StringBuilder();
            foreach (var b in buf)
            {
                sbOutstring.Append(Convert.ToChar(b));
            }
            return sbOutstring.ToString().Split(Environment.NewLine.ToCharArray()).Where(line => line.Trim().Length > 0).ToList();
        }

        #region changed event
        public event ChangedEventHandler Changed;

        protected virtual void OnChanged(TailEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        #endregion

        #region watching
        private FileSystemWatcher fsWatcher;
        private Timer watchDog;

        public void StopWatching()
        {
            if (null != fsWatcher) fsWatcher.EnableRaisingEvents = false;
        }

        public void Watch()
        {
            if (null != fsWatcher && fsWatcher.EnableRaisingEvents) return;

            var dir = Path.GetDirectoryName(myFileReader.FileName);
            if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;
            var fname = Path.GetFileName(myFileReader.FileName);
            //Console.WriteLine("initiating a FileSystemWatcher using {0} and {1}", dir, fname);
            fsWatcher = new FileSystemWatcher(dir, fname);
            fsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.Size;
            fsWatcher.Changed += fsWatcher_Changed;
            fsWatcher.Created += fsWatcher_Changed;
            fsWatcher.Deleted += fsWatcher_Changed;
            fsWatcher.Renamed += fsWatcher_Changed;
            fsWatcher.EnableRaisingEvents = true;

            watchDog = new Timer(5000) { AutoReset = true, Enabled = true };
            watchDog.Elapsed += (sender, args) => fsWatcher_Changed(sender, new FileSystemEventArgs(WatcherChangeTypes.Changed, dir, fname));

            Console.WriteLine("Tailing {0}\\{1} ", dir, fname);
            if (!File.Exists(Path.Combine(dir, fname)))
            {
                Console.WriteLine("The file doesn't exist at the moment.");
            }
            else
            {
                Console.WriteLine("Last changed: {0}", File.GetLastWriteTime(Path.Combine(dir, fname)));
            }
        }

        void fsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            //if (e.ChangeType != WatcherChangeTypes.Changed && e.ChangeType != WatcherChangeTypes.Created) return;
            if (!encodingDetected)
            {
                DetectEncoding();
                if (!encodingDetected) return;
            }

            var newLines = GetNewLines();
            if (newLines.Count > 0) OnChanged(new TailEventArgs(newLines));
        }

        #endregion

    }
}
