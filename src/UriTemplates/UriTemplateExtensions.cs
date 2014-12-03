using System;
using System.Collections.Generic;
using Tavis.UriTemplates;

namespace Tavis
{
    public static class UriTemplateExtensions
    {
        public static UriTemplate AddParameter(this UriTemplate template, string name, object value)
        {
            template.SetParameter(name, value);

            return template;
        }

        public static UriTemplate AddParameters(this UriTemplate template, object parametersObject)
        {

            if (parametersObject != null)
            {
                var type = parametersObject.GetType();
                foreach (var propinfo in type.GetProperties())
                {
                    template.SetParameter(propinfo.Name, propinfo.GetValue(parametersObject, null));
                }
            }

            return template;
        }
    }
}
