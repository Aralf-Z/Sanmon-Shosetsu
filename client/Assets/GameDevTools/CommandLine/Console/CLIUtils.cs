using System;

namespace RedSaw.CommandLineInterface
{
    /// <summary> Cliutils. </summary>
    public static class CLIUtils
    {
        public static string TimeInfo
        {
            get
            {
                var time = DateTime.Now;

                return $"[{PadZero(time.Hour)}:{PadZero(time.Minute)}:{PadZero(time.Second)}] ";
            }
        }

        public static float FindSimilarity(string x, string y)
        {
            if (x == null || y == null) return 0;

            float maxLength = Math.Max(x.Length, y.Length);

            if (maxLength > 0) return (maxLength - GetEditDistance(x, y)) / maxLength;

            return 1.0f;
        }

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

        private static string PadZero(int value)
        {
            return value < 10
                ? $"0{value}"
                : $"{value}";
        }
    }
}
