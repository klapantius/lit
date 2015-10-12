using System;
using System.Linq;
using System.Text.RegularExpressions;
using lit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace lit_utest.ParserTests
{
    [TestClass]
    public class RuleTests
    {
        [TestMethod]
        public void IsMatchingCanFalse()
        {
            var testObject = new Rule(@"a specific text");
            Assert.IsFalse(testObject.IsMatching("something absolutely else"), "Unexpected result while comparing to an unmatching string.");
        }

        [TestMethod]
        public void IsMatchingCanTrue()
        {
            const string line = "aki nem lep egyszerre nem kap retest estere";
            var testObject = new Rule(line);
            Assert.IsTrue(testObject.IsMatching(line), "Unexpected result while comparing to an  absolutly matching string.");
        }

        [TestMethod]
        public void ParseReturnsEmptyIfItIsntMatching()
        {
            var testObject = new Rule(@"a specific text");
            var result = testObject.Parse("something absolutely else");
            Assert.IsNotNull(result, "Unexpected result while parsing an unmatching string.");
            Assert.AreEqual(0, result.Count, "Parse must not return any items if it is not matching.");
        }

        [TestMethod]
        public void ParseCanReturnDictionary()
        {
            var testObject = new Rule(@"(?<first>\d\w\w),(?<second>\d\w\w),(?<third>\d\w\w)");
            Assert.IsTrue(testObject.IsValid, "Unexpected validation result on valid regex pattern.");
            var result = testObject.Parse("1st,2nd,3rd");
            Assert.IsNotNull(result, "No result dictionary created.");
            Assert.AreEqual(string.Empty, testObject.ErrorMessage, "Unexpected error message.");
            Assert.IsTrue(result.ContainsKey("first"), "Key \"first\" cound not be found.");
            Assert.IsTrue(result.ContainsKey("second"), "Key \"second\" cound not be found.");
            Assert.IsTrue(result.ContainsKey("third"), "Key \"third\" cound not be found.");
            Assert.AreEqual("1st", result["first"], "Wrong value for key \"first\"");
            Assert.AreEqual("2nd", result["second"], "Wrong value for key \"second\"");
            Assert.AreEqual("3rd", result["third"], "Wrong value for key \"third\"");
        }

        [TestMethod]
        public void CanValidatePattern()
        {
            IRule testObject = null;
            try
            {
                testObject = new Rule(@"Invalid regex pattern [{(");
            }
            catch (Exception ex)
            {
                Assert.Fail("Constructor must not throw exception on invalid pattern. {0} caught.", ex.GetType().Name);
            }
            Assert.IsFalse(testObject.IsValid, "Unexcpected validation result on invalid regex pattern.");
            Assert.IsNotNull(testObject.ErrorMessage, "Error message is missing.");
            Assert.AreNotEqual(string.Empty, testObject.ErrorMessage, "Error message is missing.");
            Console.WriteLine("Correct error message detected: {0}", testObject.ErrorMessage);
            try
            {
                Assert.IsFalse(testObject.IsMatching("alma"), "Unexpected matching result while regex pattern is invalid.");
            }
            catch (Exception ex)
            {
                Assert.Fail("IsMatching() must not throw exception. {0} caught.", ex.GetType().Name);
            }
            try
            {
                var result = testObject.Parse("foo bar");
                Assert.IsNotNull(result, "Unexpected parsing result while regex pattern is invalid.");
                Assert.AreEqual(0, result.Count, "Unexpected parsing result while regex pattern is invalid.");
            }
            catch (Exception ex)
            {
                Assert.Fail("Parse() must not throw exception. {0} caught.", ex.GetType().Name);
            }
        }

        [TestMethod]
        public void KeyIsDateTimeIfThereIsNoGroupNameInRegex()
        {
            IRule testObject = new Rule(@"\d+");
            var result = testObject.Parse("foo 1234 bar");
            Assert.AreEqual(1, result.Count, "Unexpected count of results.");
            Assert.AreEqual("1234", result[result.Keys.First()], "Unexpected result.");
            StringAssert.StartsWith(result.Keys.First(), DateTime.Now.ToString(Rule.DefaultKeyFormat.Substring(0, 10)),
                "Unexpected key value.");
            Assert.IsTrue(new Regex(@"20\d\d[0,1]\d[0,1,2,3]\d[0,1,2]\d+").IsMatch(result.Keys.First()),
                string.Format("Generated key doesn't look like a date/time string: {0}", result.Keys.First()));
        }

    }
}
