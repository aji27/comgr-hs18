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

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            _a = a;
            _b = b;
            _c = c;

            // Calculate Normal
            var ab = _b.HomogenousPosition.NormalizeByW() - _a.HomogenousPosition.NormalizeByW();
            var ac = _c.HomogenousPosition.NormalizeByW() - _a.HomogenousPosition.NormalizeByW();
            _startSurfaceNormal = -Vector3.Normalize(Vector3.Cross(ab, ac));

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

            // Gibt dasselbe für _currentSurfaceNormal:

            //if (!Matrix4x4.Invert(matrix, out var inverse))
            //    throw new ArgumentException("Could not invert given matrix.");

            //_currentSurfaceNormal = Vector3.Normalize(NormalizeByW(Vector4.Transform(new Vector4(_startSurfaceNormal, w: 0), Matrix4x4.Transpose(inverse))));

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
                var material = interpolatedMaterial.NormalizeByW();

                var interpolatedPosition = _a.HomogenousPosition + u * (_b.HomogenousPosition - _a.HomogenousPosition) + v * (_c.HomogenousPosition - _a.HomogenousPosition);
                var position = interpolatedPosition.NormalizeByW();

                for (int i = 0; i < lightSources.Length; i++)
                {
                    var lightSource = lightSources[i];

                    var lVec = lightSource.Center - position;
                    
                    // Gibt dasselbe für lVec:
                    // lVec = (new Vector4(lightSource.Center, w: 1) - interpolatedPosition).NormalizeByW();

                    var lVecNorm = Vector3.Normalize(lVec);
                    var nVecNorm = _currentSurfaceNormal;

                    var light = Conversions.FromColor(lightSource.Color);

                    // Diffuse Lambert
                    var dot_diffuse = Vector3.Dot(nVecNorm, lVecNorm);

                    if (dot_diffuse > 0)
                    {
                        var diffuse = Vector3.Multiply(light, material) * dot_diffuse;
                        color += diffuse;
                    }
                    
                    // Specular Phong
                    var eye = new Vector3(0, 0, 0);
                    var rayVecNorm = Vector3.Normalize(position - eye);

                    var specularPhongFactor = 1000;

                    // var sVec = (lVec - ((Vector3.Dot(lVec, nVecNorm)) * nVecNorm));
                    // var rVec = lVec - (2 * sVec);
                    // var dot_phong = Vector3.Dot(Vector3.Normalize(rVec), rayVecNorm);

                    // Keine Ahnung weshalb bei Verwendung des "rVec" (siehe SceneA.cs, Zeile 358) es "komisch" aussieht.

                    var dot_phong = Vector3.Dot(Vector3.Normalize(-lVec), rayVecNorm);                                       

                    if (dot_phong > 0)
                    {
                        var specular = light * (float)Math.Pow(dot_phong, specularPhongFactor);
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
