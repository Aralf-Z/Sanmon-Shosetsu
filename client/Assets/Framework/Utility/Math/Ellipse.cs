using System;

namespace Sanmon.Utility.Math
{   
    public class Ellipse
    {
        public float EllipseA { get; }

        public float EllipseB { get; }

        private readonly float mXDelta;
        private readonly float mYDelta;

        public Ellipse(float aValue, float bValue, float xDelta = 0, float yDelta = 0)
        {
            EllipseA = aValue;
            EllipseB = bValue;
            mXDelta = xDelta;
            mYDelta = yDelta;
        }

        public (float yMax, float yMin) GetYFromX(float x)
        {
            var y = MathF.Sqrt(1 - (x + mXDelta) * (x + mXDelta) / (EllipseA * EllipseA)) * EllipseB;

            return (y - mYDelta, -y - mYDelta);
        }
    }
} 