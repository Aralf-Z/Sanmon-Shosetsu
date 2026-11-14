/*
Name: VirtualMachine.cs
Time: @2024.01.26
Auth: Prince Biscuit/王子饼干
Desc: A simple virtual machine to execute syntax tree parsed by SyntaxAnalyzer.
      it use C#'s reflection to access properties and methods, no other dependencices.
      lightweight and easy to use. it highly decoupled with Console part, so it could be used in other projects singlely.
      also because using reflection, it's not fast enough, so it's usually used in console input to do 
      fast and simple operations, like set/get property, call method, etc.
      in this case, it's fast enough and very useful.
*/
using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> manage a group of value parsers. </summary>
    public class ValueParserManager
    {
        private readonly Dictionary<Type, ValueParser> _parsers = new(); // Parsers.

        private readonly Dictionary<string, Type> _typeAlias = new(); // Type alias.

        /// <summary> Create a ValueParserManager instance. </summary>
        public ValueParserManager()
        {
        }

        /// <summary> Create a ValueParserManager instance. </summary>
        public ValueParserManager(Dictionary<Type, ValueParser> parsers, Dictionary<string, Type> typeAlias)
        {
            this._parsers = parsers;
            this._typeAlias = typeAlias;
        }

        /// <summary> register new parser, if target type is already registered, then it will be replaced. </summary>
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

        /// <summary> Try parse. </summary>
        public bool TryParse(Type type, string inputStr, out object data)
        {
            if (_parsers.TryGetValue(type, out var parser))
            {
                return parser(inputStr, out data);
            }

            data = null;
            return false;
        }

        /// <summary> Try parse safe. </summary>
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

        /// <summary> Query type. </summary>
        public bool QueryType(string alias, out Type type)
        {
            return _typeAlias.TryGetValue(alias, out type);
        }
    }
}
