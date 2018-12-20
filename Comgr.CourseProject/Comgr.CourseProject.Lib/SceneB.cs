using System;
using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class SceneB
    {
        int ZBUFFER_VISUALIZE_MIN = 0;
        int ZBUFFER_VISUALIZE_MAX = 10;

        private int _screenWidth;
        private int _screenHeight;

        private BitmapImage _bitmap;
        private Vector3[,] _rgbArray;
        private float[,] _zBufferArray;

        private Triangle[] _triangles;
        private LightSource[] _lightSources;

        private bool _gammaCorrect;
        private bool _visualizeZBuffer;

        private bool _useDeferredRenderer;

        public SceneB(int screenWidth, int screenHeight, double dpiX, double dpiY, Triangle[] triangles, LightSource[] lightSources, bool gammaCorrect, bool visualizeZBuffer, bool useDeferredRenderer)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _bitmap = new BitmapImage(_screenWidth, _screenHeight, dpiX, dpiY);
            _rgbArray = new Vector3[_screenWidth, _screenHeight];
            _zBufferArray = new float[_screenWidth, _screenHeight];
            
            _triangles = triangles;
            _lightSources = lightSources;

            _gammaCorrect = gammaCorrect;
            _visualizeZBuffer = visualizeZBuffer;

            _useDeferredRenderer = useDeferredRenderer;
        }

        public void ApplyTransform(Matrix4x4 matrix)
        {
            foreach (var triangle in _triangles)
                triangle.ApplyTransform(matrix);
        }

        public ImageSource GetImage()
        {
            //** clear buffers

            Array.Clear(_rgbArray, 0, _rgbArray.Length);

            for (int x = 0; x < _screenWidth; x++)
                for (int y = 0; y < _screenHeight; y++)
                    _zBufferArray[x, y] = float.PositiveInfinity;

            if (_useDeferredRenderer)
            {
                //** first pass (Z-Prepass)

                for (int i = 0; i < _triangles.Length; i++)
                {
                    var triangle = _triangles[i];

                    if (!triangle.IsBackfacing)
                    {
                        for (int x = (int)Math.Min(0, Math.Max(triangle.MinScreenX, 0)); x < (int)Math.Min(Math.Max(triangle.MaxScreenX, 0), _screenWidth); x++)
                        {
                            for (int y = (int)Math.Min(0, Math.Max(triangle.MinScreenY, 0)); y < (int)Math.Min(Math.Max(triangle.MaxScreenY, 0), _screenHeight); y++)
                            {
                                var z = triangle.CalcZ(x, y);

                                if (!float.IsInfinity(z)
                                    && !float.IsNaN(z)
                                    && z < _zBufferArray[x, y])
                                {
                                    _zBufferArray[x, y] = z;
                                }
                            }
                        }
                    }
                }

                //** second pass

                for (int i = 0; i < _triangles.Length; i++)
                {
                    var triangle = _triangles[i];

                    if (!triangle.IsBackfacing)
                    {
                        for (int x = (int)Math.Min(0, Math.Max(triangle.MinScreenX, 0)); x < (int)Math.Min(Math.Max(triangle.MaxScreenX, 0), _screenWidth); x++)
                        {
                            for (int y = (int)Math.Min(0, Math.Max(triangle.MinScreenY, 0)); y < (int)Math.Min(Math.Max(triangle.MaxScreenY, 0), _screenHeight); y++)
                            {
                                float z = _zBufferArray[x, y];

                                if (_visualizeZBuffer)
                                {
                                    var zcolor = (int)((z - ZBUFFER_VISUALIZE_MIN) / (ZBUFFER_VISUALIZE_MAX - ZBUFFER_VISUALIZE_MIN) * 255) * new Vector3(1f / 255, 1f / 255, 1f / 255);
                                    _rgbArray[x, y] = zcolor;
                                }
                                else
                                {
                                    var rgb = Vector3.Zero;
                                    if (triangle.CalcColorDeferred(x, y, _lightSources, z, out rgb))
                                        _rgbArray[x, y] = rgb;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _triangles.Length; i++)
                {
                    var triangle = _triangles[i];

                    if (!triangle.IsBackfacing)
                    {
                        for (int x = (int)Math.Min(0, Math.Max(triangle.MinScreenX, 0)); x < (int)Math.Min(Math.Max(triangle.MaxScreenX, 0), _screenWidth); x++)
                        {
                            for (int y = (int)Math.Min(0, Math.Max(triangle.MinScreenY, 0)); y < (int)Math.Min(Math.Max(triangle.MaxScreenY, 0), _screenHeight); y++)
                            {
                                float z;
                                Vector3 rgb;

                                if (triangle.CalcColor(x, y, _lightSources, out z, out rgb)
                                    && !float.IsInfinity(z)
                                    && !float.IsNaN(z))
                                {

                                    if (z < _zBufferArray[x, y])
                                    {
                                        _zBufferArray[x, y] = z;

                                        if (_visualizeZBuffer)
                                        {
                                            var zcolor = (int)((z - ZBUFFER_VISUALIZE_MIN) / (ZBUFFER_VISUALIZE_MAX - ZBUFFER_VISUALIZE_MIN) * 255) * new Vector3(1f / 255, 1f / 255, 1f / 255);
                                            _rgbArray[x, y] = zcolor;
                                        }
                                        else
                                        {
                                            _rgbArray[x, y] = rgb;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //** buffer to image

            for (int x = 0; x < _screenWidth; x++)
            {
                for (int y = 0; y < _screenHeight; y++)
                {
                    var rgb = _rgbArray[x, y];
                    var c = Conversions.FromRGB(rgb, _gammaCorrect);
                    _bitmap.Set(x, y, c);
                }
            }

            return _bitmap.GetImageSource();
        }
    }
}
 