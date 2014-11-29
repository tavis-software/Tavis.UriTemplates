using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavis.UriTemplates;

namespace Tavis
{
    public static class UriExtensions
    {
        public static Uri ApplyParameter(this Uri url, string name, object value)
        {
            var template = new UriTemplate(url.OriginalString);

            if (value is Dictionary<string, string>)
            {
                template.SetParameter(name, value as Dictionary<string,string>);
            }
            else if (value is List<string>)
            {
                template.SetParameter(name, value as List<string>);
            }
            else
            {
                template.SetParameter(name, value);    
            }
            
            
            return new Uri(template.Resolve(),UriKind.RelativeOrAbsolute);
        }

        public static Uri ApplyParameters(this Uri url, object parametersObject)
        {
            var template = new UriTemplate(url.OriginalString);

            var type = parametersObject.GetType();
            foreach (var propinfo in type.GetProperties())
            {
                template.SetParameter(propinfo.Name, propinfo.GetValue(parametersObject,null));                
            }

            return new Uri(template.Resolve(), UriKind.RelativeOrAbsolute);
        }
    }
}
