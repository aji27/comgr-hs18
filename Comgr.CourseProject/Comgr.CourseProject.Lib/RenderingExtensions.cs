using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public static class RenderingExtensions
    {
        public static Vector3 NormalizeByW(this Vector4 v)
        {
            if (v.W != 1f && v.W != 0f)
                return new Vector3(v.X / v.W, v.Y / v.W, v.Z / v.W);
            else
                return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
