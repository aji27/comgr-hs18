using System;
using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class SceneB
    {
        private Triangle[] _triangles;

        private int _width;
        private int _height;

        private BitmapImage _bitmap;
        private Vector3[,] _rgbArray;
        private float[,] _zBufferArray;

        private const int Z_MIN = 0;
        private const int Z_MAX = 10;

        public SceneB(Triangle[] triangles, int width, int height, double dpiX, double dpiY)
        {
            _triangles = triangles;
            _width = width;
            _height = height;
            _bitmap = new BitmapImage(_width, _height, dpiX, dpiY);
            _rgbArray = new Vector3[_width, _height];
            _zBufferArray = new float[_width, _height];
        }

        public Matrix4x4 Transformation { get; set; } = Matrix4x4.Identity;

        public ImageSource GetImage()
        {
            Array.Clear(_rgbArray, 0, _rgbArray.Length);
            Array.Clear(_zBufferArray, 0, _zBufferArray.Length);

            for (int i = 0; i < _triangles.Length; i++)
            {
                var triangle = _triangles[i];
                var triangle2D = triangle.TransformAndProject(Transformation, _width, _height);
                
                if (!triangle2D.IsBackface)
                {
                    for (int x = (int)triangle2D.MinX; x <= (int)triangle2D.MaxX; x++)
                    {
                        for (int y = (int)triangle2D.MinY; y <= (int)triangle2D.MaxY; y++)
                        {
                            (Vector3 color, float z) = triangle2D.CalcColor(x, y);

                            var zcolor = (int)((z - Z_MIN) / (Z_MAX - Z_MIN) * 255) * new Vector3(1f / 255, 1f / 255, 1f / 255);

                            _rgbArray[x, y] += zcolor;

                            //_rgbArray[x, y] += triangle2D.CalcColor(x, y);
                        }
                    }
                }
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var rgb = _rgbArray[x, y];
                    var c = Conversions.FromRGB(rgb, gammaCorrection: false);
                    _bitmap.Set(x, y, c);
                }
            }

            return _bitmap.GetImageSource();
        }
    }
}
 