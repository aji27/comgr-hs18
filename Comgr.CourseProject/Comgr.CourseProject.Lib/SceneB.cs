﻿using System;
using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class SceneB
    {
        private int _screenWidth;
        private int _screenHeight;

        private BitmapImage _bitmap;
        private Vector3[,] _rgbArray;
        private float[,] _zBufferArray;

        private Triangle[] _triangles;
        private LightSource[] _lightSources;

        public SceneB(int screenWidth, int screenHeight, double dpiX, double dpiY, Triangle[] triangles, LightSource[] lightSources)
        {
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _bitmap = new BitmapImage(_screenWidth, _screenHeight, dpiX, dpiY);
            _rgbArray = new Vector3[_screenWidth, _screenHeight];
            _zBufferArray = new float[_screenWidth, _screenHeight];
            
            _triangles = triangles;
            _lightSources = lightSources;
        }

        public void ApplyTransform(Matrix4x4 matrix)
        {
            foreach (var triangle in _triangles)
                triangle.ApplyTransform(matrix);
        }

        public ImageSource GetImage()
        {
            Array.Clear(_rgbArray, 0, _rgbArray.Length);

            for (int x = 0; x < _screenWidth; x++)
                for (int y = 0; y < _screenHeight; y++)
                    _zBufferArray[x, y] = float.PositiveInfinity;

            for (int i = 0; i < _triangles.Length; i++)
            {
                var triangle = _triangles[i];

                if (!triangle.IsBackfacing)
                {
                    for (int x = (int)Math.Min(0, Math.Max(triangle.MinScreenX, 0)); x < (int)Math.Min(Math.Max(triangle.MaxScreenX, 0), _screenWidth); x++)
                    {
                        for (int y = (int)Math.Min(0, Math.Max(triangle.MinScreenY, 0)); y < (int)Math.Min(Math.Max(triangle.MaxScreenY, 0), _screenHeight); y++)
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

            for (int x = 0; x < _screenWidth; x++)
            {
                for (int y = 0; y < _screenHeight; y++)
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
 