using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriExtensionsTests
    {
        [Fact]
        public void ShouldAllowUriTemplateWithPathSegmentParameter()
        {
            var urltemplate = new Uri("http://example.org/foo/{bar}/baz");
            var url = urltemplate.ApplyParameter("bar", "yo");
            Assert.Equal("http://example.org/foo/yo/baz", url.AbsoluteUri);
        }

        [Fact]
        public void ShouldAllowUriTemplateWithQueryParameter()
        {
            var urltemplate = new Uri("http://example.org/foo?x={bar}");
            var url = urltemplate.ApplyParameter("bar", "yo");
            Assert.Equal("http://example.org/foo?x=yo", url.AbsoluteUri);
        }
        [Fact]
        public void ShouldAllowUriTemplateWithQueryParameter2()
        {
            var urltemplate = new Uri("http://example.org/foo{?bar}");
            var url = urltemplate.ApplyParameter("bar", "yo");
            Assert.Equal("http://example.org/foo?bar=yo", url.AbsoluteUri);
        }

        [Fact]
        public void ApplyDictionaryToQueryParameters()
        {
            var urltemplate = new Uri("http://example.org/foo{?coords*}");
            var url = urltemplate.ApplyParameter("coords", new Dictionary<string,string>
            {
                {"x","1"},
                {"y","2"},
            });
            Assert.Equal("http://example.org/foo?x=1&y=2", url.AbsoluteUri);
        }

        [Fact]
        public void ApplyParametersObjectToPathSegment()
        {
            var urltemplate = new Uri("http://example.org/foo/{bar}/baz");
            var url = urltemplate.ApplyParameters(new { bar="yo"});
            Assert.Equal("http://example.org/foo/yo/baz", url.AbsoluteUri);
        }

        [Fact]
        public void ApplyParametersObjectWithAList()
        {
            var urltemplate = new Uri("http://example.org/foo/{bar}/baz{?ids,order}");
            var url = urltemplate.ApplyParameters(new { bar = "yo", order="up",ids=new List<string> {"a","b","c"} });
            Assert.Equal("http://example.org/foo/yo/baz?ids=a,b,c&order=up", url.AbsoluteUri);
        }
    }
}
