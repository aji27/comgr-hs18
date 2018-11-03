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
        private Vector3 _a;
        private Vector3 _b;
        private Vector3 _c;

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public Triangle2D TransformAndProject(Matrix4x4 transform, float width, float height)
        {
            var a2 = TransformAndProject(_a, transform, width, height);
            var b2 = TransformAndProject(_b, transform, width, height);
            var c2 = TransformAndProject(_c, transform, width, height);

            return new Triangle2D(a2, b2, c2);
        }

        private Vector2 TransformAndProject(Vector3 v, Matrix4x4 transform, float width, float height)
        {
            var v_homogenous = new Vector4(v, w: 1);
            var v_transformed = Vector4.Transform(v_homogenous, transform);

            var x = width * v_transformed.X / v_transformed.Z + width / 2;
            var y = width * v_transformed.Y / v_transformed.Z + height / 2;

            return new Vector2(x, y);
        }
    }
}
