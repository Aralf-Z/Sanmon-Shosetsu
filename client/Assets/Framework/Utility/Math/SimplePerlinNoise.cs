using System;

namespace Sanmon.Utility.Math
{
    public class SimplePerlinNoise
    {
        private readonly int mScale;

        private readonly int[] mPermutation;

        public SimplePerlinNoise(int seed, int scale = 16):this(new Random(seed), scale)
        {}
        
        public SimplePerlinNoise(int scale = 16):this(new Random(), scale)
        {}

        private SimplePerlinNoise(Random random, int scale)
        {
            mScale = scale;
            mPermutation = new int[512];

            for (var i = 0; i < 256; i++)
            {
                mPermutation[i] = i;
            }

            for (var i = 0; i < 256; i++)
            {
                var j = random.Next(i, 256);
                (mPermutation[i], mPermutation[j]) = (mPermutation[j], mPermutation[i]);
            }

            for (var i = 0; i < 256; i++)
            {
                mPermutation[i + 256] = mPermutation[i];
            }
        }

        public float Get(int x, int y)
        {
            var gridX = x / mScale;
            var gridY = y / mScale;

            var x0 = x % mScale / (float)mScale;
            var y0 = y % mScale / (float)mScale;

            var a = GradsDotProduct(gridX, gridY, -x0, -y0);
            var b = GradsDotProduct(gridX, gridY + 1, -x0, 1 - y0);
            var c = GradsDotProduct(gridX + 1, gridY + 1, 1 - x0, 1 - y0);
            var d = GradsDotProduct(gridX + 1, gridY, 1 - x0, -y0);

            var u = Smooth(x0);
            var v = Smooth(y0);

            var lerp1 = Lerp(a, d, u);
            var lerp2 = Lerp(b, c, u);
            var noise = Lerp(lerp1, lerp2, v);

            return (noise + 1) / 2f;
        }

        private float GradsDotProduct(int gridX, int gridY, float x, float y)
        {
            var hash = mPermutation[mPermutation[gridX] + gridY] & 0b111;

            return hash switch
            {
                0 => x + y,
                1 => x - y,
                2 => -x + y,
                3 => -x - y,
                4 => x,
                5 => -x,
                6 => y,
                7 => -y,
                _ => 0
            };
        }

        private static float Lerp(float a, float b, float t) => a + (b - a) * t;

        private static float Smooth(float x) => x * x * x * (x * (x * 6 - 15) + 10);
    }
}