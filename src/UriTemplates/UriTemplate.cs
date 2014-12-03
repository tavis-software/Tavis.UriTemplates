using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tavis.UriTemplates
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;


        public class UriTemplate
        {


            private static Dictionary<char, OperatorInfo> _Operators = new Dictionary<char, OperatorInfo>() {
                                        {'\0', new OperatorInfo {Default = true, First = "", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = false}},
                                        {'+', new OperatorInfo {Default = false, First = "", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = true}},
                                        {'.', new OperatorInfo {Default = false, First = ".", Seperator = '.', Named = false, IfEmpty = "",AllowReserved = false}},
                                        {'/', new OperatorInfo {Default = false, First = "/", Seperator = '/', Named = false, IfEmpty = "",AllowReserved = false}},
                                        {';', new OperatorInfo {Default = false, First = ";", Seperator = ';', Named = true, IfEmpty = "",AllowReserved = false}},
                                        {'?', new OperatorInfo {Default = false, First = "?", Seperator = '&', Named = true, IfEmpty = "=",AllowReserved = false}},
                                        {'&', new OperatorInfo {Default = false, First = "&", Seperator = '&', Named = true, IfEmpty = "=",AllowReserved = false}},
                                        {'#', new OperatorInfo {Default = false, First = "#", Seperator = ',', Named = false, IfEmpty = "",AllowReserved = true}}
                                        };

            private readonly string _template;
            private readonly Dictionary<string, object> _Parameters = new Dictionary<string, object>();
            private enum States { CopyingLiterals, ParsingExpression }
            private bool _ErrorDetected = false;
            private Result _Result;
            private List<string> _ParameterNames;

            private bool _resolvePartially;

            public UriTemplate(string template, bool resolvePartially = false )
            {
                _resolvePartially = resolvePartially;
                _template = template;
            }


            public void SetParameter(string name, object value)
            {
                _Parameters[name] = value;
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
                var parameterNames = new List<string>();
                _ParameterNames = parameterNames;
                Resolve();
                _ParameterNames = null;
                return parameterNames;
            }

            public string Resolve()
            {
                var currentState = States.CopyingLiterals;
                _Result = new Result();
                StringBuilder currentExpression = null;
                foreach (var character in _template.ToCharArray())
                {
                    switch (currentState)
                    {
                        case States.CopyingLiterals:
                            if (character == '{')
                            {
                                currentState = States.ParsingExpression;
                                currentExpression = new StringBuilder();
                            }
                            else if (character == '}')
                            {
                                throw new ArgumentException("Malformed template, unexpected } : " + _Result.ToString());
                            }
                            else
                            {
                                _Result.Append(character);
                            }
                            break;
                        case States.ParsingExpression:
                            if (character == '}')
                            {
                                ProcessExpression(currentExpression);

                                currentState = States.CopyingLiterals;
                            }
                            else
                            {
                                currentExpression.Append(character);
                            }

                            break;
                    }
                }
                if (currentState == States.ParsingExpression)
                {
                    _Result.Append("{");
                    _Result.Append(currentExpression.ToString());

                    throw new ArgumentException("Malformed template, missing } : " + _Result.ToString());
                }

                if (_ErrorDetected)
                {
                    throw new ArgumentException("Malformed template : " + _Result.ToString());
                }
                return _Result.ToString();
            }

            private void ProcessExpression(StringBuilder currentExpression)
            {

                if (currentExpression.Length == 0)
                {
                    _ErrorDetected = true;
                    _Result.Append("{}");
                    return;
                }

                OperatorInfo op = GetOperator(currentExpression[0]);

                var firstChar = op.Default ? 0 : 1;
                bool multivariableExpression = false;

                var varSpec = new VarSpec(op);
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
                                if (i < currentExpression.Length) currentChar = currentExpression[i];
                            }
                            varSpec.PrefixLength = int.Parse(prefixText.ToString());
                            i--;
                            break;

                        case ',':
                            multivariableExpression = true;
                            var success = ProcessVariable(varSpec, multivariableExpression);
                            bool isFirst = varSpec.First;
                            // Reset for new variable
                            varSpec = new VarSpec(op);
                            if (success || !isFirst || _resolvePartially) varSpec.First = false;
                            if (!success && _resolvePartially) {_Result.Append(",") ; }
                            break; 
                        

                        default:
                            if (IsVarNameChar(currentChar))
                            {
                                varSpec.VarName.Append(currentChar);
                            }
                            else
                            {
                                _ErrorDetected = true;
                            }
                            break;
                    }
                }

                ProcessVariable(varSpec, multivariableExpression);
                if (multivariableExpression && _resolvePartially) _Result.Append("}");
            }

            private bool ProcessVariable(VarSpec varSpec, bool multiVariableExpression = false)
            {
                var varname = varSpec.VarName.ToString();
                if (_ParameterNames != null) _ParameterNames.Add(varname);

                if (!_Parameters.ContainsKey(varname)
                    || _Parameters[varname] == null
                    || (_Parameters[varname] is IList && ((IList) _Parameters[varname]).Count == 0)
                    || (_Parameters[varname] is IDictionary && ((IDictionary) _Parameters[varname]).Count == 0))
                {
                    if (_resolvePartially == true)
                    {
                        if (multiVariableExpression)
                        {
                            if (varSpec.First)
                            {
                                _Result.Append("{");
                            }

                            _Result.Append(varSpec.ToString());
                        }
                        else
                        {
                            _Result.Append("{");
                            _Result.Append(varSpec.ToString());
                            _Result.Append("}");
                        }
                        return false;
                    }
                    return false;
                }

                if (varSpec.First)
                {
                    _Result.Append(varSpec.OperatorInfo.First);
                }
                else
                {
                    _Result.Append(varSpec.OperatorInfo.Seperator);
                }

                object value = _Parameters[varname];

                // Handle Strings
                if (value is string)
                {
                    var stringValue = (string)value;
                    if (varSpec.OperatorInfo.Named)
                    {
                        _Result.AppendName(varname, varSpec.OperatorInfo, string.IsNullOrEmpty(stringValue));
                    }
                    _Result.AppendValue(stringValue, varSpec.PrefixLength, varSpec.OperatorInfo.AllowReserved);
                }
                else
                {
                    // Handle Lists
                    var list = value as IList;
                    if (list == null && value is IEnumerable<string>)
                    {
                        list = ((IEnumerable<string>)value).ToList<string>();
                    } ;
                    if (list != null)
                    {
                        if (varSpec.OperatorInfo.Named && !varSpec.Explode)  // exploding will prefix with list name
                        {
                            _Result.AppendName(varname, varSpec.OperatorInfo, list.Count == 0);
                        }

                        _Result.AppendList(varSpec.OperatorInfo, varSpec.Explode, varname, list);
                    }
                    else
                    {

                        // Handle associative arrays
                        var dictionary = value as IDictionary<string, string>;
                        if (dictionary != null)
                        {
                            if (varSpec.OperatorInfo.Named && !varSpec.Explode)  // exploding will prefix with list name
                            {
                                _Result.AppendName(varname, varSpec.OperatorInfo, dictionary.Count() == 0);
                            }
                            _Result.AppendDictionary(varSpec.OperatorInfo, varSpec.Explode, dictionary);
                        }
                        else
                        {
                            // If above all fails, convert the object to string using the default object.ToString() implementation
                            var stringValue = value.ToString();
                            if (varSpec.OperatorInfo.Named)
                            {
                                _Result.AppendName(varname, varSpec.OperatorInfo, string.IsNullOrEmpty(stringValue));
                            }
                            _Result.AppendValue(stringValue, varSpec.PrefixLength, varSpec.OperatorInfo.AllowReserved);
                        }

                    }

                }
                return true;
            }



            private static bool IsVarNameChar(char c)
            {
                return ((c >= 'A' && c <= 'z') //Alpha
                        || (c >= '0' && c <= '9') // Digit
                        || c == '_'
                        || c == '%'
                        || c == '.');
            }
            private static OperatorInfo GetOperator(char operatorIndicator)
            {
                OperatorInfo op;
                switch (operatorIndicator)
                {

                    case '+':
                    case ';':
                    case '/':
                    case '#':
                    case '&':
                    case '?':
                    case '.':
                        op = _Operators[operatorIndicator];
                        break;

                    default:
                        op = _Operators['\0'];
                        break;
                }
                return op;
            }

        
        }

        
    }
