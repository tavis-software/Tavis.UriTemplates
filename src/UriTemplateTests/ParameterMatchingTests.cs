using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Extensions;

namespace UriTemplateTests
{
    public class ParameterMatchingTests
    {
        

        [Fact]
        public void MatchUriToTemplate()
        {
            var uri = new Uri("http://example.com/foo/bar");

            var sTemplate = "http://example.com/{p1}/{p2}";

            var x = UriTemplate.CreateMatchingRegex(sTemplate);

            var match = Regex.IsMatch(uri.AbsoluteUri,x);
            Assert.True(match);
        }

        [Fact]
        public void GetParameters()
        {
            var uri = new Uri("http://example.com/foo/bar");

            var sTemplate = "http://example.com/{p1}/{p2}";

            var x = UriTemplate.CreateMatchingRegex(sTemplate);
            var regex = new Regex(x);

            var match = regex.Match(uri.AbsoluteUri);

            Assert.Equal("foo",match.Groups["p1"].Value);
            Assert.Equal("bar", match.Groups["p2"].Value);
        }

        [Fact]
        public void GetParametersWithOperators()
        {
            var uri = new Uri("http://example.com/foo/bar");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(2, parameters.Count);
            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
        }

        [Fact]
        public void GetParametersFromQueryString()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(3, parameters.Count);

            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);
        }

        [Fact]
        public void GetParametersFromMultipleQueryString()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur,blob}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(3, parameters.Count);
            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);

        }
        [Fact]
        public void GetParametersFromMultipleQueryStringWithTwoParamValues()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45&blob=23");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur,blob}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(4, parameters.Count);
            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);
            Assert.Equal("23", parameters["blob"]);

        }

        [Fact]
        public void GetParametersFromMultipleQueryStringWithOptionalAndMandatoryParameters()
        {
            var uri = new Uri("http://example.com/foo/bar?blur=45&blob=23");

            var template = new UriTemplate("http://example.com/{+p1}/{p2*}{?blur}{&blob}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(4, parameters.Count);
            Assert.Equal("foo", parameters["p1"]);
            Assert.Equal("bar", parameters["p2"]);
            Assert.Equal("45", parameters["blur"]);
            Assert.Equal("23", parameters["blob"]);

        }

        [Fact]
        public void TestGlimpseUrl()
        {
            var uri = new Uri("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback");

            var template = new UriTemplate("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(3, parameters.Count);
            Assert.Equal("123232323", parameters["parentRequestId"]);
            Assert.Equal("23ADE34FAE", parameters["hash"]);
            Assert.Equal("http%3A%2F%2Fexample.com%2Fcallback", parameters["callback"]);

        }

        [Fact]
        public void TestExactParameterCount()
        {
            var uri = new Uri("http://example.com/foo?bar=10");

            var template = new UriTemplate("http://example.com/foo{?bar}");

            var parameters = template.GetParameters(uri);

            Assert.Equal(1, parameters.Count);

        }

        [Fact]
        public void SimplePerfTest()
        {
            var uri = new Uri("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId=123232323&hash=23ADE34FAE&callback=http%3A%2F%2Fexample.com%2Fcallback");

            var template = new UriTemplate("http://example.com/Glimpse.axd?n=glimpse_ajax&parentRequestId={parentRequestId}{&hash,callback}");

            for (int i = 0; i < 100000; i++)
            {
                var parameters = template.GetParameters(uri);
                
            }


        }

    }
}
