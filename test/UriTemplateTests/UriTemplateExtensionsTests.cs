using System.Collections.Generic;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriTemplateExtensionsTests
    {
        [Fact]
        public void UpdatePathParameter()
        {
            string url = new UriTemplate("http://example.org/{tenant}/customers")
                .AddParameter("tenant", "acmé")
                .Resolve();

            Assert.Equal("http://example.org/acm%C3%A9/customers", url);
        }

        [Fact]
        public void QueryParametersTheOldWay()
        {
            string url = new UriTemplate("http://example.org/customers?active={activeflag}")
                .AddParameter("activeflag", "true")
                .Resolve();

            Assert.Equal("http://example.org/customers?active=true",url); 
        }

        [Fact]
        public void QueryParametersTheNewWay()
        {
            string url = new UriTemplate("http://example.org/customers{?active}")
                .AddParameter("active", "true")
                .Resolve();

            Assert.Equal("http://example.org/customers?active=true", url);
        }

        [Fact]
        public void QueryParametersTheNewWayWithoutValue()
        {
            string url = new UriTemplate("http://example.org/customers{?active}")
                .AddParameters(null)
                .Resolve();

            Assert.Equal("http://example.org/customers", url);
        }

        [Fact]
        public void ShouldResolveUriTemplateWithNonStringParameter()
        {
            string url = new UriTemplate("http://example.org/location{?lat,lng}")
                .AddParameters(new { lat = 31.464, lng = 74.386 })
                .Resolve();

            Assert.Equal("http://example.org/location?lat=31.464&lng=74.386", url);
        }

        [Fact]
        public void ParametersFromAnObject()
        {
            string url = new UriTemplate("http://example.org/{environment}/{version}/customers{?active,country}")
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
        public void SomeParametersFromAnObject()
        {
            string url = new UriTemplate("http://example.org{/environment}{/version}/customers{?active,country}")
                .AddParameters(new
                {
                    version = "v2",
                    active = "true"
                })
                .Resolve();

            Assert.Equal("http://example.org/v2/customers?active=true", url);
        }

        [Fact]
        public void ApplyDictionaryToQueryParameters()
        {
            string url = new UriTemplate("http://example.org/foo{?coords*}")
                .AddParameter("coords", new Dictionary<string, string>
                {
                    {"x", "1"},
                    {"y", "2"}
                })
                .Resolve();

            Assert.Equal("http://example.org/foo?x=1&y=2", url);
        }

        [Fact]
        public void ApplyParametersObjectToPathSegment()
        {
            string url = new UriTemplate("http://example.org/foo/{bar}/baz")
                .AddParameters(new {bar = "yo"})
                .Resolve();

            Assert.Equal("http://example.org/foo/yo/baz", url);
        }

        [Fact]
        public void ExtremeEncoding()
        {
            string url = new UriTemplate("http://example.org/sparql{?query}")
                    .AddParameter("query", "PREFIX dc: <http://purl.org/dc/elements/1.1/> SELECT ?book ?who WHERE { ?book dc:creator ?who }")
                    .Resolve();

            Assert.Equal("http://example.org/sparql?query=PREFIX%20dc%3A%20%3Chttp%3A%2F%2Fpurl.org%2Fdc%2Felements%2F1.1%2F%3E%20SELECT%20%3Fbook%20%3Fwho%20WHERE%20%7B%20%3Fbook%20dc%3Acreator%20%3Fwho%20%7D", url);
        }

        [Fact]
        public void ApplyParametersObjectWithAList()
        {
            string url = new UriTemplate("http://example.org/customers{?ids,order}")
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
            string url = new UriTemplate("http://example.org/customers{?ids,order}")
                .AddParameters(new
                {
                    order = "up",
                    ids = new[] {21, 75, 21}
                })
                .Resolve();

            Assert.Equal("http://example.org/customers?ids=21,75,21&order=up", url);
        }

        [Fact]
        public void ApplyParametersObjectWithAListofIntsExploded()
        {
            string url = new UriTemplate("http://example.org/customers{?ids*,order}")
                .AddParameters(new
                {
                    order = "up",
                    ids = new[] { 21, 75, 21 }
                })
                .Resolve();

            Assert.Equal("http://example.org/customers?ids=21&ids=75&ids=21&order=up", url);
        }

        [Fact]
        public void ApplyFoldersToPath()
        {
            string url = new UriTemplate("http://example.org/files{/folders*}{?filename}")
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
            string url = new UriTemplate("http://{environment}.example.org/{version}/customers{?active,country}")
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
            string url = new UriTemplate("http://example.org{/folders*}{?filename}")
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
            string url = new UriTemplate("{+baseUrl}api/customer/{id}")
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
            string url = new UriTemplate("{+baseUrl}api/customer/{id}",resolvePartially:true)
                .AddParameters(new
                {
                    baseUrl = "http://example.org/"
                })
                .Resolve();

            Assert.Equal("http://example.org/api/customer/{id}", url);
        }

        [Fact]
        public void PartiallyParametersFromAnObjectFromInvalidUrl()
        {
            string url = new UriTemplate("http://{environment}.example.org/{version}/customers{?active,country}",resolvePartially:true)
            .AddParameters(new
            {
                environment = "dev",
                version = "v2"
            })
            .Resolve();

            Assert.Equal("http://dev.example.org/v2/customers{?active,country}", url);
        }

        [Fact]
        public void PartiallyApplyFoldersToPathFromStringNotUrl()
        {
            string url = new UriTemplate("http://example.org{/folders*}{?filename}",true)
                .AddParameters(new
                {
                    filename = "proposal.pdf"
                })
                .Resolve();

            Assert.Equal("http://example.org{/folders*}?filename=proposal.pdf", url);
        }

        [Fact]
        public void UseArbitraryClassAsParameter()
        {
            string url = new UriTemplate("/{test}", true)
                .AddParameters(new
                {
                    test = new Something()
                })
                .Resolve();

            Assert.Equal("/something", url);
        }

        [Fact]
        public void AddMultipleParametersToLink()
        {
            var template = new UriTemplate("http://localhost/api/{dataset}/customer{?foo,bar,baz}");
            template.AddParameters(new Dictionary<string, object>
            {
                {"foo", "bar"},
                {"baz", "99"},
                {"dataset", "bob"}
            });

            string uri = template.Resolve();

            Assert.Equal("http://localhost/api/bob/customer?foo=bar&baz=99", uri);
        }
    }

    internal class Something
    {
        public override string ToString()
        {
            return "something";
        }
    }
}
