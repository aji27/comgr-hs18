using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class HitPoint
    {
        private Ray _ray;
        private Sphere _spehre;

        public HitPoint(Ray ray, Sphere sphere)
        {
            _ray = ray;
            _spehre = sphere;
        }

        public Ray Ray => _ray;

        public Sphere Sphere => _spehre;
    }
}
