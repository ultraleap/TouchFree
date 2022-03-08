using System;

namespace Ultraleap.TouchFree.Library
{
    public struct Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 Zero { get { return new Vector3(0, 0, 0); } }

        public static Vector3 operator +(Vector3 a) => a;
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);

        public static Vector3 operator +(Vector3 a, Vector3 b)
            => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        public static Vector3 operator -(Vector3 a, Vector3 b)
            => a + (-b);

        public static Vector3 operator /(Vector3 a, float b)
        {
            if (b == 0)
            {
                throw new DivideByZeroException();
            }
            return new Vector3(a.X / b, a.Y / b, a.Z / b);
        }

        public static Vector3 operator *(Vector3 a, float b)
            => new Vector3(a.X * b, a.Y * b, a.Z * b);

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return a.X != b.X || a.Y != b.Y || a.Z != b.Z;
        }

        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }
    }
}
