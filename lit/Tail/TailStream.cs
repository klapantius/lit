using System.IO;

namespace lit
{
    class TailStream : IFileStream
    {
        private FileStream fs;

        public TailStream(string path, FileMode mode, FileAccess access, FileShare share)
        {
            fs = new FileStream(path, mode, access, share);
        }

        public long Length
        {
            get { return fs.Length; }
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return fs.Seek(offset, origin);
        }

        public int Read(byte[] array, int offset, int count)
        {
            return fs.Read(array, offset, count);
        }

        public long Position
        {
            get { return fs.Position; }
        }

        public void Close()
        {
            fs.Close();
        }

        public void Dispose()
        {
            if (null != fs) fs.Dispose();
            fs = null;
        }
    }
}
