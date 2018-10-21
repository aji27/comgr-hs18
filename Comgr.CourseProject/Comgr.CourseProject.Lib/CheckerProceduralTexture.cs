using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class CheckerProceduralTexture : ITexture
    {
        private float _scale = 10;

        public Vector3 CalcColor(Vector3 point)
        {
            // Question: How do I achieve the same effect as showed in the slides?
            // Question: Do I have to implement 'Perlin noise' as an additional procedural texture?

            if (Frac(point.X * _scale) < 0.5
                && Frac(point.Y * _scale) < 0.5
                && Frac(point.Z * _scale) < 0.5)
            {
                return new Vector3(1, 0, 0);
            }
            else
                return new Vector3(0, 0, 0);
        }

        private static double Frac(double value) => value - Math.Truncate(value);
    }
}
