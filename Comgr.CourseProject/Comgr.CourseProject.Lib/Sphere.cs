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
        private Vector3 _rgb;
        private ITexture _texture;
        private bool _isWall;
        private float _brightness;
        private float _reflectiveness;

        public Sphere(string name, Vector3 center, float radius, Color color, ITexture texture = null, bool isWall = false, float brightness = 1f, float reflectiveness = 0.2f)
        {
            _name = name;
            _centerVector = center;
            _radius = radius;
            _color = color;
            _brightness = brightness;
            _rgb = Conversions.FromColor(_color);
            _texture = texture;
            _isWall = isWall;
            _reflectiveness = reflectiveness;
        }

        public string Name => _name;

        public Vector3 Center => _centerVector;

        public float Radius => _radius;

        public Color Color => _color;

        public ITexture Texture => _texture;

        public bool IsWall => _isWall;

        public float Brightness => _brightness;

        public float Reflectiveness => _reflectiveness;

        public Vector3 CalcColor(Vector3 point)
        {
            if (Texture == null)
            {
                return _rgb;
            }
            else
            {
                return Texture.CalcColor(point);
            }
        }
    }
}
