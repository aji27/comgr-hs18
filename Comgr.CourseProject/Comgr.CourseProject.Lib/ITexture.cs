using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public interface ITexture
    {
        Vector3 CalcColor(Vector3 point);
    }
}
