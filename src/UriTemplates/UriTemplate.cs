using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UriTemplates;

namespace Tavis.UriTemplates
{
#if TYPE_CONVERTER
    using System.ComponentModel;

    [TypeConverter(typeof(UriTemplateConverter))]
#endif
    public class UriTemplate
    {
        private static readonly Dictionary<char, OperatorInfo> _Operators = new Dictionary<char, OperatorInfo>
        {
            { '\0', new OperatorInfo { Default = true, First = "", Seperator = ',', Named = false, IfEmpty = "", AllowReserved = false } },
            { '+', new OperatorInfo { Default = false, First = "", Seperator = ',', Named = false, IfEmpty = "", AllowReserved = true } },
            { '.', new OperatorInfo { Default = false, First = ".", Seperator = '.', Named = false, IfEmpty = "", AllowReserved = false } },
            { '/', new OperatorInfo { Default = false, First = "/", Seperator = '/', Named = false, IfEmpty = "", AllowReserved = false } },
            { ';', new OperatorInfo { Default = false, First = ";", Seperator = ';', Named = true, IfEmpty = "", AllowReserved = false } },
            { '?', new OperatorInfo { Default = false, First = "?", Seperator = '&', Named = true, IfEmpty = "=", AllowReserved = false } },
            { '&', new OperatorInfo { Default = false, First = "&", Seperator = '&', Named = true, IfEmpty = "=", AllowReserved = false } },
            { '#', new OperatorInfo { Default = false, First = "#", Seperator = ',', Named = false, IfEmpty = "", AllowReserved = true } }
        };

        private readonly string _template;
        private readonly Dictionary<string, object> _Parameters;

        private enum States { CopyingLiterals, ParsingExpression }

        private readonly bool _resolvePartially;

        public UriTemplate(string template, bool resolvePartially = false, bool caseInsensitiveParameterNames = false)
        {
            _resolvePartially = resolvePartially;
            _template = template;
            _Parameters = caseInsensitiveParameterNames
                ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return _template;
        }

        public void SetParameter(string name, object value)
        {
            _Parameters[name] = value;
        }

        public void ClearParameter(string name)
        {
            _Parameters.Remove(name);
        }

        public void SetParameter(string name, string value)
        {
            _Parameters[name] = value;
        }

        public void SetParameter(string name, IEnumerable<string> value)
        {
            _Parameters[name] = value;
        }

        public void SetParameter(string name, IDictionary<string, string> value)
        {
            _Parameters[name] = value;
        }

        public IEnumerable<string> GetParameterNames()
        {
            var result = ResolveResult();
            return result.ParameterNames;
        }

        public string Resolve()
        {
            var result = ResolveResult();
            return result.ToString();
        }

        private Result ResolveResult()
        {
            var currentState = States.CopyingLiterals;
            var result = new Result();
            StringBuilder currentExpression = null;
            foreach (char character in _template)
            {
                switch (currentState)
                {
                    case States.CopyingLiterals:
                        switch (character)
                        {
                            case '{':
                                currentState = States.ParsingExpression;
                                currentExpression = new StringBuilder();
                                break;

                            case '}':
                                throw new ArgumentException("Malformed template, unexpected } : " + result);

                            default:
                                result.Append(character);
                                break;
                        }
                        break;

                    case States.ParsingExpression:
                        if (character == '}')
                        {
                            ProcessExpression(currentExpression, result);

                            currentState = States.CopyingLiterals;
                        }
                        else
                        {
                            currentExpression?.Append(character);
                        }

                        break;
                }
            }
            if (currentState == States.ParsingExpression)
            {
                result.Append("{");
                result.Append(currentExpression?.ToString());

                throw new ArgumentException("Malformed template, missing } : " + result);
            }

            if (result.ErrorDetected)
            {
                throw new ArgumentException("Malformed template : " + result);
            }
            return result;
        }

