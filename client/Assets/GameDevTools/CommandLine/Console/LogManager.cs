using System;
using System.Collections.Generic;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Log manager. </summary>
    public class LogManager<T>
        where T : Enum
    {
        /// <summary> this event would triggered while receive messages. </summary>
        public event Action<Log> e_onReceivedMessage;

        /// <summary> get total logs of current console. </summary>
        public IEnumerable<Log> AllLogs => _logs;

        private readonly int _logCapacity; // Log capacity.

        private readonly bool _hasLogCapacity; // Has log capacity.

        private readonly List<Log> _logs = new(); // Logs.

        /// <summary> initialize log manager. logCapacity: the capacity of logs, -1 as infinite. </summary>
        public LogManager(int logCapacity = -1)
        {
            this._logCapacity = logCapacity;
            this._hasLogCapacity = logCapacity > 0;
        }

        /// <summary> output a debug log on console, if info is null, then the tag would work on it. </summary>
        public void Output(string info, T type = default)
        {
            if (info == null || info.Length == 0)
            {
                Output(Log.Empty);
                return;
            }

            Output(new(info, type));
        }

        /// <summary> output a debug log on console, if info is null, then the tag would work on it, the color should in format of &apos;#ffffff&apos;;. </summary>
        public void Output(string info, string color, T type = default)
        {
            if (color == null || color.Length == 0)
            {
                Output(info, type);
                return;
            }

            if (info == null || info.Length == 0)
            {
                Output(Log.Empty);
                return;
            }

            Output(new(info, color, type));
        }

        /// <summary> get last logs. </summary>
        public IEnumerable<Log> GetLastLogs(int count)
        {
            if (count == 0)
                yield break;
            count = Math.Min(count, _logs.Count);
            int l = _logs.Count - count;
            int r = l + count;
            for (int i = l; i < r; i++)
            {
                yield return _logs[i];
            }
        }

        /// <summary> get last logs with target tag. </summary>
        public IEnumerable<Log> GetLastLogs(int count, T type = default)
        {
            if (count == 0)
                yield break;
            foreach (var log in _logs)
            {
                if (log.logType.Equals(type))
                {
                    yield return log;
                }

                if (--count <= 0)
                    break;
            }
        }

        // Output.
        private void Output(Log log)
        {
            /* trigger event */
            _logs.Add(log);
            if (_hasLogCapacity && _logs.Count >= _logCapacity)
            {
                _logs.RemoveAt(0);
            }

            e_onReceivedMessage?.Invoke(log);
        }

        /* about output logs */
        /// <summary> console log. </summary>
        public readonly struct Log
        {
            /// <summary> Empty. </summary>
            public static Log Empty => new(string.Empty, default);

            /// <summary> Message. </summary>
            public readonly string message;

            /// <summary> Color. </summary>
            public readonly string color;

            /// <summary> Log type. </summary>
            public readonly T logType;

            /// <summary> Has color. </summary>
            public readonly bool HasColor => color != null && color.Length > 0;

            /// <summary> Create a Log instance. </summary>
            public Log(string message, string color, T logType = default)
            {
                this.message = message;
                this.color = color;
                this.logType = logType;
            }

            /// <summary> Create a Log instance. </summary>
            public Log(string message, T logType = default)
            {
                this.message = message;
                this.color = null;
                this.logType = logType;
            }

            /// <summary> To string. </summary>
            public override string ToString()
            {
                return color != null ? $"<color={color}>{message}</color>" : message;
            }
        }
    }
}
