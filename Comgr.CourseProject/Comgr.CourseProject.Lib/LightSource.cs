using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class LightSource
    {
        private string _name;
        private Vector3 _centerVector;
        private Color _color;

        public LightSource(string name, Vector3 center, Color color)
        {
            _name = name;
            _centerVector = center;
            _color = color;
        }

        public string Name => _name;

        public Vector3 Center => _centerVector;

        public float Radius => 0.2f;

        public Color Color => _color;
    }
}
