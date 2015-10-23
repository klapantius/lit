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
            var testObject = new Parser(null, new MockedConfig(new List<IRule>()
            {
                new Rule("foo") {Name = "firstrule"},
                new Rule("bar") {Name = "secondrule"}
            }));
            CheckRules(testObject.rules);
            Assert.IsNotNull(testObject.rules, "Rules could not be loaded.");
            Assert.AreEqual(2, testObject.rules.Count, "Missing rules.");
            Assert.AreEqual("firstrule", testObject.rules[0].Name);
            Assert.AreEqual("foo", testObject.rules[0].Pattern);

            testObject = new Parser(null, new MockedConfig(new List<IRule>() {new Rule(".*") {Name = "blackhole"}}));
            CheckRules(testObject.rules);
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
            var testObject = new Parser(new MockedTail(initialLines), new MockedConfig(new List<IRule>() {new Rule(".*") {Name = "blackhole"}}));
            CheckRules(testObject.rules);
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
            var tail = new MockedTail(new[]
            {
                "foo_1",
                "xyz",
            });
            var testObject = new Parser(tail, new MockedConfig(new List<IRule>()
            {
                new Rule(@"foo_(?<foo>\d+)") {Name = "firstrule"},
                new Rule(@"bar_(?<bar>\d+)") {Name = "secondrule", ActionOnly = false}
            }));
            CheckRules(testObject.rules);
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.Run();
            tail.AddLines("bar_1");
            CheckFields(new Dictionary<string, string>() { { "foo", "1" }, { "bar", "1" } }, testRecord);
        }

        [TestMethod]
        public void ParserCanUpdateFields()
        {
            var tail = new MockedTail(new[]
            {
                "foo_1",
                "xyz",
            });
            var testObject = new Parser(tail, new MockedConfig(new List<IRule>()
            {
                new Rule(@"foo_(?<foo>\d+)") {Name = "firstrule"},
                new Rule(@"bar_(?<bar>\d+)") {Name = "secondrule", ActionOnly = false}
            }));
            CheckRules(testObject.rules);
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
            var tail = new MockedTail(new[]
            {
                "foo_1",
                "bar_2",
                "xyz_3"
            });
            var testObject = new Parser(tail, new MockedConfig(new List<IRule>()
            {
                new Rule(@"foo_(?<foo>\d+)") {Name = "firstrule"},
                new Rule(@"bar_(?<bar>\d+)") {Name = "secondrule"},
                new Rule(@"xyz_(?<xyz>\d+)") {Name = "thirdrule"},
                new Rule(@"mr.proper") {Name = "cleanerrule", Clean = "foo,xyz", ActionOnly = true},
            }));
            CheckRules(testObject.rules);
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.Run();
            CheckFields(new Dictionary<string, string>() { { "foo", "1" }, { "bar", "2" }, { "xyz", "3" } }, testRecord);
            tail.AddLines("mr.proper");
            CheckFields(new Dictionary<string, string>() { { "foo", "" }, { "bar", "2" }, { "xyz", "" } }, testRecord);
        }

        [TestMethod]
        public void ParserTakesTheLastMatchingRule()
        {
            var testObject = new Parser(
                new MockedTail(new[]
                {
                    "foo_1",
                    "bar_2",
                    "xyz_3"
                }),
                new MockedConfig(new List<IRule>()
                {
                    new Rule(@"foo_(?<foo>\d+)") {Name = "foo"},
                    new Rule(@".*_(?<firstMatching>3)") {Name = "xyz1"},
                    new Rule(@"xyz_(?<secondMatching>\d+)") {Name = "xyz2"},
                    new Rule(@"bar_(?<bar>\d+)") {Name = "bar"},
                }));
            CheckRules(testObject.rules);
            IDictionary<string, string> testRecord = new ConcurrentDictionary<string, string>();
            testObject.Changed += (rec) => { testRecord = rec; };
            testObject.Run();
            CheckFields(new Dictionary<string, string>() { { "foo", "1" }, { "bar", "2" }, { "firstMatching", null }, { "secondMatching", "3" } }, testRecord);
        }

        private void CheckRules(List<IRule> rules)
        {
            var errors = rules.Where(r => !r.IsValid).ToList();
            Assert.AreEqual(0, errors.Count, string.Format("Rule error(s) found:{0}{1}", Environment.NewLine,
                string.Join(Environment.NewLine, errors.Select(r => string.Format("\"{0}\": {1}", r.Name, r.ErrorMessage)))));
        }

        private void CheckFields(IDictionary<string, string> expected, IDictionary<string, string> actual)
        {
            foreach (var field in expected)
            {
                if (field.Value != null)
                {
                    Assert.IsTrue(actual.ContainsKey(field.Key), string.Format("Record is not complete. Missing field \"{0}\".", field.Key));
                    Assert.AreEqual(field.Value, actual[field.Key], string.Format("Wrong value for field \"{0}\".", field.Key));
                }
                else
                {
                    Assert.IsFalse(actual.ContainsKey(field.Key), string.Format("Unexpected field \"{0}\" (value: \"{1}\").", field.Key, field.Value));
                }
            }
            Assert.AreEqual(expected.Count(f => f.Value != null), actual.Count, "Unexpected count of fields.");
        }
    }
}
