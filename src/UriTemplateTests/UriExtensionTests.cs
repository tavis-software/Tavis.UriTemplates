using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriExtensionTests
    {
        [Fact]
        public void Change_an_existing_parameter_within_multiple()
        {
            var target = new Uri("http://example/customer?view=False&foo=bar");

            var parameters = target.GetQueryStringParameters();
            parameters["view"] = true;

            var template = target.MakeTemplate(parameters);

            Assert.Equal("http://example/customer?view=True&foo=bar", template.Resolve());
        }

        [Fact]
        public void Change_an_existing_parameter()
        {
            var target = new Uri("http://example/customer?view=False&foo=bar");

            var template = target.MakeTemplate();
            template.SetParameter("view", true);

            Assert.Equal("http://example/customer?view=True&foo=bar", template.Resolve());
        }

        [Fact]
        public void Remove_an_existing_parameter()
        {
            var target = new Uri("http://example/customer?view=False&foo=bar");

            var template = target.MakeTemplate();
            template.ClearParameter("view");

            Assert.Equal("http://example/customer?foo=bar", template.Resolve());
        }

        [Fact]
        public void Remove_a_query_parameters2()
        {

            var target = new Uri("http://example.org/customer?format=xml&id=23");

            var template = target.MakeTemplate();
            template.ClearParameter("format");


            Assert.Equal("http://example.org/customer?id=23", template.Resolve());
        }

        [Fact]
        public void Add_multiple_parameters_to_uri()
        {
            var target = new Uri("http://example/customer");

            var template = target.MakeTemplate(new Dictionary<string, object>
            {
                {"id", 99},
                {"view", false}
            });

            Assert.Equal("http://example/customer?id=99&view=False", template.Resolve());
        }

        [Fact]
        public void Add_parameters_to_uri_with_query_string_ignoring_path_parameter()
        {
            var target = new Uri("http://example/customer/{id}?view=true");


            var template = target.MakeTemplate(target.GetQueryStringParameters()
                .Union(new Dictionary<string, object> { { "context", "detail" } })
                .ToDictionary(k => k.Key, v => v.Value));
            template.AddParameter("id", 99);

            Assert.Equal("http://example/customer/99?view=true&context=detail", template.Resolve());
        }

        [Fact]
        public void Duplicate_query_string_key_should_parse()
        {
            var target = new Uri("http://example/customer?color=blue&color=red");
            var template = target.MakeTemplate();

            Assert.Equal("http://example/customer?color=blue,red", template.Resolve());
        }

        [Fact]
        public void Query_string_parameter_with_multiple_values_should_parse()
        {
            var target = new Uri("http://example/customer?color=blue,red");
            var template = target.MakeTemplate();

            Assert.Equal("http://example/customer?color=blue,red", template.Resolve());
        }
    }
}