        private void ProcessExpression(StringBuilder currentExpression, Result result)
        {
            if (currentExpression.Length == 0)
            {
                result.ErrorDetected = true;
                result.Append("{}");
                return;
            }

            var opLocal = GetOperator(currentExpression[0]);

            int firstChar = opLocal.Default ? 0 : 1;
            bool multivariableExpression = false;

            var varSpec = new VarSpec(opLocal);
            for (int i = firstChar; i < currentExpression.Length; i++)
            {
                char currentChar = currentExpression[i];
                switch (currentChar)
                {
                    case '*':
                        varSpec.Explode = true;
                        break;

                    case ':':  // Parse Prefix Modifier
                        var prefixText = new StringBuilder();
                        currentChar = currentExpression[++i];
                        while (currentChar >= '0' && currentChar <= '9' && i < currentExpression.Length)
                        {
                            prefixText.Append(currentChar);
                            i++;
                            if (i < currentExpression.Length)
                            {
                                currentChar = currentExpression[i];
                            }
                        }
                        varSpec.PrefixLength = int.Parse(prefixText.ToString());
                        i--;
                        break;

                    case ',':
                        multivariableExpression = true;
                        bool success = ProcessVariable(varSpec, result, true);
                        bool isFirst = varSpec.First;

                        // Reset for new variable
                        varSpec = new VarSpec(opLocal);
                        if (success || !isFirst || _resolvePartially)
                        {
                            varSpec.First = false;
                        }

                        if (!success && _resolvePartially) {result.Append(",") ; }
                        break; 
                    

                    default:
                        if (IsVarNameChar(currentChar))
                        {
                            varSpec.VarName.Append(currentChar);
                        }
                        else
                        {
                            result.ErrorDetected = true;
                        }
                        break;
                }
            }

            ProcessVariable(varSpec, result, multivariableExpression);
            if (multivariableExpression && _resolvePartially)
            {
                result.Append("}");
            }
        }

        private bool ProcessVariable(VarSpec varSpec, Result result, bool multiVariableExpression = false)
        {
            string varnameLocal = varSpec.VarName.ToString();
            result.ParameterNames.Add(varnameLocal);

            if (!_Parameters.ContainsKey(varnameLocal)
                || _Parameters[varnameLocal] == null
                || _Parameters[varnameLocal] is IList && ((IList) _Parameters[varnameLocal]).Count == 0
                || _Parameters[varnameLocal] is IDictionary && ((IDictionary) _Parameters[varnameLocal]).Count == 0)
            {
                if (_resolvePartially)
                {
                    if (multiVariableExpression)
                    {
                        if (varSpec.First)
                        {
                            result.Append("{");
                        }

                        result.Append(varSpec.ToString());
                    }
                    else
                    {
                        result.Append("{");
                        result.Append(varSpec.ToString());
                        result.Append("}");
                    }
                    return false;
                }
                return false;
            }

            if (varSpec.First)
            {
                result.Append(varSpec.OperatorInfo.First);
            }
            else
            {
                result.Append(varSpec.OperatorInfo.Seperator);
            }

            var value = _Parameters[varnameLocal];

            // Handle Strings
            if (value is string stringValue1)
            {
                if (varSpec.OperatorInfo.Named)
                {
                    result.AppendName(varnameLocal, varSpec.OperatorInfo, string.IsNullOrEmpty(stringValue1));
                }
                result.AppendValue(stringValue1, varSpec.PrefixLength, varSpec.OperatorInfo.AllowReserved);
            }
            else
            {
                // Handle Lists
                var list = value as IList;
                if (list == null && value is IEnumerable<string> strings)
                {
                    list = strings.ToList();
                }
                if (list != null)
                {
                    if (varSpec.OperatorInfo.Named && !varSpec.Explode)  // exploding will prefix with list name
                    {
                        result.AppendName(varnameLocal, varSpec.OperatorInfo, list.Count == 0);
                    }

                    result.AppendList(varSpec.OperatorInfo, varSpec.Explode, varnameLocal, list);
                }
                else
                {
                    // Handle associative arrays
                    if (value is IDictionary<string, string> dictionary)
                    {
                        if (varSpec.OperatorInfo.Named && !varSpec.Explode)  // exploding will prefix with list name
                        {
                            result.AppendName(varnameLocal, varSpec.OperatorInfo, !dictionary.Any());
                        }
                        result.AppendDictionary(varSpec.OperatorInfo, varSpec.Explode, dictionary);
                    }
                    else
                    {
                        // If above all fails, convert the object to string using the default object.ToString() implementation
                        using (new WithCultureInfo(CultureInfo.InvariantCulture))
                        {
                            string stringValue = value.ToString();
                            if (varSpec.OperatorInfo.Named)
                            {
                                result.AppendName(varnameLocal, varSpec.OperatorInfo, string.IsNullOrEmpty(stringValue));
                            }
                            result.AppendValue(stringValue, varSpec.PrefixLength, varSpec.OperatorInfo.AllowReserved);
                        }
                    }
                }
            }
            return true;
        }

        private static bool IsVarNameChar(char c)
        {
            return c >= 'A' && c <= 'z' //Alpha
                || c >= '0' && c <= '9' // Digit
                || c == '_'
                || c == '%'
                || c == '.';
        }

