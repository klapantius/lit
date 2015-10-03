using lit;

namespace lit_utest.TailTests
{
    class MockedFile : IFile
    {
        public bool Exists(string path)
        {
            return true;
        }
    }
}
