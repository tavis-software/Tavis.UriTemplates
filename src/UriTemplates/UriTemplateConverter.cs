using System;
using System.ComponentModel;
using System.Globalization;

namespace Tavis.UriTemplates
{
    /// <summary>
    /// Converts to <see cref="UriTemplate"/> instances from other representations.
    /// </summary>
    public sealed class UriTemplateConverter 
        : TypeConverter
    {
        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(
            ITypeDescriptorContext context,
            CultureInfo culture,
            object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is string template)
            {
                return template.Length == 0
                    ? null
                    : new UriTemplate(template);
            }

            throw (NotSupportedException)GetConvertFromException(value);
        }
    }
}
