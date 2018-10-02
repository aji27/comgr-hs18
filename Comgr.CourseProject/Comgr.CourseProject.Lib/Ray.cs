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
        Vector3 _eyeVector; // Startpunkt

        float _lambda;
        Vector3 _directionVector; // Richtung

        public Ray(Vector3 eye, Vector3 direction)
            : this(eye, 1, direction)
        {
        }

        public Ray(Vector3 eye, float lambda, Vector3 direction)
        {
            _eyeVector = eye;
            _lambda = lambda;
            _directionVector = direction;
        }

        public Vector3 Eye => _eyeVector;

        public float Lambda => _lambda;

        public Vector3 Direction => _directionVector;
    }
}
