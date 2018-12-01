using System;
using System.Drawing;
using System.Numerics;

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

        public Vector3 CalcColor(float s, float t, bool bilinearFiltering)
        {
            s = Clamp(0, _width - 1, s);
            t = Clamp(0, _height - 1, t);

            Color color = Color.Black;

            lock (_bitmap)
            {
                if (bilinearFiltering)
                {
                    var s_floor = (int)Math.Floor(s);
                    var t_floor = (int)Math.Floor(t);

                    if (s_floor < _width - 1
                        && t_floor < _height - 1)
                    {
                        var t_lerp = t - t_floor;
                        var s_lerp = s - s_floor;

                        var c1 = _bitmap.GetPixel(s_floor, t_floor);
                        var c2 = _bitmap.GetPixel(s_floor, t_floor + 1);
                        var c3 = _bitmap.GetPixel(s_floor + 1, t_floor);
                        var c4 = _bitmap.GetPixel(s_floor + 1, t_floor + 1);

                        var c_red = Lerp(c1, c2, t_lerp);
                        var c_green = Lerp(c3, c4, t_lerp);

                        var rgb = Vector3.Lerp(c_red, c_green, s_lerp);

                        return rgb;
                    }
                    else
                    {
                        // edge case
                        color = _bitmap.GetPixel((int)s, (int)t);
                    }
                }
                else
                {
                    color = _bitmap.GetPixel((int)s, (int)t);
                }
            }
                        
            return FromColor(color);
        }

        private static Vector3 FromColor(Color c) => new Vector3(c.R / (float)byte.MaxValue, c.G / (float)byte.MaxValue, c.B / (float)byte.MaxValue);

        private static float Clamp(float minValue, float maxValue, float value) => Math.Min(Math.Max(value, minValue), maxValue);

        private static Vector3 Lerp(Color c1, Color c2, float amount) => Vector3.Lerp(FromColor(c1), FromColor(c2), amount);
            
    }
}
