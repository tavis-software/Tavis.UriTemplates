using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Tavis.UriTemplates;
using UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriTemplateTests2
    {
        [Theory, MemberData(nameof(SpecSamples))]
        public void SpecSamplesTest(string template, string[] results, TestSet.TestCase testCase)
        {
            var uriTemplate = new UriTemplate(template);

            foreach (var variable in testCase.TestSet.Variables)
            {
                uriTemplate.SetParameter(variable.Key, variable.Value);
            }

            string result = uriTemplate.Resolve();

            Assert.Contains(result, results);
        }

        [Theory, MemberData(nameof(ExtendedSamples))]
        public void ExtendedSamplesTest(string template, string[] results, TestSet.TestCase testCase)
        {
            var uriTemplate = new UriTemplate(template);

            foreach (var variable in testCase.TestSet.Variables)
            {
                uriTemplate.SetParameter(variable.Key, variable.Value);
            }

            string result = null;
            ArgumentException aex = null;
            try
            {
                result = uriTemplate.Resolve();
            }
            catch (ArgumentException ex)
            {
                aex = ex;
            }

            if (results[0] == "False")
            {
                Assert.NotNull(aex);
            }
            else
            {
                Assert.Contains(result, results);
            }
        }

        // Disabled for the moment. [Theory, MemberData(nameof(FailureSamples))]
        //public void FailureSamplesTest(string template, string[] results, TestSet.TestCase testCase)
        //{
        //    var uriTemplate = new UriTemplate(template);

        //    foreach (var variable in testCase.TestSet.Variables)
        //    {
        //        uriTemplate.SetParameter(variable.Key, variable.Value);
        //    }

        //    ArgumentException aex = null;
        //    try
        //    {
        //        uriTemplate.Resolve();
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        aex = ex;
        //    }

        //    Assert.NotNull(aex);
        //}

        public static IEnumerable<object[]> SpecSamples
        {
            get
            {
                var suites = new List<Dictionary<string, TestSet>>();

                var stream = File.OpenRead(Path.Combine(@"..\..\..\..\..\uritemplate-test", "spec-examples.json"));
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));

                stream = File.OpenRead(Path.Combine(@"..\..\..\..\..\uritemplate-test", "spec-examples-by-section.json"));
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));

                foreach (var suite in suites)
                {
                    foreach (var testset in suite.Values)
                    {
                        foreach (var testCase in testset.TestCases)
                        {
                            yield return new object[] { testCase.Template, testCase.Result, testCase };
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> ExtendedSamples
        {
            get
            {
                var suites = new List<Dictionary<string, TestSet>>();

                var stream = File.OpenRead(Path.Combine(@"..\..\..\..\..\uritemplate-test", "extended-tests.json"));
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));

                foreach (var suite in suites)
                {
                    foreach (var testset in suite.Values)
                    {
                        foreach (var testCase in testset.TestCases)
                        {
                            yield return new object[] { testCase.Template, testCase.Result, testCase };
                        }
                    }
                }
            }
        }

        public static IEnumerable<object[]> FailureSamples
        {
            get
            {
                var suites = new List<Dictionary<string, TestSet>>();

                var stream = File.OpenRead(Path.Combine(@"..\..\..\..\..\uritemplate-test", "negative-tests.json"));
                suites.Add(CreateTestSuite(new StreamReader(stream).ReadToEnd()));

                foreach (var suite in suites)
                {
                    foreach (var testset in suite.Values)
                    {
                        foreach (var testCase in testset.TestCases)
                        {
                            yield return new object[] { testCase.Template, testCase.Result, testCase };
                        }
                    }
                }
            }
        }

        private static Dictionary<string, TestSet> CreateTestSuite(string json)
        {
            var token = JObject.Parse(json);

            var testSuite = new Dictionary<string, TestSet>();
            foreach (var jToken in token.Children())
            {
                var levelSet = (JProperty)jToken;
                testSuite.Add(levelSet.Name, CreateTestSet(levelSet.Name, levelSet.Value));
            }
            return testSuite;
        }

        private static TestSet CreateTestSet(string name, JToken token)
        {
            var testSet = new TestSet { Name = name };

            var variables = token["variables"];

            foreach (var jToken in variables)
            {
                var variable = (JProperty)jToken;
                ParseVariable(variable, testSet.Variables);
            }

            var testcases = token["testcases"];

            foreach (var testcase in testcases)
            {
                testSet.TestCases.Add(CreateTestCase(testSet, testcase));
            }

            return testSet;
        }

        private static void ParseVariable(JProperty variable, IDictionary<string, object> dictionary)
        {
            using (new WithCultureInfo(CultureInfo.InvariantCulture))
            {
                if (variable.Value.Type == JTokenType.Array)
                {
                    var array = (JArray) variable.Value;
                    dictionary.Add(variable.Name, array.Count == 0
                        ? new List<string>()
                        : array.Values<string>());
                }
                else if (variable.Value.Type == JTokenType.Object)
                {
                    var jvalue = (JObject) variable.Value;
                    var dict = new Dictionary<string, string>();
                    foreach (var prop in jvalue.Properties())
                    {
                        dict[prop.Name] = prop.Value.ToString();
                    }

                    dictionary.Add(variable.Name, dict);
                }
                else
                {
                    dictionary.Add(variable.Name, ((JValue) variable.Value).Value == null
                        ? null
                        : variable.Value.ToString());
                }
            }
        }

        private static TestSet.TestCase CreateTestCase(TestSet testSet, JToken testcase)
        {
            var testCase = new TestSet.TestCase(testSet) { Template = testcase[0].Value<string>() };

            if (testcase[1].Type == JTokenType.Array)
            {
                var results = (JArray)testcase[1];
                testCase.Result = results.Select(jv => jv.Value<string>()).ToArray();
            }
            else
            {
                testCase.Result = new string[1];
                testCase.Result[0] = testcase[1].Value<string>();
            }
            return testCase;
        }

        public class TestSet
        {
            public string Name { get; set; }
            public Dictionary<string, object> Variables = new Dictionary<string, object>();
            public List<TestCase> TestCases = new List<TestCase>();

            public class TestCase
            {
                public TestCase(TestSet testSet)
                {
                    TestSet = testSet;
                }

                public TestSet TestSet { get; }
                public string Template { get; set; }
                public string[] Result { get; set; }
            }
        }
    }
}
