using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GameConsole
{
    /// <summary> 检查 JsonUtility 会补默认值或转换类型的字段, 具体数据仍由 JsonUtility 读取. </summary>
    internal static class JsonShape
    {
        private static readonly Regex _tokens = new(@"""(?:[^""\\]|\\.)*""|[{}\[\]:,]|[^""\s{}\[\]:,]+", RegexOptions.Compiled);

        internal static Dictionary<string, Match> ReadFields(string json) => ReadFields(_tokens.Match(json));

        internal static Dictionary<string, Match> ReadFields(Match start)
        {
            var fields = new Dictionary<string, Match>(StringComparer.Ordinal);
            var token = After(start, '{');

            while (Kind(token) != '}')
            {
                var colon = After(token, '"');
                var value = After(colon, ':');
                var name = ReadName(token.Value);

                if (!fields.TryAdd(name, value))
                {
                    throw new FormatException("Duplicate JSON property: " + name);
                }

                token = AfterValue(value, '}');
            }

            return fields;
        }

        internal static IEnumerable<Match> ReadItems(Match start)
        {
            var token = After(start, '[');

            while (Kind(token) != ']')
            {
                yield return token;
                token = AfterValue(token, ']');
            }
        }

        internal static Match Require(IReadOnlyDictionary<string, Match> fields, string name, char kind)
        {
            if (fields.TryGetValue(name, out var value) && IsKind(value, kind)) return value;

            throw new FormatException("Missing or invalid JSON property: " + name);
        }

        internal static bool IsKind(Match token, char kind) => Kind(token) == kind;

        private static Match AfterValue(Match token, char end)
        {
            if (!token.Success)
            {
                throw new FormatException("Missing JSON value.");
            }

            var depth = 0;
            do
            {
                var kind = Kind(token);

                if (kind is '{' or '[')
                {
                    depth++;
                }
                else if (kind is '}' or ']')
                {
                    depth--;
                }

                token = token.NextMatch();
            }
            while (depth > 0 && token.Success);

            return Kind(token) == end ? token : After(token, ',');
        }

        private static Match After(Match token, char expected)
        {
            if (Kind(token) == expected) return token.NextMatch();

            throw new FormatException("Expected JSON token: " + expected);
        }

        private static char Kind(Match token) => token.Success ? token.Value[0] : '\0';

        private static string ReadName(string token)
        {
            if (!token.Contains('\\')) return token.Substring(1, token.Length - 2);

            var escapedName = JsonUtility.FromJson<EscapedName>("{\"value\":" + token + "}");

            return escapedName.value;
        }

        [Serializable]
        private sealed class EscapedName
        {
            public string value;
        }
    }
}
