using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Triangle2D
    {
        private Vector2 _a;
        private Vector2 _b;
        private Vector2 _c;

        private bool _isBackface;

        private Matrix2x2 _inverse;

        public Triangle2D(Vector2 a, Vector2 b, Vector2 c)
        {
            _a = a;
            _b = b;
            _c = c;

            var AB = _b - _a;
            var AC = _c - _a;

            // Backface Culling: when Z > 0 = Clockwise, z < 0 Counter Clockwise
            var cross = Vector3.Cross(new Vector3(AB, 0), new Vector3(AC, 0)); 
            _isBackface = cross.Z > 0;

            var A = new Matrix2x2(AB.X, AB.Y, AC.X, AC.Y);
            _inverse = A.Inverse();
        }
        
        public float MinX => Min(_a.X, _b.X, _c.X);

        public float MinY => Min(_a.Y, _b.Y, _c.Y);

        public float MaxX => Max(_a.X, _b.X, _c.X);

        public float MaxY => Max(_a.Y, _b.Y, _c.Y);

        public bool IsBackface => _isBackface;

        public Vector3 CalcColor(float x, float y)
        {
            var p = new Vector2(x, y);
            var AP = p - _a;
            var vec = _inverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                return Conversions.FromColor(Colors.LightSkyBlue);
            }

            return Vector3.Zero;
        }        
 
        private float Min(float f1, float f2, float f3)
        {
            float min = f1;
            if (f2 < min) min = f2;
            if (f3 < min) min = f3;
            return min;
        }

        private float Max(float f1, float f2, float f3)
        {
            float max = f1;
            if (f2 > max) max = f2;
            if (f3 > max) max = f3;
            return max;
        }

    }
}
