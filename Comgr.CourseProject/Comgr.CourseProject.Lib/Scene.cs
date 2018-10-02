using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class Scene
    {
        private Vector3 _eyeVector;
        private Vector3 _lookAtVector;

        /// <summary>
        /// Field of View (Angle)
        /// </summary>
        private float _fieldOfView;

        private ICollection<Sphere> _sphereCollection;

        public Scene(Vector3 eye, Vector3 lookAt, float fieldOfView)
        {
            _eyeVector = eye;
            _lookAtVector = lookAt;
            _fieldOfView = fieldOfView;

            _sphereCollection = new List<Sphere>();
        }

        //public static readonly Vector3 Up = new Vector3(0, -2002, 0);
        public static readonly Vector3 Up = -Vector3.UnitY;

        public Vector3 Eye => _eyeVector;

        public Vector3 LookAt => _lookAtVector;

        public float FieldOfView => _fieldOfView;

        public ICollection<Sphere> Spheres => _sphereCollection;

        Vector3 fVectorNorm => Vector3.Normalize(LookAt - Eye);

        Vector3 rVectorNorm => Vector3.Normalize(Vector3.Cross(fVectorNorm, Up));

        Vector3 uVectorNorm => Vector3.Normalize(Vector3.Cross(rVectorNorm, fVectorNorm));

        public ImageSource GetImage(int width, int height, double dpiX, double dpiY)
        {
            var bitmap = new Bitmap(width, height, dpiX, dpiY);

            var divideX = width / (float)2;
            var alignX = 1 - divideX;

            var divideY = height / (float)2;
            var alignY = 1 - divideY;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    // Question: Why is it mirrored?

                    var x = ((width - i) + alignX) / divideX;
                    var y = (j + alignY) / divideY;
                    var c = GetColor(x, y);

                    bitmap.Set(i, j, c);
                }
            }

            return bitmap.GetImage();
        }

        public Color GetColor(float x, float y)
        {
            var pixel = new Vector2(x, y);
            var ray = CreateEyeRay(pixel);
            var hitPoint = FindClosestHitPoint(ray);

            if (hitPoint != null)
            {
                return hitPoint.Sphere.Color;
            }

            return Colors.Transparent;
        }

        private Ray CreateEyeRay(Vector2 pixel)
        {
            var tanFieldOfView = (float)Math.Tan(FieldOfView / 2);
            var directionVector = fVectorNorm + (pixel.X * rVectorNorm * tanFieldOfView) + (pixel.Y * uVectorNorm * tanFieldOfView);
            return new Ray(Eye, directionVector);
        }

        private HitPoint FindClosestHitPoint(Ray ray)
        {
            var hitPoints = new List<HitPoint>();

            foreach (var sphere in Spheres)
            {
                var hitPoint = FindHitpoint(ray, sphere);

                if (hitPoint != null)
                    hitPoints.Add(hitPoint);
            }

            return hitPoints.OrderBy(h => h.Ray.Lambda).FirstOrDefault();
        }

        private HitPoint FindHitpoint(Ray ray, Sphere sphere)
        {
            var ceVector = ray.Eye - sphere.Center;

            var b = 2 * Vector3.Dot(ceVector, Vector3.Normalize(ray.Direction));
            var c = Vector3.DistanceSquared(sphere.Center, ray.Eye) - (sphere.Radius * sphere.Radius);

            var b_squared = b * b;
            var four_a_c = 4 * c;

            if (b_squared >= four_a_c)
            {
                var sqrtExpr = Math.Sqrt(b_squared - four_a_c);

                var lambda1 = (-b + sqrtExpr) / 2;
                var lambda2 = (-b - sqrtExpr) / 2;

                var lambda = (float)Math.Min(Math.Max(lambda1, 0), Math.Max(lambda2, 0));

                if (lambda > 0)
                    return new HitPoint(new Ray(ray.Eye, lambda, Vector3.Normalize(ray.Direction)), sphere);
            }

            return null;
        }
    }
}
 