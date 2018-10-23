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
            if ((AbsFrac(point.X * _scale) < 0.5)
                ^ (AbsFrac(point.Y * _scale) < 0.5)
                ^ (AbsFrac(point.Z * _scale) < 0.5))
            {
                return new Vector3(1, 0, 0);
            }
            else
                return new Vector3(0, 0, 0);
        }

        private static double AbsFrac(double value) => Math.Abs(value - Math.Truncate(value));
    }
}
