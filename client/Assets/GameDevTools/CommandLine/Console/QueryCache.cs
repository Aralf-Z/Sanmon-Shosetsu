/* MIT License - CommandSystem.cs; Created By : Prince Biscuit */
using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> query buffer. </summary>
    class QueryCache<T>
        where T : struct
    {
        private readonly Dictionary<string, T[]> _buffer = new();

        private readonly List<string> _queryHistory = new();

        private readonly int _capacity; // Capacity.

        public QueryCache(int capacity)
        {
            this._capacity = Math.Max(1, capacity);
        }

        /// <summary> store a query result. query: query string, this must not be a empty or null value. result: query result, must not be a null value. </summary>
        public void Cache(string query, T[] result)
        {
            if (_buffer.ContainsKey(query)) return;

            if (_queryHistory.Count + 1 > _capacity)
            {
                _buffer.Remove(_queryHistory[0]);
                _queryHistory.RemoveAt(0);
            }

            _buffer.Add(query, result);
            _queryHistory.Add(query);
        }

        /// <summary> check if some query has been recorded. query: query string, and you should check if it is null or empty before you call this function. </summary>
        public bool GetCache(string query, out T[] result)
        {
            return _buffer.TryGetValue(query, out result);
        }
    }
}
