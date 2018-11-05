using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Vertex
    {
        public Vertex(Vector3 position)
            : this(position, new Vector3(1, 0, 0) /* red */)
        {
        }

        public Vertex(Vector3 position, Vector3 color)
        {
            Position = position;
            Color = color;
        }

        public Vector3 Position { get; private set; }

        public Vector3 Color { get; private set; }
    }
}
