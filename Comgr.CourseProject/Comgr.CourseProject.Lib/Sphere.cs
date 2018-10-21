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
        private ITexture _texture;

        public Sphere(string name, Vector3 center, float radius, Color color, ITexture texture = null)
        {
            _name = name;
            _centerVector = center;
            _radius = radius;
            _color = color;
            _texture = texture;
        }

        public string Name => _name;

        public Vector3 Center => _centerVector;

        public float Radius => _radius;

        public Color Color => _color;

        public ITexture Texture => _texture;

        public Vector3 CalcColor(Vector3 point)
        {
            if (Texture == null)
                return Conversions.FromColor(Color);
            else
            {
                return Texture.CalcColor(point);
            }
        }

    }
}
