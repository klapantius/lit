using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using lit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace lit_utest.ParserTests
{
    [TestClass]
    public class ParserTests
    {
        private static readonly XDocument SimpleRuleSet = new XDocument(
            new XElement("Configuration",
                new XElement("Rules",
                    new XElement("Rule",
                        new XAttribute("name", "firstrule"),
                        new XAttribute("pattern", "foo")),
                    new XElement("Rule",
                        new XAttribute("name", "secondrule"),
                        new XAttribute("pattern", "bar"))
            )));

        private static readonly XDocument BlackHoleRuleSet = new XDocument(
            new XElement("Configuration",
                new XElement("Rules",
                    new XElement("Rule",
                        new XAttribute("name", "blackhole")),
                    new XAttribute("pattern", ".*")
                    )));

        [TestMethod]
        public void ParserCanLoadRules()
        {
            var testObject = new Parser(null);
            Assert.IsNull(testObject.Data.Rules, "Ctor must not set any rules.");
            testObject.LoadRules(SimpleRuleSet);
            Assert.IsNotNull(testObject.Data.Rules, "Rules could not be loaded.");
            Assert.AreEqual(2, testObject.Data.Rules.Count, "Missing rules.");
        }

        [TestMethod]
        public void ParserCanSendLines()
        {
            var initialLines = new[]
            {
                "foo",
                "bar",
                "xyz",
            };
            var testObject = new Parser(new MockedTail(initialLines));
            var receivedLines = new List<string>();
            testObject.Changed += receivedLines.Add;
            testObject.LoadRules(BlackHoleRuleSet);
            testObject.Run();
            Assert.AreEqual(3, receivedLines, "Missing lines.");
        }

    }
}
