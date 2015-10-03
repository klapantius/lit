using System.IO;

namespace lit
{
    class TailFile :IFile
    {
        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}
