using System.Numerics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Sphere
    {
        private string _name;
        private Vector3 _centerVector;
        private float _radius;
        private Color _color;

        public Sphere(string name, Vector3 center, float radius, Color color)
        {
            _name = name;
            _centerVector = center;
            _radius = radius;
            _color = color;
        }

        public string Name => _name;

        public Vector3 Center => _centerVector;

        public float Radius => _radius;

        public Color Color => _color;
        
    }
}
