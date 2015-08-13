# Uri Templates #

.NET implementation of the [URI Template Spec RFC6570](http://tools.ietf.org/html/rfc6570). ![Build Status](https://ci.appveyor.com/api/projects/status/nol9sb59uvxvgt8l?svg=true)

Library implements Level 4 compliance and is tested against test cases from [UriTemplate test suite](https://github.com/uri-templates/uritemplate-test).


Here are some basic usage examples:

Replacing a path segment parameter,

```csharp
[Fact]
public void UpdatePathParameter()
{
    var url = new UriTemplate("http://example.org/{tenant}/customers")
        .AddParameter("tenant", "acmé")
        .Resolve();

    Assert.Equal("http://example.org/acm%C3%A9/customers", url);
}
```

Setting query string parameters,

```csharp
[Fact]
public void ShouldResolveUriTemplateWithNonStringParameter()
{
    var url = new UriTemplate("http://example.org/location{?lat,lng}")
        .AddParameters(new { lat = 31.464, lng = 74.386 })
        .Resolve();

    Assert.Equal("http://example.org/location?lat=31.464&lng=74.386", url);
}
```


Resolving a URI when parameters are not set will simply remove the parameters,

```csharp
[Fact]
public void SomeParametersFromAnObject()
{
    var url = new UriTemplate("http://example.org{/environment}{/version}/customers{?active,country}")
        .AddParameters(new
        {
            version = "v2",
            active = "true"
        })
        .Resolve();

    Assert.Equal("http://example.org/v2/customers?active=true", url);
}
```

You can even pass lists as parameters

```csharp
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
```

And dictionaries,

```csharp
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
```

We also handle all the complex URI encoding rules automatically.

```csharp
[Fact]
public void TestExtremeEncoding()
{
    var url = new UriTemplate("http://example.org/sparql{?query}")
            .AddParameter("query", "PREFIX dc: <http://purl.org/dc/elements/1.1/> SELECT ?book ?who WHERE { ?book dc:creator ?who }")
            .Resolve();
    Assert.Equal("http://example.org/sparql?query=PREFIX%20dc%3A%20%3Chttp%3A%2F%2Fpurl.org%2Fdc%2Felements%2F1.1%2F%3E%20SELECT%20%3Fbook%20%3Fwho%20WHERE%20%7B%20%3Fbook%20dc%3Acreator%20%3Fwho%20%7D", url);
}
```

There is a [blogpost](http://bizcoder.com/constructing-urls-the-easy-way) that discusses these examples and more in detail.

As well as having a set of regular usage tests, this library also executes tests based on a standard test suite.  This test suite is pulled in as a Git Submodule, therefore when cloning this repo, you will need use the `--recursive` switch,

        git clone --recursive git@github.com:tavis-software/Tavis.UriTemplates.git


Current this library does not pass all of the failure tests.  I.e. If you pass an invalid URI Template, you may not get an exception.
