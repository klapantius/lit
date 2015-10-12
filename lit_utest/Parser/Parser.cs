using lit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Serialization;

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
                        new XAttribute("pattern", "bar.*"))
            )));

        private static readonly XDocument BlackHoleRuleSet = new XDocument(
            new XElement("Configuration",
                new XElement("Rules",
                    new XElement("Rule",
                        new XAttribute("name", "blackhole"),
                        new XAttribute("pattern", ".*"))
                    )));

        private static readonly XDocument NamedGroupsRuleSet = new XDocument(
            new XElement("Configuration",
                new XElement("Rules",
                    new XElement("Rule",
                        new XAttribute("name", "firstrule"),
                        new XAttribute("pattern", @"foo_(?<foo>\d+)")),
                    new XElement("Rule",
                        new XAttribute("name", "secondrule"),
                        new XAttribute("pattern", @"bar_(?<bar>\d+)"),
                        new XAttribute("actiononly", "false"))
            )));

        private static readonly XDocument FieldCleanerRuleSet = new XDocument(
            new XElement("Configuration",
                new XElement("Rules",
                    new XElement("Rule",
                        new XAttribute("name", "firstrule"),
                        new XAttribute("pattern", @"foo_(?<foo>\d+)")),
                    new XElement("Rule",
                        new XAttribute("name", "secondrule"),
                        new XAttribute("pattern", @"bar_(?<bar>\d+)")),
                    new XElement("Rule",
                        new XAttribute("name", "thirdrule"),
                        new XAttribute("pattern", @"xyz_(?<xyz>\d+)")),
                    new XElement("Rule",
                        new XAttribute("name", "cleanerrule"),
                        new XAttribute("pattern", @"mr.proper"),
                        new XAttribute("clean", "foo,xyz"),
                        new XAttribute("actiononly", "true"))
            )));

        [TestMethod]
        public void t()
        {
            var c = new Parser.ConfigurationData()
            {
                Rules = new List<Rule>()
                {
                    new Rule("foo") {Name = "r1"},
                    new Rule("bar") {Name = "r2"},
                }
            };
            var xd = new XDocument();
            using (var writer = xd.CreateWriter())
            {
                // write xml into the writer
                var serializer = new XmlSerializer(c.GetType());
                serializer.Serialize(writer, c);
            }
            Console.WriteLine(xd.ToString());
        }

        [TestMethod]
        public void ParserCanLoadRules()
        {
            var testObject = new Parser(null);
            Assert.IsNull(testObject.Configuration.Rules, "Ctor must not set any rules.");
            testObject.LoadRules(SimpleRuleSet);
            Assert.IsNotNull(testObject.Configuration.Rules, "Rules could not be loaded.");
            Assert.AreEqual(2, testObject.Configuration.Rules.Count, "Missing rules.");
            Assert.AreEqual("firstrule", testObject.Configuration.Rules[0].Name);
            Assert.AreEqual("foo", testObject.Configuration.Rules[0].Pattern);
            testObject.LoadRules(BlackHoleRuleSet);
            Assert.IsNotNull(testObject.Configuration.Rules, "Rules could not be loaded.");
            Assert.AreEqual(1, testObject.Configuration.Rules.Count, "Missing rules.");
            Assert.AreEqual("blackhole", testObject.Configuration.Rules[0].Name);
            Assert.AreEqual(".*", testObject.Configuration.Rules[0].Pattern);
        }

        [TestMethod]
        public void ParserCanSendResult()
        {
            var initialLines = new[]
            {
                "foo",
                "bar",
                "xyz",
            };
            var testObject = new Parser(new MockedTail(initialLines));
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            var events = 0;
            testObject.Changed += (rec) =>
            {
                events += 1;
                testRecord = rec;
            };
            testObject.LoadRules(BlackHoleRuleSet);
            testObject.Run();
            Assert.AreEqual(1, events, "Unexpected count of received events.");
            Assert.AreEqual(initialLines.Count(), testRecord.Count, "Unexpected count of received fields.");
        }

        [TestMethod]
        public void ParserCanCollectFields()
        {
            var initialLines = new[]
            {
                "foo_1",
                "xyz",
            };
            var tail = new MockedTail(initialLines);
            var testObject = new Parser(tail);
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.LoadRules(NamedGroupsRuleSet);
            testObject.Run();
            tail.AddLines("bar_1");
            CheckFields(new Dictionary<string, string>() { {"foo", "1"}, {"bar", "1"}}, testRecord );
        }

        [TestMethod]
        public void ParserCanUpdateFields()
        {
            var initialLines = new[]
            {
                "foo_1",
                "xyz",
            };
            var tail = new MockedTail(initialLines);
            var testObject = new Parser(tail);
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.LoadRules(NamedGroupsRuleSet);
            testObject.Run();
            tail.AddLines("bar_1");
            tail.AddLines("foo_2");
            CheckFields(new Dictionary<string, string>() { { "foo", "2" }, { "bar", "1" } }, testRecord);
        }

        [TestMethod]
        public void ParserCanCleanFields()
        {
            var initialLines = new[]
            {
                "foo_1",
                "bar_2",
                "xyz_3"
            };
            var tail = new MockedTail(initialLines);
            var testObject = new Parser(tail);
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.LoadRules(FieldCleanerRuleSet);
            testObject.Run();
            CheckFields(new Dictionary<string, string>() { { "foo", "1" }, { "bar", "2" }, {"xyz", "3"} }, testRecord);
            tail.AddLines("mr.proper");
            CheckFields(new Dictionary<string, string>() { { "foo", "" }, { "bar", "2" }, { "xyz", "" } }, testRecord);
        }

        private void CheckFields(IDictionary<string, string> expected, IDictionary<string, string> actual)
        {
            foreach (var field in expected)
            {
                Assert.IsTrue(actual.ContainsKey(field.Key), string.Format("Record is not complete. Missing field \"{0}\".", field.Key));
                Assert.AreEqual(field.Value, actual[field.Key], string.Format("Wrong value for field \"{0}\".", field.Key));
            }
            Assert.AreEqual(expected.Count, actual.Count, "Unexpected count of fields.");
        }
    }
}