        private static OperatorInfo GetOperator(char operatorIndicator)
        {
            OperatorInfo opLocal;
            switch (operatorIndicator)
            {

                case '+':
                case ';':
                case '/':
                case '#':
                case '&':
                case '?':
                case '.':
                    opLocal = _Operators[operatorIndicator];
                    break;

                default:
                    opLocal = _Operators['\0'];
                    break;
            }
            return opLocal;
        }

        private const string varname = "[a-zA-Z0-9_]*";
        private const string op = "(?<op>[+#./;?&]?)";
        private const string var = "(?<var>(?:(?<lvar>" + varname + ")[*]?,?)*)";
        private const string varspec = "(?<varspec>{" + op + var + "})";

        // (?<varspec>{(?<op>[+#./;?&]?)(?<var>[a-zA-Z0-9_]*[*]?|(?:(?<lvar>[a-zA-Z0-9_]*[*]?),?)*)})

        private Regex _ParameterRegex;

        public IDictionary<string,object> GetParameters(Uri uri)
        {
            if (_ParameterRegex == null)
            {
                string matchingRegex = CreateMatchingRegex(_template);
                lock(this)
                {
                    _ParameterRegex = new Regex(matchingRegex);
                }
            }

            var match = _ParameterRegex.Match(uri.OriginalString);
            var parameters = new Dictionary<string, object>();

            for(int x = 1; x <= match.Groups.Count; x ++)
            {
                if (match.Groups[x].Success)
                {
                    string paramName = _ParameterRegex.GroupNameFromNumber(x);
                    if (!string.IsNullOrEmpty(paramName))
                    {
                        parameters.Add(paramName, Uri.UnescapeDataString(match.Groups[x].Value));
                    }
                }
            }
            return match.Success ? parameters : null;
        }

        public static string CreateMatchingRegex(string uriTemplate)
        {
            var findParam = new Regex(varspec);

            string template = new Regex(@"([^{]|^)\?").Replace(uriTemplate, @"$+\?");
            string regex = findParam.Replace(template, delegate (Match m)
            {
                var paramNames = m.Groups["lvar"].Captures.Cast<Capture>().Where(c => !string.IsNullOrEmpty(c.Value)).Select(c => c.Value).ToList();
                string opLocal = m.Groups["op"].Value;
                switch (opLocal)
                {
                    case "?":
                        return GetQueryExpression(paramNames, prefix: "?");
                    case "&":
                        return GetQueryExpression(paramNames, prefix: "&");
                    case "#":
                        return GetExpression(paramNames, prefix: "#" );
                    case "/":
                        return GetExpression(paramNames, prefix: "/");

                    case "+":
                        return GetExpression(paramNames);
                    default:
                        return GetExpression(paramNames);
                }
                
            });

            return regex +"$";
        }

        private static string GetQueryExpression(List<string> paramNames, string prefix)
        {
            var sb = new StringBuilder();
            foreach (string paramname in paramNames)
            {
                sb.Append(@"\"+prefix+"?");
                if (prefix == "?")
                {
                    prefix = "&";
                }

                sb.Append("(?:");
                sb.Append(paramname);
                sb.Append("=");

                sb.Append("(?<");
                sb.Append(paramname);
                sb.Append(">");
                sb.Append("[^/?&]+");
                sb.Append(")");
                sb.Append(")?");
            }

            return sb.ToString();
        }

        private static string GetExpression(IEnumerable<string> paramNames, string prefix = null)
        {
            var sb = new StringBuilder();

            string paramDelim;

            switch (prefix)
            {
                case "#":
                    paramDelim = "[^,]+";
                    break;
                case "/":
                    paramDelim = "[^/?]+";
                    break;
                case "?":
                case "&":
                    paramDelim = "[^&#]+";
                    break;
                case ";":
                    paramDelim = "[^;/?#]+";
                    break;
                case ".":
                    paramDelim = "[^./?#]+";
                    break;

                default:
                    paramDelim = "[^/?&]+";
                    break;
            }

            foreach (string paramname in paramNames)
            {
                if (string.IsNullOrEmpty(paramname))
                {
                    continue;
                }

                if (prefix != null)
                {
                    sb.Append(@"\" + prefix + "?");
                    if (prefix == "#") { prefix = ","; }
                }
                sb.Append("(?<");
                sb.Append(paramname);
                sb.Append(">");
                sb.Append(paramDelim); // Param Value
                sb.Append(")?");
            }

            return sb.ToString();
        }
    }
}
