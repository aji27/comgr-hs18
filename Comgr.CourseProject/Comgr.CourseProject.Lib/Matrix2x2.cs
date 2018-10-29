using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public struct Matrix2x2
    {
        public float X1;
        public float X2;

        public float Y1;
        public float Y2;

        public Matrix2x2(float x1, float y1, float x2, float y2)
        {
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
        }

        public Matrix2x2 Inverse()
        {
            var s = 1f / (X1 * Y2 - X2 * Y1);
            var m = new Matrix2x2(Y2, -Y1, -X2, X1);
            return s * m;
        }

        public static Matrix2x2 operator *(float scalar, Matrix2x2 m) => new Matrix2x2(scalar * m.X1, scalar * m.Y1, scalar * m.X2, scalar * m.Y2);

        public static Vector2 operator *(Matrix2x2 m, Vector2 v)
        {
            var x = m.X1 * v.X + m.X2 * v.Y;
            var y = m.Y1 * v.X + m.Y2 * v.Y;

            return new Vector2(x, y);
        }
    }
}
