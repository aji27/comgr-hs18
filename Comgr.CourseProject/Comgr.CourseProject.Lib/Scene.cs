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
        private readonly Random _random = new Random(Seed: 0);
        private readonly object _sync_random = new object();

        private Vector3 _eyeVector;
        private Vector3 _lookAtVector;
        private float _fieldOfView;
        private ICollection<Sphere> _spheres;
        private ICollection<LightSource> _lightSources;
        private BVHNode _accelerationStructure;

        public Scene(Vector3 eye, Vector3 lookAt, float fieldOfView)
        {
            _eyeVector = eye;
            _lookAtVector = lookAt;
            _fieldOfView = fieldOfView;

            _spheres = new List<Sphere>();
            _lightSources = new List<LightSource>();

            fVectorNorm = Vector3.Normalize(LookAt - Eye);
            rVectorNorm = Vector3.Normalize(Vector3.Cross(fVectorNorm, Up));
            uVectorNorm = Vector3.Normalize(Vector3.Cross(rVectorNorm, fVectorNorm));

            var fieldOfViewInRadians = (Math.PI / 180) * (_fieldOfView / 2);
            FOV_tan = (float)Math.Tan(fieldOfViewInRadians);
        }

        public bool AntiAliasing { get; set; } = false;

        public int AntiAliasingSampleSize { get; set; } = 64;

        public bool Parallelize { get; set; } = true;

        public bool GammaCorrect { get; set; } = false;

        public bool DiffuseLambert { get; set; } = true;

        public bool SpecularPhong { get; set; } = false;

        public int SpecularPhongFactor { get; set; } = 1000;

        public bool Reflection { get; set; } = false;

        public bool Shadows { get; set; } = true;

        public bool SoftShadows { get; set; } = false;

        public int SoftShadowFeelers { get; set; } = 8;

        public bool AccelerationStructure { get; set; } = false;

        public bool PathTracing { get; set; } = true;

        public int PathTracingRays { get; set; } = 128;

        public static readonly Vector3 Up = -Vector3.UnitY;

        public Vector3 Eye => _eyeVector;

        public Vector3 LookAt => _lookAtVector;

        public float FieldOfView => _fieldOfView;

        public ICollection<Sphere> Spheres => _spheres;

        public ICollection<LightSource> LightSources => _lightSources;

        private Vector3 fVectorNorm { get; set; }

        private Vector3 rVectorNorm { get; set; }

        private Vector3 uVectorNorm { get; set; }

        private float FOV_tan { get; set; }

        public ImageSource GetImage(int width, int height, double dpiX, double dpiY)
        {
            var sw = Stopwatch.StartNew();

            if (AccelerationStructure)
            {
                _accelerationStructure = BVHNode.BuildTopDown(_spheres);
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
                        if (AntiAliasing)
                        {
                            Vector3 result = Vector3.Zero;
                            for (int k = 0; k < AntiAliasingSampleSize; k++)
                            {
                                var dx = 0f;
                                var dy = 0f;

                                lock (_sync_random)
                                {
                                    dx = (float)_random.NextGaussian(0d, 0.5d);
                                    dy = (float)_random.NextGaussian(0d, 0.5d);
                                }

                                var x = (i + alignX + dx) / divideX;
                                var y = ((height - j) + alignY + dy) / divideY;
                                var rgb = GetColor(x, y);
                                result += rgb;
                            }

                            var avg_rgb = result * (1f / AntiAliasingSampleSize);
                            var c = Conversions.FromRGB(avg_rgb, GammaCorrect);
                            bitmap.Set(i, j, c);
                        }
                        else
                        {
                            var x = (i + alignX) / divideX;
                            var y = ((height - j) + alignY) / divideY;
                            var rgb = GetColor(x, y);
                            var c = Conversions.FromRGB(rgb, GammaCorrect);
                            bitmap.Set(i, j, c);
                        }

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
                        if (AntiAliasing)
                        {
                            Vector3 result = Vector3.Zero;
                            for (int k = 0; k < AntiAliasingSampleSize; k++)
                            {
                                var x = (i + alignX + (float)_random.NextGaussian(0d, 0.5d)) / divideX;
                                var y = ((height - j) + alignY + (float)_random.NextGaussian(0d, 0.5d)) / divideY;
                                var rgb = GetColor(x, y);
                                result += rgb;
                            }

                            var avg_rgb = result * (1f / AntiAliasingSampleSize);
                            var c = Conversions.FromRGB(avg_rgb, GammaCorrect);
                            bitmap.Set(i, j, c);
                        }
                        else
                        {
                            // Question: Why is it mirrored?
                            var x = (i + alignX) / divideX;
                            var y = ((height - j) + alignY) / divideY;
                            var rgb = GetColor(x, y);
                            var c = Conversions.FromRGB(rgb, GammaCorrect);
                            bitmap.Set(i, j, c);
                        }

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
                
        public Vector3 GetColor(float x, float y)
        {
            var pixel = new Vector2(x, y);
            var ray = CreateEyeRay(pixel);
            return CalcColor(ray);
        }

        private Vector3 CalcColor(Ray ray, int reflectionLimit = 1, int pathTracingLimit = 1)
        {
            var rgb = Vector3.Zero;

            var hitPoint = FindClosestHitPoint(ray);

            if (hitPoint != null)
            {
                var sphere = hitPoint.Sphere;
                
                var rayVecNorm = Vector3.Normalize(hitPoint.Ray.DirectionVec);
                var rayVec = (hitPoint.Ray.Lambda * rayVecNorm) + (-rayVecNorm * 0.001f) /* nudging..? */;
                var hitPointVec = hitPoint.Ray.StartVec + rayVec;
                var nVecNorm = Vector3.Normalize(hitPointVec - hitPoint.Sphere.Center);

                var material = sphere.CalcColor(hitPointVec);

                var directDiffuse = Vector3.Zero;

                if (Reflection)
                {
                    // Reflection
                    if (!sphere.IsWall /* ignore walls */
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
                            if (SoftShadows)
                            {
                                light_cos *= GetShadowedRatio(hitPointVec, lightSource);
                            }
                            else
                            {
                                // Shadows 
                                if (HasObjectInFrontOfLightSource(hitPointVec, lightSource.Center))
                                {
                                    light_cos *= 0.01f;
                                }
                            }
                        }

                        if (DiffuseLambert)
                        {
                            // Diffuse "Lambert" 
                            var diffuse = Vector3.Multiply(light, material) * light_cos;
                            directDiffuse += diffuse;
                        }

                        if (SpecularPhong)
                        {
                            // Ignore walls
                            if (!sphere.IsWall)
                            {
                                // Specular "Phong"
                                var sVec = (lVec - ((Vector3.Dot(lVec, nVecNorm)) * nVecNorm));
                                var rVec = lVec - (2 * sVec);
                                var specular = light * (float)Math.Pow((Vector3.Dot(Vector3.Normalize(rVec), rayVecNorm)), SpecularPhongFactor);
                                rgb += specular;
                            }
                        }
                    }
                }
                
                if (PathTracing)
                {
                    var russianRoulette = 0d;

                    if (Parallelize)
                    {
                        lock (_sync_random)
                        {
                            russianRoulette = _random.NextDouble();
                        }
                    }
                    else
                    {
                        russianRoulette = _random.NextDouble();
                    }

                    if (pathTracingLimit > 0
                        && russianRoulette > 0.2d)
                    {
                        // Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/global-illumination-path-tracing-practical-implementation

                        Vector3 indirectDiffuse = Vector3.Zero;

                        Vector3 ntVec = Vector3.Zero;

                        if (Math.Abs(nVecNorm.X) > Math.Abs(nVecNorm.Y))
                            ntVec = (new Vector3(nVecNorm.Z, 0, -nVecNorm.X) / (float)Math.Sqrt(nVecNorm.X * nVecNorm.X + nVecNorm.Z * nVecNorm.Z));
                        else
                            ntVec = (new Vector3(0, -nVecNorm.Z, nVecNorm.Y) / (float)Math.Sqrt(nVecNorm.Y * nVecNorm.Y + nVecNorm.Z * nVecNorm.Z));

                        var nbVec = Vector3.Cross(nVecNorm, ntVec);

                        for (int i = 0; i < PathTracingRays; i++)
                        {
                            var cosTheta = 0f;
                            var phi = 0f;

                            if (Parallelize)
                            {
                                lock (_sync_random)
                                {
                                    cosTheta = (float)_random.NextDouble();
                                    phi = (float)(_random.NextDouble() * 2 * Math.PI);
                                }
                            }
                            else
                            {
                                cosTheta = (float)_random.NextDouble();
                                phi = (float)(_random.NextDouble() * 2 * Math.PI);
                            }

                            var sinTheta = (float)Math.Sqrt(1 - cosTheta * cosTheta);
                            float x = sinTheta * (float)Math.Cos(phi);
                            float z = sinTheta * (float)Math.Sin(phi);

                            var sampleLocalVec = new Vector3(x, cosTheta, z);
                            var sampleWorldVec = new Vector3()
                            {
                                X = sampleLocalVec.X * nbVec.X + sampleLocalVec.Y * nVecNorm.X + sampleLocalVec.Z * ntVec.X,
                                Y = sampleLocalVec.X * nbVec.Y + sampleLocalVec.Y * nVecNorm.Y + sampleLocalVec.Z * ntVec.Y,
                                Z = sampleLocalVec.X * nbVec.Z + sampleLocalVec.Y * nVecNorm.Z + sampleLocalVec.Z * ntVec.Z
                            };

                            var randomRay = new Ray(hitPointVec, sampleWorldVec);
                            indirectDiffuse += CalcColor(randomRay, reflectionLimit, pathTracingLimit - 1) * cosTheta;                            
                        }

                        indirectDiffuse /= PathTracingRays;

                        rgb += Vector3.Multiply((directDiffuse / (float)Math.PI + 2 * indirectDiffuse), material);
                    }
                }
                else
                {
                    rgb += directDiffuse;
                }
            }

            return rgb;
        }

        private Ray CreateEyeRay(Vector2 pixel)
        {
            var directionVector = fVectorNorm + (pixel.X * rVectorNorm * FOV_tan) + (pixel.Y * uVectorNorm * FOV_tan);
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

        private float GetShadowedRatio(Vector3 hitPointVec, LightSource lightSource)
        {
            int hits = 0;

            var lVec = lightSource.Center - hitPointVec;
            Vector3 xVecNorm = Vector3.Normalize(Vector3.Cross(lVec, Up));
            Vector3 yVecNorm = Vector3.Normalize(Vector3.Cross(lVec, xVecNorm));

            for (int i = 0; i < SoftShadowFeelers; i++)
            {
                // Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/introduction-global-illumination-path-tracing

                var cosTheta = 0f;
                var phi = 0f;

                if (Parallelize)
                {
                    lock (_sync_random)
                    {
                        cosTheta = (float)_random.NextDouble();
                        phi = (float)(_random.NextDouble() * 2 * Math.PI);
                    }
                }
                else
                {
                    cosTheta = (float)_random.NextDouble();
                    phi = (float)(_random.NextDouble() * 2 * Math.PI);
                }

                float x = (float)(Math.Sqrt(cosTheta) * Math.Sin(phi));
                float y = (float)(Math.Sqrt(cosTheta) * Math.Cos(phi));

                var randomVec = lightSource.Center + lightSource.Radius * xVecNorm * x + lightSource.Radius * yVecNorm * y;

                if (HasObjectInFrontOfLightSource(hitPointVec, randomVec))
                    hits++;
            }

            return (float)(SoftShadowFeelers - hits) / SoftShadowFeelers;
        }

        private bool HasObjectInFrontOfLightSource(Vector3 hitPointVec, Vector3 pos)
        {
            var lVec = pos - hitPointVec;
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

            var rayDirectionVecNorm = Vector3.Normalize(ray.DirectionVec);

            var b = 2 * Vector3.Dot(ceVec, rayDirectionVecNorm);
            var c = Vector3.Dot(ceVec, ceVec) - (sphere.Radius * sphere.Radius);

            var b_squared = b * b;
            var four_a_c = 4 * c;

            if (b_squared >= four_a_c)
            {
                var sqrtExpr = Math.Sqrt(b_squared - four_a_c);

                var lambda1 = (-b + sqrtExpr) / 2;
                var lambda2 = (-b - sqrtExpr) / 2;

                var lambda = 0f;

                if (lambda1 > 0)
                    lambda = (float)lambda1;

                if (lambda2 > 0 && lambda2 < lambda)
                    lambda = (float)lambda2;

                if (lambda > 0)
                    return new HitPoint(new Ray(ray.StartVec, lambda, rayDirectionVecNorm), sphere);
            }

            return null;
        }
    }
}
 