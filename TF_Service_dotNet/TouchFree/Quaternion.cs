using System;

namespace Ultraleap.TouchFree.Library
{
    public struct Quaternion
    {
        public float X, Y, Z, W;

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public static Quaternion CreateFromYawPitchRoll(double yaw, double pitch, double roll)
        {
            var cy = Math.Cos(yaw * 0.5);
            var sy = Math.Sin(yaw * 0.5);
            var cp = Math.Cos(pitch * 0.5);
            var sp = Math.Sin(pitch * 0.5);
            var cr = Math.Cos(roll * 0.5);
            var sr = Math.Sin(roll * 0.5);

            return new Quaternion(
                (float)(cr * sp * cy + sr * cp * sy),
                (float)(cr * cp * sy - sr * sp * cy),
                (float)(sr * cp * cy - cr * sp * sy),
                (float)(cr * cp * cy + sr * sp * sy));
        }
    }
}
