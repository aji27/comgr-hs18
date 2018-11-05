using System;
using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class SceneB
    {
        private int _width;
        private int _height;

        private BitmapImage _bitmap;
        private Vector3[,] _rgbArray;
        private float[,] _zBufferArray;

        private Triangle[] _triangles;
        private LightSource[] _lightSources;

        public SceneB(int width, int height, double dpiX, double dpiY, Triangle[] triangles, LightSource[] lightSources)
        {
            _width = width;
            _height = height;
            _bitmap = new BitmapImage(_width, _height, dpiX, dpiY);
            _rgbArray = new Vector3[_width, _height];
            _zBufferArray = new float[_width, _height];
            
            _triangles = triangles;
            _lightSources = lightSources;
        }

        public Matrix4x4 Transformation { get; set; } = Matrix4x4.Identity;

        public ImageSource GetImage()
        {
            Array.Clear(_rgbArray, 0, _rgbArray.Length);

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height; y++)
                    _zBufferArray[x, y] = float.PositiveInfinity;

            for (int i = 0; i < _triangles.Length; i++)
            {
                var triangle = _triangles[i];
                triangle.ApplyTransform(Transformation);

                if (!triangle.IsBackfacing)
                {
                    for (int x = (int)triangle.MinScreenX; x <= (int)triangle.MaxScreenX; x++)
                    {
                        for (int y = (int)triangle.MinScreenY; y <= (int)triangle.MaxScreenY; y++)
                        {
                            (Vector3 color, float z) = triangle.CalcColor(x, y, _lightSources);

                            if (!float.IsInfinity(z)
                                && !float.IsNaN(z))
                            {
                                // visualize z-buffer

                                // int Z_MIN = 0;
                                // int Z_MAX = 10;
                                // var zcolor = (int)((z - Z_MIN) / (Z_MAX - Z_MIN) * 255) * new Vector3(1f / 255, 1f / 255, 1f / 255);
                                // _rgbArray[x, y] = zcolor;

                                if (z < _zBufferArray[x, y])
                                {
                                    _rgbArray[x, y] = color;
                                    _zBufferArray[x, y] = z;
                                }
                            }
                        }
                    }
                }
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var rgb = _rgbArray[x, y];
                    var c = Conversions.FromRGB(rgb, gammaCorrection: true);
                    _bitmap.Set(x, y, c);
                }
            }

            return _bitmap.GetImageSource();
        }
    }
}
 