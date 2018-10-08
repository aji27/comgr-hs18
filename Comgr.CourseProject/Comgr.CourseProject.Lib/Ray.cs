using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class Ray
    {
        Vector3 _startVec;
        float _lambda;
        Vector3 _directionVec;

        public Ray(Vector3 startVec, Vector3 directionVec)
            : this(startVec, 1, directionVec)
        {
        }

        public Ray(Vector3 startVec, float lambda, Vector3 directionVec)
        {
            _startVec = startVec;
            _lambda = lambda;
            _directionVec = directionVec;
        }

        public Vector3 StartVec => _startVec;

        public float Lambda => _lambda;

        public Vector3 DirectionVec => _directionVec;
    }
}
