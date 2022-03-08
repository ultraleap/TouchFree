using System;

namespace Ultraleap.TouchFree.Library
{
    public struct Vector2
    {
        public float X, Y;
        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero { get { return new Vector2(0, 0); } }

        public static Vector2 operator +(Vector2 a) => a;
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);

        public static Vector2 operator +(Vector2 a, Vector2 b)
            => new Vector2(a.X + b.X, a.Y + b.Y);

        public static Vector2 operator -(Vector2 a, Vector2 b)
            => a + (-b);

        public static Vector2 operator /(Vector2 a, float b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }
            return new Vector2(a.X / b, a.Y / b);
        }

        public static Vector2 operator *(Vector2 a, float b)
            => new Vector2(a.X * b, a.Y * b);

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        internal static Vector2 Normalize(Vector2 vector2)
        {
            return vector2 / vector2.Length();
        }

        internal static float Distance(Vector2 previous, Vector2 current)
        {
            return (previous - current).Length();
        }

        internal static float Dot(Vector2 defaultPositionChange, Vector2 previousConstraintVector)
        {
            return defaultPositionChange.X * previousConstraintVector.X +
                defaultPositionChange.Y * previousConstraintVector.Y;
        }
    }
}
