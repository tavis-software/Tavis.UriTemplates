using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis.UriTemplates;
using Xunit;
using Xunit.Extensions;


namespace UriTemplateTests
{
    
    public class BasicTests
    {

       
        /// <summary>
        /// Level 1 Tests
        /// </summary>
        /// <param name="templateValue"></param>
        /// <param name="expectedURI"></param>


        [Theory]

        // Simple string expansion 
        [InlineData("{var}", "value")]
        [InlineData("{hello}", "Hello%20World%21")]
        public void Level1Tests(string templateValue, string expectedURI)
        {
            var template = new UriTemplate(templateValue);
            SetLevel1Params(template);
            var uriString = template.Resolve();
            Assert.Equal(expectedURI, uriString);
        }

        private void SetLevel1Params(UriTemplate template)
        {
            template.SetParameter("var", "value");
            template.SetParameter("hello", "Hello World!");
        }



        /// <summary>
        /// Level 2 Tests
        /// </summary>
        /// <param name="templateValue"></param>
        /// <param name="expectedURI"></param>

        [Theory]

        // String expansion with Reserved characters as per RFC 3986
        [InlineData("{+var}", "value")]
        [InlineData("{+hello}", "Hello%20World!")]
        [InlineData("{+path}/here", "/foo/bar/here")]
        [InlineData("here?ref={+path}", "here?ref=/foo/bar")]

        // Fragment expansion, crosshatch-prefixed 
        [InlineData("X{#var}", "X#value")]
        [InlineData("X{#hello}", "X#Hello%20World!")]
        public void Level2Tests(string templateValue, string expectedURI)
        {
            var template = new UriTemplate(templateValue);
            SetLevel2Params(template);
            var uriString = template.Resolve();
            Assert.Equal(expectedURI, uriString);
        }

        private void SetLevel2Params(UriTemplate template)
        {
            template.SetParameter("var", "value");
            template.SetParameter("hello", "Hello World!");
            template.SetParameter("path", "/foo/bar");
        }


        /// <summary>
        /// Level 3 Tests
        /// </summary>
        /// <param name="templateValue"></param>
        /// <param name="expectedURI"></param>

        [Theory]
        // String expansion with multiple variables  
        [InlineData("map?{x,y}", "map?1024,768")]
        [InlineData("{x,hello,y}", "1024,Hello%20World,768")]

        // Reserved expansion with multiple variables
        [InlineData("{+x,hello,y}", "1024,Hello%20World,768")]
        [InlineData("{+path,x}/here", "/foo/bar,1024/here")]

        //  Fragment expansion with multiple variables
        [InlineData("{#x,hello,y}", "#1024,Hello%20World,768")]
        [InlineData("{#path,x}/here", "#/foo/bar,1024/here")]

        // Label expansion, dot-prefixed  
        [InlineData("X{.var}", "X.value")]
        [InlineData("X{.x,y}", "X.1024.768")]

        //  Path segments, slash-prefixed     
        [InlineData("{/var}", "/value")]
        [InlineData("{/var,x}/here", "/value/1024/here")]

        // Path-style parameters, semicolon-prefixed
        [InlineData("{;x,y}", ";x=1024;y=768")]
        [InlineData("{;x,y,empty}", ";x=1024;y=768;empty")]

        // Form-style query, ampersand-separated
        [InlineData("{?x,y}", "?x=1024&y=768")]
        [InlineData("{?x,y,empty}", "?x=1024&y=768&empty=")]

        // Form-style query continuation  
        [InlineData("?fixed=yes{&x}", "?fixed=yes&x=1024")]
        [InlineData("{&x,y,empty}", "&x=1024&y=768&empty=")]   
        public void Level3Tests(string templateValue, string expectedURI)
        {
            var template = new UriTemplate(templateValue);
            SetLevel3Params(template);
            var uriString = template.Resolve();
            Assert.Equal(expectedURI, uriString);
        }

        private void SetLevel3Params(UriTemplate template)
        {
            template.SetParameter("var", "value");
            template.SetParameter("hello", "Hello World");
            template.SetParameter("empty", "");
            template.SetParameter("path", "/foo/bar");
            template.SetParameter("x", "1024");
            template.SetParameter("y", "768");
        }

        /// <summary>
        /// Level 4 Tests
        /// </summary>
        /// <param name="templateValue"></param>
        /// <param name="expectedURI"></param>

