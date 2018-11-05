using System;
using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Triangle2D
    {
        private Vertex2D _a_2D;
        private Vertex2D _b_2D;
        private Vertex2D _c_2D;
                
        private bool _isBackface;

        private Matrix2x2 _inverse;

        private Vector3 _surfaceNormal;

        public Triangle2D(Vertex2D a_2D, Vertex2D b_2D, Vertex2D c_2D)
        {
            _a_2D = a_2D;
            _b_2D = b_2D;
            _c_2D = c_2D;

            var AB = NormalizeByW(_b_2D.HomogenousPosition) - NormalizeByW(_a_2D.HomogenousPosition);
            var AC = NormalizeByW(_c_2D.HomogenousPosition) - NormalizeByW(_a_2D.HomogenousPosition);

            _surfaceNormal = -Vector3.Normalize(Vector3.Cross(AB, AC));

            var AB_2D = _b_2D.Position - _a_2D.Position;
            var AC_2D = _c_2D.Position - _a_2D.Position;

            // Backface Culling: when Z > 0 = Clockwise, z < 0 Counter Clockwise
            var cross = Vector3.Cross(new Vector3(AB_2D, 0), new Vector3(AC_2D, 0)); 
            _isBackface = cross.Z < 0;

            var A = new Matrix2x2(AB_2D.X, AB_2D.Y, AC_2D.X, AC_2D.Y);
            _inverse = A.Inverse();
        }
        
        public float MinX => Min(_a_2D.Position.X, _b_2D.Position.X, _c_2D.Position.X);

        public float MinY => Min(_a_2D.Position.Y, _b_2D.Position.Y, _c_2D.Position.Y);

        public float MaxX => Max(_a_2D.Position.X, _b_2D.Position.X, _c_2D.Position.X);

        public float MaxY => Max(_a_2D.Position.Y, _b_2D.Position.Y, _c_2D.Position.Y);

        public bool IsBackface => _isBackface;

        private static Vector3 NormalizeByW(Vector4 v) => new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);

        public (Vector3 color, float z) CalcColor(float x, float y, LightSource[] lightSources)
        {
            var p = new Vector2(x, y);
            var AP = p - _a_2D.Position;
            var vec = _inverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                var color = Vector3.Zero;

                var interpolatedMaterial = _a_2D.Color + u * (_b_2D.Color - _a_2D.Color) + v * (_c_2D.Color - _a_2D.Color);
                var material = NormalizeByW(interpolatedMaterial);

                var interpolatedPosition = _a_2D.HomogenousPosition + u * (_b_2D.HomogenousPosition - _a_2D.HomogenousPosition) + v * (_c_2D.HomogenousPosition - _a_2D.HomogenousPosition);
                var position = NormalizeByW(interpolatedPosition);

                for (int i = 0; i < lightSources.Length; i++)
                {
                    var lightSource = lightSources[i];

                    var lVec = lightSource.Center - position;
                    var lVecNorm = Vector3.Normalize(lVec);
 
                    var light_cos = Vector3.Dot(_surfaceNormal, lVecNorm);

                    if (light_cos >= 0)
                    {
                        // Diffuse Lambert
                        var light = Conversions.FromColor(lightSource.Color);
                        var diffuse = Vector3.Multiply(light, material) * light_cos;
                        color += diffuse;

                        // Specular Phong
                        var eye = new Vector3(0, 0, 0);
                        var rayVecNorm = Vector3.Normalize(position - eye);

                        var specularPhongFactor = 40;
                        var sVec = (lVec - ((Vector3.Dot(lVec, _surfaceNormal)) * _surfaceNormal));
                        var rVec = lVec - (2 * sVec);
                        var specular = light * (float)Math.Pow((Vector3.Dot(Vector3.Normalize(rVec), rayVecNorm)), specularPhongFactor);
                        color += specular;
                    }
                }

                return (color, position.Z);
            }

            return (Vector3.Zero, float.PositiveInfinity);
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
