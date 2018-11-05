using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class Triangle
    {
        private Vertex _a;
        private Vertex _b;
        private Vertex _c;
        private Vector3 _startSurfaceNormal;
        private Vector3 _currentSurfaceNormal;

        private bool _isBackfacingOnScreen;
        private Matrix2x2 _barryCentricMatrixInverse;

        public Triangle(Vertex a, Vertex b, Vertex c, Vector3 surfaceNormal)
        {
            _a = a;
            _b = b;
            _c = c;

            _startSurfaceNormal = surfaceNormal;
            _currentSurfaceNormal = _startSurfaceNormal;

            OnPropertyChanged();
        }

        public bool IsBackfacing => _isBackfacingOnScreen;

        public float MinScreenX => Min(_a.ScreenPosition.X, _b.ScreenPosition.X, _c.ScreenPosition.X);

        public float MinScreenY => Min(_a.ScreenPosition.Y, _b.ScreenPosition.Y, _c.ScreenPosition.Y);

        public float MaxScreenX => Max(_a.ScreenPosition.X, _b.ScreenPosition.X, _c.ScreenPosition.X);

        public float MaxScreenY => Max(_a.ScreenPosition.Y, _b.ScreenPosition.Y, _c.ScreenPosition.Y);
        
        public void ApplyTransform(Matrix4x4 matrix)
        {
            _a.ApplyTransform(matrix);
            _b.ApplyTransform(matrix);
            _c.ApplyTransform(matrix);

            _currentSurfaceNormal = Vector3.TransformNormal(_startSurfaceNormal, matrix);

            OnPropertyChanged();
        }

        private void OnPropertyChanged()
        {
            _isBackfacingOnScreen = IsBackfacingOnScreen();
            _barryCentricMatrixInverse = GetBarryCentricInverseMatrix();
        }

        private bool IsBackfacingOnScreen()
        {
            var AB = _b.ScreenPosition - _a.ScreenPosition;
            var AC = _c.ScreenPosition - _a.ScreenPosition;

            // Backface Culling: when Z > 0 = Clockwise, z < 0 Counter Clockwise
            var cross = Vector3.Cross(new Vector3(AB, 0), new Vector3(AC, 0));
            return cross.Z < 0;
        }

        private Matrix2x2 GetBarryCentricInverseMatrix()
        {
            var AB = _b.ScreenPosition - _a.ScreenPosition;
            var AC = _c.ScreenPosition - _a.ScreenPosition;

            var A = new Matrix2x2(AB.X, AB.Y, AC.X, AC.Y);
            return A.Inverse();
        }

        public (Vector3 color, float z) CalcColor(int x, int y, LightSource[] lightSources)
        {
            var p = new Vector2(x, y);
            var AP = p - _a.ScreenPosition;
            var vec = _barryCentricMatrixInverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                var color = Vector3.Zero;

                var interpolatedMaterial = _a.HomogenousColor + u * (_b.HomogenousColor - _a.HomogenousColor) + v * (_c.HomogenousColor - _a.HomogenousColor);
                var material = NormalizeByW(interpolatedMaterial);

                var interpolatedPosition = _a.HomogenousPosition + u * (_b.HomogenousPosition - _a.HomogenousPosition) + v * (_c.HomogenousPosition - _a.HomogenousPosition);
                var position = NormalizeByW(interpolatedPosition);

                for (int i = 0; i < lightSources.Length; i++)
                {
                    var lightSource = lightSources[i];

                    var lVec = lightSource.Center - position;
                    var lVecNorm = Vector3.Normalize(lVec);
                    var nVecNorm = _currentSurfaceNormal;

                    var light_cos = Vector3.Dot(nVecNorm, lVecNorm);

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
                        var sVec = (lVec - ((Vector3.Dot(lVec, nVecNorm)) * nVecNorm));
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

        private static Vector3 NormalizeByW(Vector4 v) => new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
    }
}
