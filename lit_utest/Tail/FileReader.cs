using lit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace lit_utest.TailTests
{
    [TestClass]
    public class FileReaderTest
    {
        private const string DummyFileName = "dummyFileName";
        [TestMethod]
        public void ReadLastNBytes_GetLastTwo()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader(DummyFileName) { myFileStream = mockedFileStream, File = new MockedFile() };

            var lastTwoBytes = reader.ReadLastNBytes(2);
            Assert.AreEqual(2, lastTwoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(4, lastTwoBytes[0], "Unexpected result.");
            Assert.AreEqual(5, lastTwoBytes[1], "Unexpected result.");
        }
        [TestMethod]
        public void ReadLastNBytes_GetMoreThanPossible()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader(DummyFileName) { myFileStream = mockedFileStream, File = new MockedFile() };

            var last7Bytes = reader.ReadLastNBytes(7);
            Assert.AreEqual(5, last7Bytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(1, last7Bytes[0], "Unexpected result.");
            Assert.AreEqual(5, last7Bytes[4], "Unexpected result.");
        }

        [TestMethod]
        public void ReadNewBytes_ReadThrough5ItemsBy2()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader(DummyFileName) { myFileStream = mockedFileStream, File = new MockedFile() };

            var twoBytes = reader.ReadNewBytes(2);
            Assert.AreEqual(2, twoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(1, twoBytes[0], "Unexpected result after 1st 2 bytes read.");
            Assert.AreEqual(2, twoBytes[1], "Unexpected result after 1st 2 bytes read.");

            twoBytes = reader.ReadNewBytes(2);
            Assert.AreEqual(2, twoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(3, twoBytes[0], "Unexpected result after 2nd 2 bytes read.");
            Assert.AreEqual(4, twoBytes[1], "Unexpected result after 2nd 2 bytes read.");

            twoBytes = reader.ReadNewBytes(2);
            Assert.AreEqual(1, twoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(5, twoBytes[0], "Unexpected result after 3rd 2 bytes read.");
        }

        [TestMethod]
        public void ReadNewBytes_ContinuousReadingAfterNewItemsAdded()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader(DummyFileName) { myFileStream = mockedFileStream, File = new MockedFile() };

            var bytes = reader.ReadNewBytes();
            Assert.AreEqual(5, bytes.Length, "Wrong number of returned bytes in the 1st round.");
            Assert.AreEqual(1, bytes[0], "Unexpected result in the 1st round.");
            Assert.AreEqual(5, bytes[4], "Unexpected result in the 1st round.");

            mockedFileStream.AddItems(new byte[] { 7, 8, 9 });
            bytes = reader.ReadNewBytes();
            Assert.AreEqual(3, bytes.Length, "Wrong number of returned bytes in the 2nd round.");
            Assert.AreEqual(7, bytes[0], "Unexpected result in the 2nd round.");
            Assert.AreEqual(8, bytes[1], "Unexpected result in the 2nd round.");
            Assert.AreEqual(9, bytes[2], "Unexpected result in the 2nd round.");

        }

        [TestMethod]
        public void ReadNewBytes_ContinuousReadingAfterSourceReset()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader(DummyFileName) { myFileStream = mockedFileStream, File = new MockedFile() };

            var bytes = reader.ReadNewBytes();
            Assert.AreEqual(5, bytes.Length, "Wrong number of returned bytes in the 1st round.");
            Assert.AreEqual(1, bytes[0], "Unexpected result in the 1st round.");
            Assert.AreEqual(5, bytes[4], "Unexpected result in the 1st round.");

            mockedFileStream.Reset();
            mockedFileStream.AddItems(new byte[] { 7, 8, 9 });
            bytes = reader.ReadNewBytes();
            Assert.AreEqual(3, bytes.Length, "Wrong number of returned bytes in the 2nd round.");
            Assert.AreEqual(7, bytes[0], "Unexpected result in the 2nd round.");
            Assert.AreEqual(8, bytes[1], "Unexpected result in the 2nd round.");
            Assert.AreEqual(9, bytes[2], "Unexpected result in the 2nd round.");

        }

    }
}
