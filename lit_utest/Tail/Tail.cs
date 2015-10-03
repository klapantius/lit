using System.Linq;
using System.Text.RegularExpressions;
using lit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;

namespace lit_utest.TailTests
{
    /// <summary>
    /// Summary description for Tail
    /// </summary>
    [TestClass]
    public class TailTests
    {
        public TailTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        private const string FIRSTLINE = "alma a fa alatt";
        private Regex firstLineRegex { get { return new Regex(FIRSTLINE); } }

        [TestMethod]
        public void GetNewLines()
        {
            var source = new MockedFileStream(FIRSTLINE);
            var reader = new FileReader("dummyFileName") { myFileStream = source, File = new MockedFile() };

            var tail = new Tail("dummyFileName", Encoding.Default, reader);

            Assert.AreEqual(Encoding.ASCII, tail.myFileType, "Encoding detected wrong.");
            var lines = tail.GetNewLines();
            Assert.AreEqual(1, lines.Count, "Unexpected count of lines read.");
            StringAssert.Matches(lines.First(), firstLineRegex, "Wrong content.");
        }

        [TestMethod]
        public void GetNewLines_ContinuousReading()
        {
            var input = new List<string>
            {
                "first line",
                "second line",
                "third line"
            };
            var additionalLines = new List<string>
            {
                "fourth line",
                "fifth line",
                "sixth line",
                "7th line"
            };
            var source = new MockedFileStream(input);
            var reader = new FileReader("dummyFileName") { myFileStream = source, File = new MockedFile() };
            var tail = new Tail("dummyFileName", Encoding.Default, reader);

            var output = tail.GetNewLines();
            Assert.AreEqual(input.Count, output.Count, "Unexpected count of lines at start");
            for (var i = 0; i < input.Count; i++)
            {
                StringAssert.Matches(output[i], new Regex(input[i]), "Unmatching line in the 1st round.");
            }

            source.AddNewLines(additionalLines, Encoding.Default);
            output = tail.GetNewLines();
            Assert.AreEqual(additionalLines.Count, output.Count, "Unexpected count of lines after new lines added.");
            for (var i = 0; i < additionalLines.Count; i++)
            {
                StringAssert.Matches(output[i], new Regex(additionalLines[i]), "Unmatching line in the 2nd round.");
            }
        }
    }
}
