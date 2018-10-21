using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public enum BitmapTextureMode
    {
        PlanarProjection,
        SphericalProjection
    }

    public class BitmapTexture : ITexture
    {
        private Bitmap _bitmap;
        private BitmapTextureMode _mode;

        public BitmapTexture(Bitmap bitmap, BitmapTextureMode mode)
        {
            _bitmap = bitmap ?? throw new ArgumentNullException(nameof(bitmap));
            _mode = mode;
        }
        
        public BitmapTexture(string fileName, BitmapTextureMode mode)
            : this(new Bitmap(fileName), mode)
        {
        }

        public Vector3 CalcColor(Vector3 point)
        {
            var x = point.X;
            var y = point.Y;
            var z = point.Z;

            int s = 0;
            int t = 0;

            // Question: Something seems wrong in both modes?

            Color color = Color.Black;

            switch (_mode)
            {
                case BitmapTextureMode.PlanarProjection:
                    s = MapValue(-1f, 1f, 0f, _bitmap.Width - 1, x);
                    t = MapValue(-1f, 1f, 0f, _bitmap.Height - 1, y);
                    color = _bitmap.GetPixel(s, t);
                    break;
                case BitmapTextureMode.SphericalProjection:
                    if (x >= -1
                        && x <= 1
                        && y >= -1
                        && y <= 1
                        && z >= -1
                        && z <= 1)
                    {
                        s = MapValue((float)-Math.PI, (float)Math.PI, 0f, _bitmap.Width - 1, (float)Math.Atan2(x, z));
                        t = MapValue(0f, (float)Math.PI, 0f, _bitmap.Height - 1, (float)Math.Acos(y));

                        color = _bitmap.GetPixel(s, t);
                    }
                    break;
            }
            
            var rgb = new Vector3(color.R / (float)byte.MaxValue, color.G / (float)byte.MaxValue, color.B / (float)byte.MaxValue);
            return rgb;
        }

        private int MapValue(float sourceRangeMin, float sourceRangeMax, float targetRangeMin, float targetRangeMax, float sourceValue)
        {
            var sourceRange = sourceRangeMax - sourceRangeMin;
            var targetRange = targetRangeMax - targetRangeMin;

            var factor = targetRange / sourceRange;

            var sourceDiff = sourceValue - sourceRangeMin;      
            var targetDiff = sourceDiff * factor;

            var target = targetDiff + targetRangeMin;
            return (int)Math.Round(target);
        }
    }
}
