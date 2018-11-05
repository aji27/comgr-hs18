using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Triangle2D
    {
        private Vertex2D _a_2D;
        private Vertex2D _b_2D;
        private Vertex2D _c_2D;

        // Better Checkout this: http://www.scratchapixel.com/lessons/3d-basic-rendering/rasterization-practical-implementation/rasterization-stage

        private bool _isBackface;

        private Matrix2x2 _inverse;

        public Triangle2D(Vertex2D a_2D, Vertex2D b_2D, Vertex2D c_2D)
        {
            _a_2D = a_2D;
            _b_2D = b_2D;
            _c_2D = c_2D;

            var AB = _b_2D.Position - _a_2D.Position;
            var AC = _c_2D.Position - _a_2D.Position;

            // Backface Culling: when Z > 0 = Clockwise, z < 0 Counter Clockwise
            var cross = Vector3.Cross(new Vector3(AB, 0), new Vector3(AC, 0)); 
            _isBackface = cross.Z < 0;

            var A = new Matrix2x2(AB.X, AB.Y, AC.X, AC.Y);
            _inverse = A.Inverse();
        }
        
        public float MinX => Min(_a_2D.Position.X, _b_2D.Position.X, _c_2D.Position.X);

        public float MinY => Min(_a_2D.Position.Y, _b_2D.Position.Y, _c_2D.Position.Y);

        public float MaxX => Max(_a_2D.Position.X, _b_2D.Position.X, _c_2D.Position.X);

        public float MaxY => Max(_a_2D.Position.Y, _b_2D.Position.Y, _c_2D.Position.Y);

        public bool IsBackface => _isBackface;

        public (Vector3 color, float z) CalcColor(float x, float y)
        {
            var p = new Vector2(x, y);
            var AP = p - _a_2D.Position;
            var vec = _inverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                var homogenousColor = _a_2D.Color + u * (_b_2D.Color - _a_2D.Color) + v * (_c_2D.Color - _a_2D.Color);
                var color = new Vector3(homogenousColor.X / homogenousColor.W, homogenousColor.Y / homogenousColor.W, homogenousColor.Z / homogenousColor.W);

                var homogenousPosition = _a_2D.HomogenousPosition + u * (_b_2D.HomogenousPosition - _a_2D.HomogenousPosition) + v * (_c_2D.HomogenousPosition - _a_2D.HomogenousPosition);
                var position = new Vector3(homogenousPosition.X / homogenousPosition.W, homogenousPosition.Y / homogenousPosition.W, homogenousPosition.Z / homogenousPosition.W);

                return (color, position.Z);
            }

            return (Vector3.Zero, 0);
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
