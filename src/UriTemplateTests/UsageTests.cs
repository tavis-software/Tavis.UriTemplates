using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UsageTests
    {
        [Fact]
        public void TestHexEscape()
        {
            for (int i = 20; i < 128; i++) {
                Assert.Equal(Uri.HexEscape((char)i), UriTemplate.HexEscape((char)i));    
            }
            
        }

        [Fact]
        public void ShouldAllowUriTemplateWithPathSegmentParameter()
        {
            var template = new UriTemplate("http://example.org/foo/{bar}/baz");
            template.SetParameter("bar", "yo");
            var uriString = template.Resolve();
            Assert.Equal("http://example.org/foo/yo/baz", uriString);
        }


        [Fact]
        public void ShouldAllowUriTemplateWithMultiplePathSegmentParameter()
        {
            var template = new UriTemplate("http://example.org/foo/{bar}/baz/{blar}");
            template.SetParameter("bar", "yo");
            template.SetParameter("blar", "yuck");
            var uriString = template.Resolve();
            Assert.Equal("http://example.org/foo/yo/baz/yuck", uriString);
        }

        [Fact]
        public void ShouldResolveUriTemplateWithNonStringParameter()
        {
            var template = new UriTemplate("http://example.org/foo/{bar}/baz{?lat,lng}");

            double lat = 31.464, lng = 74.386;

            template.SetParameter("bar", "yo");
            template.SetParameter("lat", lat);
            template.SetParameter("lng", lng);
            
            var uriString = template.Resolve();
            Assert.Equal("http://example.org/foo/yo/baz?lat=31.464&lng=74.386", uriString);
        }

        [Fact]
        public void ShouldAllowUriTemplateWithQueryParamsButNoValues()
        {
            var template = new UriTemplate("http://example.org/foo{?bar,baz}");
            //template.SetParameter("bar", "yo");
            //template.SetParameter("blar", "yuck");
            var uriString = template.Resolve();
            Assert.Equal("http://example.org/foo", uriString);
        }
        [Fact]
        public void ShouldAllowUriTemplateWithQueryParamsWithOneValue()
        {
            var template = new UriTemplate("http://example.org/foo{?bar,baz}");
            template.SetParameter("baz", "yo");

            var uriString = template.Resolve();
            Assert.Equal("http://example.org/foo?baz=yo", uriString);
        }


        [Fact]
        public void LabelExpansionWithDotPrefixAndEmptyKeys()
        {
            var template = new UriTemplate("X{.empty_keys}");
            template.SetParameter("empty_keys", new Dictionary<string, string>());
            var uriString = template.Resolve();
            Assert.Equal("X", uriString);
        }

        [Fact]
        public void ShouldAllowListAndSingleValueInQueryParam()
        {
            var template = new UriTemplate("http://example.org{/id*}{?fields,token}");
            template.SetParameter("id", new List<string>() { "person", "albums" });
            template.SetParameter("fields", new List<string>() { "id", "name", "picture" });
            template.SetParameter("token", "12345");
            var uriString = template.Resolve();
            Assert.Equal("http://example.org/person/albums?fields=id,name,picture&token=12345", uriString);
        }


        [Fact]
        public void ShouldThrowWhenExpressionIsNotClosed()
        {
            var result = string.Empty;
            try
            {
                var template = new UriTemplate("http://example.org/foo/{bar/baz/");
                var uriString = template.Resolve();
            }
            catch (ArgumentException ex)
            {

                result = ex.Message;
            }

            Assert.Equal("Malformed template : http://example.org/foo/{bar/baz/", result);

        }

        [Fact]
        public void ShouldThrowWhenTemplateExpressionIsEmpty()
        {
            var result = string.Empty;
            try
            {
                var template = new UriTemplate("http://example.org/foo/{}/baz/");
                var uriString = template.Resolve();
            }
            catch (ArgumentException ex)
            {

                result = ex.Message;
            }

            Assert.Equal("Malformed template : http://example.org/foo/{}/baz/", result);

        }

    }
}
