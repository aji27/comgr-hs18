using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
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

        private BVHNode _accelerationStructure;

        public Scene(Vector3 eye, Vector3 lookAt, float fieldOfView)
        {
            _eyeVector = eye;
            _lookAtVector = lookAt;
            _fieldOfView = fieldOfView;

            _sphereCollection = new List<Sphere>();
            _lightSourceCollection = new List<LightSource>();
        }

        public bool Parallelize { get; set; } = true;

        public bool GammaCorrect { get; set; } = true;

        public bool DiffuseLambert { get; set; } = true;

        public bool SpecularPhong { get; set; } = true;

        public bool Reflection { get; set; } = false;

        public bool Shadows { get; set; } = true;

        public bool AccelerationStructure { get; set; } = true;

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
            var sw = Stopwatch.StartNew();

            if (AccelerationStructure
                && _accelerationStructure == null)
            {
                _accelerationStructure = BVHNode.BuildTopDown(_sphereCollection);
            }

            var bitmap = new BitmapImage(width, height, dpiX, dpiY);

            var divideX = width / (float)2;
            var alignX = 1 - divideX;

            var divideY = height / (float)2;
            var alignY = 1 - divideY;

            int workDone = 0;
            int totalWork = width * height;

            if (Parallelize)
            {
                Parallel.For(0, width, i =>
                {
                    Parallel.For(0, height, j =>
                    {
                        var x = (i + alignX) / divideX;
                        var y = ((height - j) + alignY) / divideY;
                        var c = GetColor(x, y);

                        bitmap.Set(i, j, c);

                        Interlocked.Increment(ref workDone);

                        if ((int)sw.Elapsed.TotalMilliseconds % 1000 == 0)
                            Debug.WriteLine($"{((float)workDone / totalWork * 100):F2}% progress. Running time {sw.Elapsed}.");
                    });
                });
            }
            else
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        // Question: Why is it mirrored?

                        var x = (i + alignX) / divideX;
                        var y = ((height - j) + alignY) / divideY;
                        var c = GetColor(x, y);

                        bitmap.Set(i, j, c);

                        ++workDone;

                        if ((int)sw.Elapsed.TotalMilliseconds % 1000 == 0)
                            Debug.WriteLine($"{((float)workDone / totalWork * 100):F2}% progress. Running time {sw.Elapsed}.");
                    }
                }
            }

            var imageSource = bitmap.GetImageSource();

            sw.Stop();

            Debug.WriteLine($"Image generated in {sw.Elapsed}.");

            return imageSource;
        }
                
        public Color GetColor(float x, float y)
        {
            var pixel = new Vector2(x, y);
            var ray = CreateEyeRay(pixel);
            var rgb = CalcColor(ray);

            return Conversions.FromRGB(rgb, GammaCorrect);
        }

        private Vector3 CalcColor(Ray ray, int reflectionLimit = 1)
        {
            var rgb = Vector3.Zero;

            var hitPoint = FindClosestHitPoint(ray);

            if (hitPoint != null)
            {
                var walls = new[] { "a", "b", "c", "d", "e" };
                var isWall = walls.Contains(hitPoint.Sphere.Name);
                                
                var rayVecNorm = Vector3.Normalize(hitPoint.Ray.DirectionVec);
                var rayVec = (hitPoint.Ray.Lambda * rayVecNorm) + (-rayVecNorm * 0.001f) /* nudging..? */;
                var hitPointVec = hitPoint.Ray.StartVec + rayVec;
                var nVecNorm = Vector3.Normalize(hitPointVec - hitPoint.Sphere.Center);

                var material = hitPoint.Sphere.CalcColor(hitPointVec);

                if (Reflection)
                {
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
                }

                foreach (var lightSource in LightSources)
                {
                    var lVec = lightSource.Center - hitPointVec;
                    var lVecNorm = Vector3.Normalize(lVec);

                    var light_cos = Vector3.Dot(nVecNorm, lVecNorm);

                    if (light_cos >= 0)
                    {
                        var light = Conversions.FromColor(lightSource.Color);

                        if (Shadows)
                        {
                            // Shadows 
                            if (HasObjectInFrontOfLightSource(hitPointVec, lightSource))
                            {
                                // Question: What is meant by "contribute nothing if it is occluded"?
                                //light = Conversions.FromColor(Color.FromRgb(32, 32, 32));
                                //light = Conversions.FromColor(Colors.Gray);
                                light_cos = 0.05f;
                            }
                        }

                        if (DiffuseLambert)
                        {
                            // Diffuse "Lambert" 
                            var diffuse = Vector3.Multiply(light, material) * light_cos;
                            rgb += diffuse;
                        }

                        if (SpecularPhong)
                        {
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

        private List<HitPoint> FindHitpoints(Ray ray, BVHNode node)
        {
            var hitPoints = new List<HitPoint>();

            var queue = new Queue<BVHNode>();
            queue.Enqueue(node);

            while(queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current.Items.Count > 0)
                {
                    foreach (var item in current.Items)
                    {
                        var hitPoint = FindHitpoint(ray, item);
                        if (hitPoint != null)
                        {
                            hitPoints.Add(hitPoint);
                        }
                    }
                }
                else
                {
                    if (current.Left != null)
                    {
                        var boundingSphere = current.Left.BoundingSphere;
                        if (FindHitpoint(ray, boundingSphere) != null)
                        {
                            queue.Enqueue(current.Left);
                        }
                    }

                    if (current.Right != null)
                    {
                        var boundingSphere = current.Right.BoundingSphere;
                        if (FindHitpoint(ray, boundingSphere) != null)
                        {
                            queue.Enqueue(current.Right);
                        }
                    }
                }
            }

            return hitPoints;
        }

        private HitPoint FindClosestHitPoint(Ray ray)
        {
            var hitPoints = new List<HitPoint>();

            if (AccelerationStructure)
            {
                BVHNode root = _accelerationStructure;
                hitPoints.AddRange(FindHitpoints(ray, root));
            }
            else
            {
                foreach (var sphere in Spheres)
                {
                    var hitPoint = FindHitpoint(ray, sphere);

                    if (hitPoint != null)
                        hitPoints.Add(hitPoint);
                }
            }

            return hitPoints.OrderBy(h => h.Ray.Lambda).FirstOrDefault();
        }

        private bool HasObjectInFrontOfLightSource(Vector3 hitPointVec, LightSource lightSource)
        {
            var lVec = lightSource.Center - hitPointVec;
            var ray = new Ray(hitPointVec, lVec);
            var length = Vector3.DistanceSquared(ray.StartVec, lVec);

            if (AccelerationStructure)
            {
                BVHNode root = _accelerationStructure;
                var hitPoints = FindHitpoints(ray, root);

                foreach (var hitPoint in hitPoints)
                {
                    var rayVec = hitPoint.Ray.Lambda * hitPoint.Ray.DirectionVec;
                    var scale = Vector3.DistanceSquared(hitPoint.Ray.StartVec, rayVec) / length;

                    if (scale <= 1)
                        return true;
                }
            }
            else
            {
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

                //var lambda = (float)Math.Min(Math.Max(lambda1, 0), Math.Max(lambda2, 0));

                var lambda = 0f;

                if (lambda1 > 0)
                    lambda = (float)lambda1;

                if (lambda2 > 0 && lambda2 < lambda)
                    lambda = (float)lambda2;

                if (lambda > 0)
                    return new HitPoint(new Ray(ray.StartVec, lambda, Vector3.Normalize(ray.DirectionVec)), sphere);
            }

            return null;
        }
    }
}
 