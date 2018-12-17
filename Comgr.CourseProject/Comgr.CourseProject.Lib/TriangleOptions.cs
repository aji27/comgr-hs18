using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class TriangleOptions
    {
        public bool DiffuseLambert { get; set; }

        public bool SpecularPhong { get; set; }

        public float SpecularPhongFactor { get; set; }

        public TriangleTexture Texture { get; set; }

        public bool BilinearFilter { get; set; }

        public bool GammaCorrect { get; set; }
    }
}
