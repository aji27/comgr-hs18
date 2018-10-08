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
        private ICollection<LightSource> _lightSourceCollection;

        private readonly bool _gammaCorrect;

        public Scene(Vector3 eye, Vector3 lookAt, float fieldOfView, bool gammaCorrect = true)
        {
            _eyeVector = eye;
            _lookAtVector = lookAt;
            _fieldOfView = fieldOfView;

            _sphereCollection = new List<Sphere>();
            _lightSourceCollection = new List<LightSource>();

            _gammaCorrect = gammaCorrect;
        }

        //public static readonly Vector3 Up = new Vector3(0, -2002, 0);
        public static readonly Vector3 Up = -Vector3.UnitY;

        public Vector3 Eye => _eyeVector;

        public Vector3 LookAt => _lookAtVector;

        public float FieldOfView => _fieldOfView;

        public ICollection<Sphere> Spheres => _sphereCollection;

        public ICollection<LightSource> LightSources => _lightSourceCollection;

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

                    var x = (i + alignX) / divideX;
                    var y = ((height - j) + alignY) / divideY;
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
            var rgb = CalcColor(ray);

            return Conversions.FromRGB(rgb, _gammaCorrect);
        }

        private Vector3 CalcColor(Ray ray, int reflectionLimit = 1)
        {
            var rgb = Vector3.Zero;

            var hitPoint = FindClosestHitPoint(ray);

            if (hitPoint != null)
            {
                var walls = new[] { "a", "b", "c", "d", "e" };
                var isWall = walls.Contains(hitPoint.Sphere.Name);

                var material = Conversions.FromColor(hitPoint.Sphere.Color);

                var rayVecNorm = Vector3.Normalize(hitPoint.Ray.DirectionVec);
                var rayVec = (hitPoint.Ray.Lambda * rayVecNorm) * 0.9999f /* nudging? */;
                var hitPointVec = hitPoint.Ray.StartVec + rayVec;
                var nVecNorm = Vector3.Normalize(hitPointVec - hitPoint.Sphere.Center);

                // Reflection
                if (!isWall /* ignore walls */
                    && reflectionLimit > 0)
                {
                    var reflectionMaterial = Conversions.FromColor(Colors.White);

                    var refVecNorm = Vector3.Normalize(rayVecNorm - 2 * Vector3.Dot(nVecNorm, rayVecNorm) * nVecNorm);
                    var refVec2 = refVecNorm + (Vector3.One - refVecNorm) * (float)Math.Pow((1 - Vector3.Dot(nVecNorm, refVecNorm)), 5);
                    var reflectionRay = new Ray(hitPointVec, refVec2);
                    var reflectionColor = CalcColor(reflectionRay, --reflectionLimit);
                    var reflection = Vector3.Multiply(reflectionColor, reflectionMaterial) * 0.25f;
                    rgb += reflection;
                }

                foreach (var lightSource in LightSources)
                {
                    var lVec = lightSource.Center - hitPointVec;
                    var lVecNorm = Vector3.Normalize(lVec);

                    var light_cos = Vector3.Dot(nVecNorm, lVecNorm);

                    if (light_cos >= 0)
                    {
                        var light = Conversions.FromColor(lightSource.Color);

                        // Shadows 
                        if (HasObjectInFrontOfLightSource(hitPointVec, lightSource))
                        {
                            // Question: What is meant by "contribute nothing if it is occluded"?
                            //light = Conversions.FromColor(Color.FromRgb(32, 32, 32));
                            //light = Conversions.FromColor(Colors.Gray);
                            light_cos = 0.05f;
                        }

                        // Diffuse "Lambert" 
                        var diffuse = Vector3.Multiply(light, material) * light_cos;
                        rgb += diffuse;

                        // Ignore walls
                        if (!isWall)
                        {
                            // Specular "Phong"
                            var k = 10;
                            var sVec = (lVec - ((Vector3.Dot(lVec, nVecNorm)) * nVecNorm));
                            var rVec = lVec - (2 * sVec);
                            var specular = light * (float)Math.Pow((Vector3.Dot(Vector3.Normalize(rVec), rayVecNorm)), k);
                            rgb += specular;
                        }
                    }
                }                
            }

            return rgb;
        }

        private Ray CreateEyeRay(Vector2 pixel)
        {
            var fieldOfViewInRadians = (Math.PI / 180) * (FieldOfView / 2);
            var tanOfFieldOfView = (float)Math.Tan(fieldOfViewInRadians);
            var directionVector = fVectorNorm + (pixel.X * rVectorNorm * tanOfFieldOfView) + (pixel.Y * uVectorNorm * tanOfFieldOfView);
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

        private bool HasObjectInFrontOfLightSource(Vector3 hitPointVec, LightSource lightSource)
        {
            var lVec = lightSource.Center - hitPointVec;
            var ray = new Ray(hitPointVec, lVec);
            var length = Vector3.DistanceSquared(ray.StartVec, lVec);

            foreach (var sphere in Spheres)
            {
                var hitPoint = FindHitpoint(ray, sphere);

                if (hitPoint != null)
                {
                    var rayVec = hitPoint.Ray.Lambda * hitPoint.Ray.DirectionVec;
                    var scale = Vector3.DistanceSquared(hitPoint.Ray.StartVec, rayVec) / length;

                    if (scale <= 1)
                        return true;
                }
            }

            return false;
        }

        private HitPoint FindHitpoint(Ray ray, Sphere sphere)
        {
            var ceVec = ray.StartVec - sphere.Center;

            var b = 2 * Vector3.Dot(ceVec, Vector3.Normalize(ray.DirectionVec));
            var c = Vector3.Dot(ceVec, ceVec) - (sphere.Radius * sphere.Radius);

            var b_squared = b * b;
            var four_a_c = 4 * c;

            if (b_squared >= four_a_c)
            {
                var sqrtExpr = Math.Sqrt(b_squared - four_a_c);

                var lambda1 = (-b + sqrtExpr) / 2;
                var lambda2 = (-b - sqrtExpr) / 2;

                var lambda = (float)Math.Min(Math.Max(lambda1, 0), Math.Max(lambda2, 0));

                if (lambda > 0)
                    return new HitPoint(new Ray(ray.StartVec, lambda, Vector3.Normalize(ray.DirectionVec)), sphere);
            }

            return null;
        }
    }
}
 