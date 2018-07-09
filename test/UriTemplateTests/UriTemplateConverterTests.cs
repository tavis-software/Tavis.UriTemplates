using System.ComponentModel;
using Tavis.UriTemplates;
using Xunit;

namespace UriTemplateTests
{
    public class UriTemplateConverterTests
    {
        [Theory]
        [InlineData("http://example.org/{tenant}/customers")]
        [InlineData("http://example.org/{environment}/{version}/customers{?active,country}")]
        [InlineData("http://example.org/foo{?coords*}")]
        public void ConvertFromString(string rawTemplate)
        {
            var converter = TypeDescriptor.GetConverter(typeof(UriTemplate));
            var template = converter.ConvertFromString(rawTemplate);

            Assert.NotNull(template);
            Assert.Equal(rawTemplate, template.ToString());
        }
    }
}
