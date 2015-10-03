using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using lit;

namespace lit_utest.TailTests
{
    class MockedFileStream : IFileStream
    {
        private byte[] myContent;

        public MockedFileStream(byte[] content)
        {
            myContent = new byte[content.Length];
            content.CopyTo(myContent, 0);
        }

        public MockedFileStream(string content, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            var bytes = encoding.GetBytes(content + Environment.NewLine);
            if (Equals(encoding, Encoding.ASCII))
            {
                myContent = new byte[bytes.Length];
                bytes.CopyTo(myContent, 0);
                return;
            }
            myContent = new byte[2 + bytes.Length];
            if (Equals(encoding, Encoding.Unicode))
            {
                myContent[0] = 255;
                myContent[1] = 254;
            }
            else if (Equals(encoding, Encoding.UTF8))
            {
                myContent[0] = 239;
                myContent[1] = 187;
            }
            else if (Equals(encoding, Encoding.BigEndianUnicode))
            {
                myContent[0] = 254;
                myContent[1] = 255;
            }
            bytes.CopyTo(myContent, 2);
        }

        public MockedFileStream(List<string> content, Encoding encoding = null)
            : this(content.First(), encoding)
        {
            content.Skip(1).ToList().ForEach(l => this.AddNewLine(l, encoding));
        }

        public void AddItems(byte[] newItems)
        {
            var oriLength = null != myContent ? myContent.Length : 0;
            var items = new byte[oriLength + newItems.Length];
            if (null != myContent) myContent.CopyTo(items, 0);
            newItems.CopyTo(items, oriLength);
            myContent = items;
        }

        public void AddNewLine(string newContent, Encoding encoding = null)
        {
            if (null == encoding) encoding = Encoding.ASCII;
            AddItems(encoding.GetBytes(newContent + Environment.NewLine));
        }

        public void AddNewLines(List<string> lines, Encoding encoding)
        {
            lines.ForEach(l => AddNewLine(l, encoding));
        }

        public void Reset()
        {
            myContent = null;
            Position = 0;
        }

        public long Length
        {
            get { return myContent.Length; }
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = Math.Min(Length, offset);
                    break;
                case SeekOrigin.Current:
                    Position = Math.Min(Length, Position + offset);
                    break;
                case SeekOrigin.End:
                    Position = Math.Max(0, Length + offset);
                    break;
            }
            return Position;
        }

        public int Read(byte[] array, int offset, int count)
        {
            if (null == myContent) return 0;
            var i = 0;
            Seek(offset, SeekOrigin.Current);
            while (Position < Length && i < count) array[i++] = myContent[Position++];
            return i + 1;
        }

        public long Position { get; set; }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
