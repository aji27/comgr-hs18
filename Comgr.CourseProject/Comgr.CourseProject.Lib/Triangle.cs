using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Comgr.CourseProject.Lib
{
    public class Triangle
    {
        private Vertex _a;
        private Vertex _b;
        private Vertex _c;

        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public Triangle2D TransformAndProject(Matrix4x4 transform, float width, float height)
        {
            var a_2D = TransformAndProject(_a, transform, width, height);
            var b_2D = TransformAndProject(_b, transform, width, height);
            var c_2D = TransformAndProject(_c, transform, width, height);

            return new Triangle2D(a_2D, b_2D, c_2D);
        }

        private Vertex2D TransformAndProject(Vertex v, Matrix4x4 transform, float width, float height)
        {
            var v_homogenous = new Vector4(v.Position, w: 1);
            var v_transformed = Vector4.Transform(v_homogenous, transform);

            var x = width * v_transformed.X / v_transformed.Z + width / 2;
            var y = width * v_transformed.Y / v_transformed.Z + height / 2;

            var p = new Vector2(x, y);

            return new Vertex2D(v, v_transformed, p);
        }
    }
}
