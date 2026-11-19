using System;

namespace GameConsole
{
    [Flags]
    internal enum FloatingFrameMetrics
    {
        None = 0,
        RealtimeFps = 1,
        AverageFps = 2,
        LowFps = 4,
        FrameTimeGraph = 8,
        FpsValues = RealtimeFps | AverageFps | LowFps
    }

    /// <summary> 以采样完成时刻维护最近十秒的逐帧耗时. </summary>
    internal sealed class FloatingFrameSamples
    {
        internal const double WINDOW_SECONDS = 10d;

        internal int Count { get; private set; }

        internal double RealtimeFps => Count == 0 ? double.NaN : 1d / At(Count - 1).Seconds;

        internal double AverageFps => Count == 0 ? double.NaN : Count / _totalSeconds;

        private Sample[] _samples = new Sample[256];

        private float[] _sortedSeconds = Array.Empty<float>();

        private int _start;

        private double _totalSeconds;

        internal bool Add(double now, float seconds)
        {
            while (Count > 0 && _samples[_start].Time < now - WINDOW_SECONDS)
            {
                _totalSeconds -= _samples[_start].Seconds;
                _start = (_start + 1) % _samples.Length;
                Count--;
            }

            if (Count == 0) _totalSeconds = 0d;
            if (seconds <= 0f || float.IsNaN(seconds) || float.IsInfinity(seconds)) return false;
            if (Count == _samples.Length)
            {
                var resized = new Sample[_samples.Length * 2];

                for (var index = 0; index < Count; index++)
                {
                    resized[index] = At(index);
                }

                _samples = resized;
                _start = 0;
            }

            _samples[(_start + Count) % _samples.Length] = new Sample(now, seconds);
            _totalSeconds += seconds;
            Count++;
            return true;
        }

        internal double OnePercentLow()
        {
            if (Count == 0) return double.NaN;
            if (_sortedSeconds.Length < Count)
            {
                Array.Resize(ref _sortedSeconds, _samples.Length);
            }

            for (var index = 0; index < Count; index++)
            {
                _sortedSeconds[index] = At(index).Seconds;
            }

            Array.Sort(_sortedSeconds, 0, Count);
            var tail = (int)Math.Ceiling(Count * .01d);
            var sum = 0d;

            for (var index = Count - tail; index < Count; index++)
            {
                sum += _sortedSeconds[index];
            }

            return tail / sum;
        }

        internal float FillPeaks(double now, float[] peaks, int width)
        {
            Array.Clear(peaks, 0, width);
            var start = now - WINDOW_SECONDS;
            var maximum = 0f;

            for (var index = 0; index < Count; index++)
            {
                var sample = At(index);

                if (sample.Time < start || sample.Time > now) continue;

                var bucket = Math.Min(width - 1, (int)((sample.Time - start) / WINDOW_SECONDS * width));
                var milliseconds = sample.Seconds * 1000f;

                peaks[bucket] = Math.Max(peaks[bucket], milliseconds);
                maximum = Math.Max(maximum, milliseconds);
            }

            return maximum;
        }

        internal void Clear()
        {
            _start = 0;
            Count = 0;
            _totalSeconds = 0d;
        }

        private Sample At(int index) => _samples[(_start + index) % _samples.Length];

        private readonly struct Sample
        {
            internal double Time { get; }

            internal float Seconds { get; }

            internal Sample(double time, float seconds)
            {
                Time = time;
                Seconds = seconds;
            }
        }
    }
}

