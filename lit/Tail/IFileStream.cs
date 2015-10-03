using System;
using System.IO;

namespace lit
{
    internal interface IFileStream : IDisposable
    {
        long Length { get; }
        long Seek(long offset, SeekOrigin origin);
        int Read(byte[] array, int offset, int count);
        long Position { get; }
        void Close();
    }
}
