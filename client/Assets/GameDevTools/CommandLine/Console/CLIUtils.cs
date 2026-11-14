using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Cliutils. </summary>
    public static class CLIUtils
    {
        /// <summary> get time information of [HH:mm:ss]. </summary>
        public static string TimeInfo
        {
            get
            {
                var time = DateTime.Now;
                return $"[{PadZero(time.Hour)}:{PadZero(time.Minute)}:{PadZero(time.Second)}] ";
            }
        }

        /// <summary> find similarity of two strings. x: string x. y: string y. </summary>
        public static float FindSimilarity(string x, string y)
        {
            if (x == null || y == null)
                return 0;
            float maxLength = Math.Max(x.Length, y.Length);
            if (maxLength > 0)
            {
                // optionally ignore case if needed
                return (maxLength - GetEditDistance(x, y)) / maxLength;
            }

            return 1.0f;
        }

        // Get edit distance.
        private static int GetEditDistance(string x, string y)
        {
            int m = x.Length;
            int n = y.Length;
            int[][] t = new int[m + 1][];
            for (int i = 0; i < m + 1; ++i)
            {
                t[i] = new int[n + 1];
            }

            for (int i = 1; i <= m; i++)
            {
                t[i][0] = i;
            }

            for (int j = 1; j <= n; j++)
            {
                t[0][j] = j;
            }

            int cost;
            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    cost = x[i - 1] == y[j - 1] ? 0 : 1;
                    t[i][j] = Math.Min(Math.Min(t[i - 1][j] + 1, t[i][j - 1] + 1), t[i - 1][j - 1] + cost);
                }
            }

            return t[m][n];
        }

        // Pad zero.
        private static string PadZero(int value)
        {
            return value < 10 ? $"0{value}" : $"{value}";
        }
    }
}
