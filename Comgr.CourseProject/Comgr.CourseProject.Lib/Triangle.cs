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

        private TriangleOptions _options;

        public Triangle(Vertex a, Vertex b, Vertex c, TriangleOptions options)
        {
            _a = a;
            _b = b;
            _c = c;

            // Calculate Normal
            var ab = _b.HomogenousPosition.HomogenousNormalize() - _a.HomogenousPosition.HomogenousNormalize();
            var ac = _c.HomogenousPosition.HomogenousNormalize() - _a.HomogenousPosition.HomogenousNormalize();
            _startSurfaceNormal = -Vector3.Normalize(Vector3.Cross(ab, ac));
            _currentSurfaceNormal = _startSurfaceNormal;

            _a.Normal = _currentSurfaceNormal;
            _b.Normal = _currentSurfaceNormal;
            _c.Normal = _currentSurfaceNormal;

            _options = options;

            OnPropertyChanged();
        }

        public bool IsBackfacing => _options.PerformBackfaceCulling && _isBackfacingOnScreen;

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

        public float CalcZ(int x, int y)
        {
            var p = new Vector2(x, y);
            var AP = p - _a.ScreenPosition;
            var vec = _barryCentricMatrixInverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                var interpolatedPosition = _a.HomogenousPosition + u * (_b.HomogenousPosition - _a.HomogenousPosition) + v * (_c.HomogenousPosition - _a.HomogenousPosition);
                var position = interpolatedPosition.HomogenousNormalize();

                return position.Z;
            }

            return float.PositiveInfinity;
        }

        public bool CalcColor(int x, int y, LightSource[] lightSources, out float z, out Vector3 rgb)
        {
            z = 0f;
            rgb = Vector3.Zero;

            var p = new Vector2(x, y);
            var AP = p - _a.ScreenPosition;
            var vec = _barryCentricMatrixInverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                var interpolatedPosition = _a.HomogenousPosition + u * (_b.HomogenousPosition - _a.HomogenousPosition) + v * (_c.HomogenousPosition - _a.HomogenousPosition);
                var position = interpolatedPosition.HomogenousNormalize();

                z = position.Z;

                var material = Vector3.Zero;

                if (_options.SingleColor)
                {
                    material = new Vector3(1, 0, 0); // red
                }
                else
                {

                    if (_options.Texture == null)
                    {
                        var interpolatedMaterial = _a.HomogenousColor + u * (_b.HomogenousColor - _a.HomogenousColor) + v * (_c.HomogenousColor - _a.HomogenousColor);
                        material = interpolatedMaterial.HomogenousNormalize();
                    }
                    else
                    {
                        var interpolatedTexture = _a.HomogenousTexturePosition + u * (_b.HomogenousTexturePosition - _a.HomogenousTexturePosition) + v * (_c.HomogenousTexturePosition - _a.HomogenousTexturePosition);
                        var texturePosition = interpolatedTexture.HomogenousNormalize();
                        material = _options.Texture.CalcColor(texturePosition.X, texturePosition.Y, _options.BilinearFilter, _options.GammaCorrect);
                    }
                }

                var interpolatedNormal = Vector3.Normalize(_a.Normal + u * (_b.Normal - _a.Normal) + v * (_c.Normal - _a.Normal));

                for (int i = 0; i < lightSources.Length; i++)
                {
                    var lightSource = lightSources[i];

                    var lVec = lightSource.Center - position;

                    // Gibt dasselbe für lVec:
                    // lVec = (new Vector4(lightSource.Center, w: 1) - interpolatedPosition).HomogenousNormalize();

                    var lVecNorm = Vector3.Normalize(lVec);
                    //var nVecNorm = _currentSurfaceNormal;
                    var nVecNorm = interpolatedNormal;

                    var light = Conversions.FromColor(lightSource.Color);

                    // Diffuse Lambert
                    var dot_diffuse = Vector3.Dot(nVecNorm, lVecNorm);

                    if (_options.DiffuseLambert
                        && dot_diffuse > 0)
                    {
                        var diffuse = Vector3.Multiply(light, material) * dot_diffuse;
                        rgb += diffuse;
                    }

                    // Specular Phong
                    var eye = new Vector3(0, 0, 0);
                    var rayVecNorm = Vector3.Normalize(position - eye);

                    var sVec = (lVec - ((Vector3.Dot(lVec, nVecNorm)) * nVecNorm));
                    var rVec = lVec - (2 * sVec);
                    var dot_phong = -Vector3.Dot(Vector3.Normalize(rVec), rayVecNorm);

                    if (_options.SpecularPhong
                        && dot_phong > 0)
                    {
                        var specular = light * (float)Math.Pow(dot_phong, _options.SpecularPhongFactor);
                        rgb += specular;
                    }
                }

                return true;
            }

            return false;
        }

        public bool CalcColorDeferred(int x, int y, LightSource[] lightSources, float z, out Vector3 rgb)
        {
            rgb = Vector3.Zero;

            var p = new Vector2(x, y);
            var AP = p - _a.ScreenPosition;
            var vec = _barryCentricMatrixInverse * AP;
            var u = vec.X;
            var v = vec.Y;
            bool drawPoint = (u >= 0 && v >= 0 && (u + v) < 1);

            if (drawPoint)
            {
                var interpolatedPosition = _a.HomogenousPosition + u * (_b.HomogenousPosition - _a.HomogenousPosition) + v * (_c.HomogenousPosition - _a.HomogenousPosition);
                var position = interpolatedPosition.HomogenousNormalize();

                if (position.Z == z)
                {
                    var material = Vector3.Zero;

                    if (_options.SingleColor)
                    {
                        material = new Vector3(1, 0, 0); // red
                    }
                    else
                    {

                        if (_options.Texture == null)
                        {
                            var interpolatedMaterial = _a.HomogenousColor + u * (_b.HomogenousColor - _a.HomogenousColor) + v * (_c.HomogenousColor - _a.HomogenousColor);
                            material = interpolatedMaterial.HomogenousNormalize();
                        }
                        else
                        {
                            var interpolatedTexture = _a.HomogenousTexturePosition + u * (_b.HomogenousTexturePosition - _a.HomogenousTexturePosition) + v * (_c.HomogenousTexturePosition - _a.HomogenousTexturePosition);
                            var texturePosition = interpolatedTexture.HomogenousNormalize();
                            material = _options.Texture.CalcColor(texturePosition.X, texturePosition.Y, _options.BilinearFilter, _options.GammaCorrect);
                        }
                    }

                    var interpolatedNormal = Vector3.Normalize(_a.Normal + u * (_b.Normal - _a.Normal) + v * (_c.Normal - _a.Normal));

                    for (int i = 0; i < lightSources.Length; i++)
                    {
                        var lightSource = lightSources[i];

                        var lVec = lightSource.Center - position;

                        // Gibt dasselbe für lVec:
                        // lVec = (new Vector4(lightSource.Center, w: 1) - interpolatedPosition).HomogenousNormalize();
                                                
                        var lVecNorm = Vector3.Normalize(lVec);
                        //var nVecNorm = _currentSurfaceNormal;
                        var nVecNorm = interpolatedNormal;

                        var light = Conversions.FromColor(lightSource.Color);

                        // Diffuse Lambert
                        var dot_diffuse = Vector3.Dot(nVecNorm, lVecNorm);

                        if (_options.DiffuseLambert
                            && dot_diffuse > 0)
                        {
                            var diffuse = Vector3.Multiply(light, material) * dot_diffuse;
                            rgb += diffuse;
                        }

                        // Specular Phong
                        var eye = new Vector3(0, 0, 0);
                        var rayVecNorm = Vector3.Normalize(position - eye);

                        var sVec = (lVec - ((Vector3.Dot(lVec, nVecNorm)) * nVecNorm));
                        var rVec = lVec - (2 * sVec);
                        var dot_phong = -Vector3.Dot(Vector3.Normalize(rVec), rayVecNorm);

                        if (_options.SpecularPhong
                            && dot_phong > 0)
                        {
                            var specular = light * (float)Math.Pow(dot_phong, _options.SpecularPhongFactor);
                            rgb += specular;
                        }
                    }

                    return true;
                }
            }

            return false;
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
