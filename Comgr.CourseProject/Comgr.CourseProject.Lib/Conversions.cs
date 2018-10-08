using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public static class Conversions
    {
        public static Color FromRGB(float r, float g, float b, bool gammaCorrection = true) => FromRGB(new Vector3(r, g, b), gammaCorrection);

        public static Color FromRGB(Vector3 rgb, bool gammaCorrection = true)
        {
            var r = rgb.X;
            var g = rgb.Y;
            var b = rgb.Z;

            if (gammaCorrection)
            {
                r = GammaCorrect(r);
                g = GammaCorrect(g);
                b = GammaCorrect(b);
            }

            return Color.FromScRgb(1.0f, r, g, b);
        }

        // According to Lecture Slides from Week 1
        public static float GammaCorrect(float f)
        {
            if (f <= 0.0031308f)
            {
                return 12.92f * f;
            }
            else
            {
                return (float)((1.055d * Math.Pow(f, 1 / 2.4)) - 0.055d);
            }
        }

        public static Color GammaCorrect(Color c) => Color.FromScRgb(GammaCorrect(c.ScA), GammaCorrect(c.ScR), GammaCorrect(c.ScG), GammaCorrect(c.ScB));

        public static Vector3 FromColor(Color color) => new Vector3(color.ScR, color.ScG, color.ScB);
    }
}
