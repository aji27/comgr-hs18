using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class Vertex2D
    {
        public Vertex2D(Vertex origin, Vector4 homogenousPosition, Vector2 position)
        {
            Origin = origin;
            HomogenousPosition = homogenousPosition;
            Position = position;
        }
        
        public Vertex Origin { get; private set; }

        public Vector4 HomogenousPosition { get; private set; }

        public Vector2 Position { get; private set; }

        public Vector4 Color => new Vector4(Origin.Color / HomogenousPosition.W, 1 / HomogenousPosition.W);
    }
}
