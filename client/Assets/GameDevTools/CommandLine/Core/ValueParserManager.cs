/* Auth: Prince Biscuit/王子饼干 */
using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> manage a group of value parsers. </summary>
    public class ValueParserManager
    {
        private readonly Dictionary<Type, ValueParser> _parsers = new();

        private readonly Dictionary<string, Type> _typeAlias = new();

        public ValueParserManager()
        {
        }

        public ValueParserManager(Dictionary<Type, ValueParser> parsers, Dictionary<string, Type> typeAlias)
        {
            this._parsers = parsers;
            this._typeAlias = typeAlias;
        }

        public void RegisterParser(Type type, ValueParser parser, string typeAlias = null)
        {
            if (_parsers.ContainsKey(type))
            {
                _parsers[type] = parser;
            }
            else
            {
                _parsers.Add(type, parser);
            }

            string alias = typeAlias ?? type.Name;

            if (this._typeAlias.ContainsKey(alias))
            {
                this._typeAlias[alias] = type;
            }
            else
            {
                this._typeAlias.Add(alias, type);
            }
        }

        public bool TryParse(Type type, string inputStr, out object data)
        {
            if (_parsers.TryGetValue(type, out var parser)) return parser(inputStr, out data);

            data = null;
            return false;
        }

        public bool TryParseSafe(Type type, string inputStr, out object data)
        {
            if (_parsers.TryGetValue(type, out var parser))
            {
                try
                {
                    return parser(inputStr, out data);
                }
                catch (Exception)
                {
                    data = null;
                    return false;
                }
            }

            data = null;
            return false;
        }

        public bool QueryType(string alias, out Type type)
        {
            return _typeAlias.TryGetValue(alias, out type);
        }
    }
}
