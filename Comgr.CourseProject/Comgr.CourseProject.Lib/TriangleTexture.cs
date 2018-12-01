using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class TriangleTexture
    {
        private Bitmap _bitmap;

        private int _width;
        private int _height;

        public TriangleTexture(Bitmap bitmap)
        {
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));

            _width = _bitmap.Width;
            _height = _bitmap.Height;
        }

        public TriangleTexture(string fileName)
            : this(new Bitmap(fileName))
        {
        }

        public Vector3 CalcColor(float s, float t)
        {
            var x = Clamp(0, _width - 1, s);
            var y = Clamp(0, _height - 1, t);

            Color color = Color.Black;

            lock (_bitmap)
                color = _bitmap.GetPixel((int)x, (int)y);

            var rgb = new Vector3(color.R / (float)byte.MaxValue, color.G / (float)byte.MaxValue, color.B / (float)byte.MaxValue);
            return rgb;
        }

        private static float Clamp(float minValue, float maxValue, float value) => Math.Min(Math.Max(value, minValue), maxValue);
    }
}
