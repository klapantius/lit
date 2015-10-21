using System.Text;
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
        private static readonly List<IRule> SimpleRuleSet = new List<IRule>()
        {
            new Rule("foo") {Name = "firstrule"},
            new Rule("bar") {Name = "secondrule"}
        };
        private static readonly List<IRule> BlackHoleRuleSet = new List<IRule>()
        {
            new Rule(".*") {Name = "blackhole"}
        };
        private static readonly List<IRule> NamedGroupsRuleSet = new List<IRule>()
        {
            new Rule(@"foo_(?<foo>\d+)") {Name = "firstrule"},
            new Rule(@"bar_(?<bar>\d+)") {Name = "secondrule", ActionOnly = false}
        };
        private static readonly List<IRule> FieldCleanerRuleSet = new List<IRule>()
        {
            new Rule(@"foo_(?<foo>\d+)") {Name = "firstrule"},
            new Rule(@"bar_(?<bar>\d+)") {Name = "secondrule"},
            new Rule(@"xyz_(?<xyz>\d+)") {Name = "thirdrule"},
            new Rule(@"mr.proper") {Name = "cleanerrule", Clean = "foo,xyz", ActionOnly = true},
        };

        //[TestMethod]
        //public void t()
        //{
        //    var c = new Parser.ConfigurationData()
        //    {
        //        Rules = new List<Rule>()
        //        {
        //            new Rule("foo") {Name = "r1"},
        //            new Rule("bar") {Name = "r2"},
        //        }
        //    };
        //    var xd = new XDocument();
        //    using (var writer = xd.CreateWriter())
        //    {
        //        // write xml into the writer
        //        var serializer = new XmlSerializer(c.GetType());
        //        serializer.Serialize(writer, c);
        //    }
        //    Console.WriteLine(xd.ToString());
        //}

        private class MockedConfig : IConfiguration
        {
            public MockedConfig(List<IRule> rules)
            {
                Rules = rules;
            }

            public string InputFile
            {
                get { throw new NotImplementedException(); }
            }

            public TransferOptions Transfer
            {
                get { throw new NotImplementedException(); }
            }

            public List<IRule> Rules { get; private set; }
        }

        [TestMethod]
        public void ParserCanLoadRules()
        {
            var testObject = new Parser(null, new MockedConfig(SimpleRuleSet));
            Assert.IsNotNull(testObject.rules, "Rules could not be loaded.");
            Assert.AreEqual(2, testObject.rules.Count, "Missing rules.");
            Assert.AreEqual("firstrule", testObject.rules[0].Name);
            Assert.AreEqual("foo", testObject.rules[0].Pattern);

            testObject = new Parser(null, new MockedConfig(BlackHoleRuleSet));
            Assert.IsNotNull(testObject.rules, "Rules could not be loaded.");
            Assert.AreEqual(1, testObject.rules.Count, "Missing rules.");
            Assert.AreEqual("blackhole", testObject.rules[0].Name);
            Assert.AreEqual(".*", testObject.rules[0].Pattern);
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
            var testObject = new Parser(new MockedTail(initialLines), new MockedConfig(BlackHoleRuleSet));
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            var events = 0;
            testObject.Changed += (rec) =>
            {
                events += 1;
                testRecord = rec;
            };
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
            var testObject = new Parser(tail, new MockedConfig(NamedGroupsRuleSet));
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.Run();
            tail.AddLines("bar_1");
            CheckFields(new Dictionary<string, string>() { { "foo", "1" }, { "bar", "1" } }, testRecord);
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
            var testObject = new Parser(tail, new MockedConfig(NamedGroupsRuleSet));
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
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
            var testObject = new Parser(tail, new MockedConfig(FieldCleanerRuleSet));
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.Run();
            CheckFields(new Dictionary<string, string>() { { "foo", "1" }, { "bar", "2" }, { "xyz", "3" } }, testRecord);
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
