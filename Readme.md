# Uri Templates #

.Net implementation of the [URI Template Spec RFC6570](http://tools.ietf.org/html/rfc6570).

Library implements Level 4 compliance and is tested against test cases from [UriTemplate test suite](https://github.com/uri-templates/uritemplate-test).


Here are some basic usage examples:

Replacing a path segment parameter,

      var template = new UriTemplate("http://example.org/foo/{bar}/baz");
      template.SetParameter("bar", "yo");
      var uriString = template.Resolve();
      // uriString == "http://example.org/foo/yo/baz"

Setting query string parameters,

      var template = new UriTemplate("http://example.org/location{?lat,lng}");

      double lat = 31.464, lng = 74.386;

      template.SetParameter("bar", "yo");
      template.SetParameter("lat", lat);
      var uriString = template.Resolve();
      // uriString == "http://example.org/location?lat=31.464&lng=74.386"


Resolving a URI when parameters are not set will simply remove the parameters,

      var template = new UriTemplate("http://example.org/foo{?bar,baz}");
      var uriString = template.Resolve();
      // uriString == "http://example.org/foo"

You can even pass lists as parameters

      var template = new UriTemplate("http://example.org{/id*}{?fields,token}");
      template.SetParameter("id", new List<string>() { "person", "albums" });
      template.SetParameter("fields", new List<string>() { "id", "name", "picture" });
      template.SetParameter("token", "12345");
      var uriString = template.Resolve();
      // uriString == "http://example.org/person/albums?fields=id,name,picture&token=12345"

And dictionaries,

      var template = new UriTemplate("http://example.org/customers{?query*}");
      template.SetParameter("query", new Dictionary<string, string>()
      {
          {"active","true"},
          {"Country","Brazil"}
      });
      var uriString = template.Resolve();
      // uriString == "http://example.org/customers?active=true&Country=Brazil"


We also handle all the complex URI encoding rules automatically.

      var template = new UriTemplate("http://example.org/sparql{?query}");
      template.SetParameter("query", "PREFIX dc: <http://purl.org/dc/elements/1.1/> SELECT ?book ?who WHERE { ?book dc:creator ?who }");
      var uriString = template.Resolve();
      // uriString ==             "http://example.org/sparql?query=PREFIX%20dc%3A%20%3Chttp%3A%2F%2Fpurl.org%2Fdc%2Felements%2F1.1%2F%3E%20SELECT%20%3Fbook%20%3Fwho%20WHERE%20%7B%20%3Fbook%20dc%3Acreator%20%3Fwho%20%7D"



Current this library does not pass all of the failure tests.  I.e. If you pass an invalid URI Template, you may not get an exception.

