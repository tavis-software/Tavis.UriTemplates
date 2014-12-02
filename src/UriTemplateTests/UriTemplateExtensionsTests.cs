using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriTemplateExtensionsTests
    {
        [Fact]
        public void UpdatePathParameter()
        {
            var url = new UriTemplate("http://example.org/{tenant}/customers")
                .AddParameter("tenant", "acmé")
                .Resolve();

            Assert.Equal("http://example.org/acm%C3%A9/customers", url);
        }


        [Fact]
        public void QueryParametersTheOldWay()
        {
            var url = new UriTemplate("http://example.org/customers?active={activeflag}")
                .AddParameter("activeflag", "true")
                .Resolve();

            Assert.Equal("http://example.org/customers?active=true",url); 
        }

        [Fact]
        public void QueryParametersTheNewWay()
        {
            var url = new UriTemplate("http://example.org/customers{?active}")
                .AddParameter("active", "true")
                .Resolve();

            Assert.Equal("http://example.org/customers?active=true", url);
        }

        [Fact]
        public void QueryParametersTheNewWayWithoutValue()
        {

            var url = new UriTemplate("http://example.org/customers{?active}")
                .AddParameters(null)
                .Resolve();

            Assert.Equal("http://example.org/customers", url);
        }

        [Fact]
        public void ParametersFromAnObject()
        {
            var url = new UriTemplate("http://example.org/{environment}/{version}/customers{?active,country}")
                .AddParameters(new
                {
                    environment = "dev",
                    version = "v2",
                    active = "true",
                    country = "CA"
                })
                .Resolve();

            Assert.Equal("http://example.org/dev/v2/customers?active=true&country=CA", url);
        }

        [Fact]
        public void ApplyDictionaryToQueryParameters()
        {
            var url = new UriTemplate("http://example.org/foo{?coords*}")
                .AddParameter("coords", new Dictionary<string, string>
                {
                    {"x", "1"},
                    {"y", "2"},
                })
                .Resolve();

            Assert.Equal("http://example.org/foo?x=1&y=2", url);
        }

        [Fact]
        public void ApplyParametersObjectToPathSegment()
        {
            var url = new UriTemplate("http://example.org/foo/{bar}/baz")
                .AddParameters(new {bar = "yo"})
                .Resolve();

            Assert.Equal("http://example.org/foo/yo/baz", url);
        }

        [Fact]
        public void ApplyParametersObjectWithAList()
        {
            var url = new UriTemplate("http://example.org/customers{?ids,order}")
                .AddParameters(new
                {
                    order = "up", 
                    ids = new List<string> {"21", "75", "21"}
                }).Resolve();

            Assert.Equal("http://example.org/customers?ids=21,75,21&order=up", url);
        }

        [Fact]
        public void ApplyParametersObjectWithAListofInts()
        {
            var url = new UriTemplate("http://example.org/customers{?ids,order}")
                .AddParameters(new
                {
                    order = "up",
                    ids = new[] {21, 75, 21}
                })
                .Resolve();

            Assert.Equal("http://example.org/customers?ids=21,75,21&order=up", url);
        }

        [Fact]
        public void ApplyFoldersToPath()
        {
            var url = new UriTemplate("http://example.org/files{/folders*}{?filename}")
                .AddParameters(new
                {
                    folders = new[] {"customer", "project"},
                    filename = "proposal.pdf"
                })
                .Resolve();

            Assert.Equal("http://example.org/files/customer/project?filename=proposal.pdf", url);
        }

        [Fact]
        public void ParametersFromAnObjectFromInvalidUrl()
        {

            var url = new UriTemplate("http://{environment}.example.org/{version}/customers{?active,country}")
            .AddParameters(new
            {
                environment = "dev",
                version = "v2",
                active = "true",
                country = "CA"
            })
            .Resolve();

            Assert.Equal("http://dev.example.org/v2/customers?active=true&country=CA", url);
        }


        [Fact]
        public void ApplyFoldersToPathFromStringNotUrl()
        {

            var url = new UriTemplate("http://example.org{/folders*}{?filename}")
                .AddParameters(new
                {
                    folders = new[] { "files", "customer", "project" },
                    filename = "proposal.pdf"
                })
                .Resolve();

            Assert.Equal("http://example.org/files/customer/project?filename=proposal.pdf", url);
        }


        [Fact]
        public void ReplaceBaseAddress()
        {

            var url = new UriTemplate("{+baseUrl}api/customer/{id}")
                .AddParameters(new
                {
                    baseUrl = "http://example.org/",
                    id = "22"
                })
                .Resolve();

            Assert.Equal("http://example.org/api/customer/22", url);
        }

        [Fact]
        public void ReplaceBaseAddressButNotId()
        {

            var url = new UriTemplate("{+baseUrl}api/customer/{id}",resolvePartially:true)
                .AddParameters(new
                {
                    baseUrl = "http://example.org/"
                })
                .Resolve();

            Assert.Equal("http://example.org/api/customer/{id}", url);
        }

        //[Fact]
        public void PartiallyParametersFromAnObjectFromInvalidUrl()
        {

            var url = new UriTemplate("http://{environment}.example.org/{version}/customers{?active,country}",resolvePartially:true)
            .AddParameters(new
            {
                environment = "dev",
                version = "v2"
            })
            .Resolve();

            Assert.Equal("http://dev.example.org/v2/customers{?active,country}", url);
        }
    }
}