        [Theory]
        // String expansion with value modifiers
        [InlineData("{var:3}", "val")]
        [InlineData("{var:30}", "value")]
        [InlineData("{list}", "red,green,blue")]
        [InlineData("{list*}", "red,green,blue")]
        [InlineData("{keys}", "semi,%3B,dot,.,comma,%2C")]
        [InlineData("{keys*}", "semi=%3B,dot=.,comma=%2C")]

        // Reserved expansion with value modifiers
        [InlineData("{+path:6}/here", "/foo/b/here")]
        [InlineData("{+list}", "red,green,blue")]
        [InlineData("{+list*}", "red,green,blue")]
        [InlineData("{+keys}", "semi,;,dot,.,comma,,")]
        [InlineData("{+keys*}", "semi=;,dot=.,comma=,")]

        // Fragment expansion with value modifiers
        [InlineData("{#path:6}/here", "#/foo/b/here")]
        [InlineData("{#list}", "#red,green,blue")]
        [InlineData("{#list*}", "#red,green,blue")]
        [InlineData("{#keys}", "#semi,;,dot,.,comma,,")]
        [InlineData("{#keys*}", "#semi=;,dot=.,comma=,")]

        // Label expansion, dot-prefixed
        [InlineData("X{.var:3}", "X.val")]
        [InlineData("X{.list}", "X.red,green,blue")]
        [InlineData("X{.list*}", "X.red.green.blue")]
        [InlineData("X{.keys}", "X.semi,%3B,dot,.,comma,%2C")]
        [InlineData("X{.keys*}", "X.semi=%3B.dot=..comma=%2C")]

        // Path segments, slash-prefixed 
        [InlineData("{/var:1,var}", "/v/value")]
        [InlineData("{/list}", "/red,green,blue")]
        [InlineData("{/list*}", "/red/green/blue")]
        [InlineData("{/list*,path:4}", "/red/green/blue/%2Ffoo")]
        [InlineData("{/keys}", "/semi,%3B,dot,.,comma,%2C")]
        [InlineData("{/keys*}", "/semi=%3B/dot=./comma=%2C")]

        // Path-style parameters, semicolon-prefixed
        [InlineData("{;hello:5}", ";hello=Hello")]
        [InlineData("{;list}", ";list=red,green,blue")]
        [InlineData("{;list*}", ";list=red;list=green;list=blue")]
        [InlineData("{;keys}", ";keys=semi,%3B,dot,.,comma,%2C")]
        [InlineData("{;keys*}", ";semi=%3B;dot=.;comma=%2C")]

        // Form-style query, ampersand-separated 
        [InlineData("{?var:3}", "?var=val")]
        [InlineData("{?list}", "?list=red,green,blue")]
        [InlineData("{?list*}", "?list=red&list=green&list=blue")]
        [InlineData("{?keys}", "?keys=semi,%3B,dot,.,comma,%2C")]
        [InlineData("{?keys*}", "?semi=%3B&dot=.&comma=%2C")]   

        // Form-style query continuation
        [InlineData("{&var:3}", "&var=val")]
        [InlineData("{&list}", "&list=red,green,blue")]
        [InlineData("{&list*}", "&list=red&list=green&list=blue")]
        [InlineData("{&keys}", "&keys=semi,%3B,dot,.,comma,%2C")]
        [InlineData("{&keys*}", "&semi=%3B&dot=.&comma=%2C")]

        public void Level4Tests(string templateValue, string expectedURI)
        {
            var template = new UriTemplate(templateValue);
            SetLevel4Params(template);
            var uriString = template.Resolve();
            Assert.Equal(expectedURI, uriString);
        }

        private void SetLevel4Params(UriTemplate template)
        {
            template.SetParameter("var", "value");
            template.SetParameter("hello", "Hello World");
            template.SetParameter("empty", "");
            template.SetParameter("path", "/foo/bar");
            template.SetParameter("list", new List<string> {"red","green","blue"});
            template.SetParameter("keys", new Dictionary<string, string> { { "semi", ";" }, { "dot", "." }, { "comma", "," } });
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
            template.SetParameter("bar", "yo");
            
            var uriString = template.Resolve();
            Assert.Equal("http://example.org/foo?bar=yo", uriString);
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
