using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Comgr.CourseProject.Lib
{
    public class Scene
    {
        private readonly ThreadLocal<Random> _threadLocalRandom = new ThreadLocal<Random>(() => new Random(Seed: 0));

        private Random _random => _threadLocalRandom.Value;

        private const float _pdf = (1f / (2 * (float)Math.PI));

        private Vector3 _eyeVector;
        private Vector3 _lookAtVector;
        private float _fieldOfView;
        private Sphere[] _spheres;
        private LightSource[] _lightSources;
        private BVHNode _accelerationStructure;

        private readonly Action<string> _logger;

        private readonly object _syncLogger = new object();

        private int OutputLogEveryXPixel;

        public Scene(Action<string> logger, Vector3 eye, Vector3 lookAt, float fieldOfView, Sphere[] spheres, LightSource[] lightSources)
        {
            _logger = logger;
            _eyeVector = eye;
            _lookAtVector = lookAt;
            _fieldOfView = fieldOfView;

            _spheres = spheres;
            _lightSources = lightSources;

            fVectorNorm = Vector3.Normalize(LookAt - Eye);
            rVectorNorm = Vector3.Normalize(Vector3.Cross(fVectorNorm, Up));
            uVectorNorm = Vector3.Normalize(Vector3.Cross(rVectorNorm, fVectorNorm));

            var fieldOfViewInRadians = (Math.PI / 180) * (_fieldOfView / 2);
            FOV_tan = (float)Math.Tan(fieldOfViewInRadians);
        }

        public bool AntiAliasing { get; set; } = false;

        public int AntiAliasingSampleSize { get; set; } = 16;

        public bool Parallelize { get; set; } = true;

        public bool GammaCorrect { get; set; } = false;

        public bool DiffuseLambert { get; set; } = false;

        public bool SpecularPhong { get; set; } = false;

        public int SpecularPhongFactor { get; set; } = 1000;

        public bool Reflection { get; set; } = false;

        public int ReflectionBounces { get; set; } = 1;

        public bool Shadows { get; set; } = false;

        public bool SoftShadows { get; set; } = false;

        public int SoftShadowFeelers { get; set; } = 8;

        public bool AccelerationStructure { get; set; } = false;

        public bool PathTracing { get; set; } = true;

        public int PathTracingRays { get; set; } = 1024;

        public int PathTracingMaxBounces { get; set; } = 30;

        public static readonly Vector3 Up = -Vector3.UnitY;

        public Vector3 Eye => _eyeVector;

        public Vector3 LookAt => _lookAtVector;

        public float FieldOfView => _fieldOfView;

        private Vector3 fVectorNorm { get; set; }

        private Vector3 rVectorNorm { get; set; }

        private Vector3 uVectorNorm { get; set; }

        private float FOV_tan { get; set; }
        
        private void WriteOutput(string message)
        {
            _logger(message);
        }

        public string GetImageFileName(int width, int height, double dpiX, double dpiY, CancellationToken cancellationToken, string settingsSummary, string exportDirectory)
        {
            var sw = Stopwatch.StartNew();

            OutputLogEveryXPixel = (width * height / 100);

            if (AccelerationStructure)
            {
                _accelerationStructure = BVHNode.BuildTopDown(_spheres, _logger);
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
                var options = new ParallelOptions() { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount };

                Parallel.For(0, width, options, i =>
                {
                    for (int j = 0; j < height; j++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (AntiAliasing)
                        {
                            Vector3 result = Vector3.Zero;
                            for (int k = 0; k < AntiAliasingSampleSize; k++)
                            {
                                var dx = (float)_random.NextGaussian(0d, 0.5d);
                                var dy = (float)_random.NextGaussian(0d, 0.5d);

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

                        var value = Interlocked.Increment(ref workDone);

                        if (value % OutputLogEveryXPixel == 0)
                        {
                            var progress = (float)value / totalWork;
                            WriteOutput($"{(progress * 100):F3}% progress. Running {sw.Elapsed}. Remaining {TimeSpan.FromMilliseconds(sw.Elapsed.TotalMilliseconds / progress * (1f - progress))}.");
                        }
                    }
                });
            }
            else
            {
                for (int i = 0; i < width; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    for (int j = 0; j < height; j++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            break;

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

                        if (workDone % OutputLogEveryXPixel == 0)
                        {
                            var progress = (float)workDone / totalWork;
                            WriteOutput($"{(progress * 100):F3}% progress. Running {sw.Elapsed}. Remaining {TimeSpan.FromMilliseconds(sw.Elapsed.TotalMilliseconds / progress * (1f - progress))}.");
                        }
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                WriteOutput("Operation canceled by user.");
                return null;
            }

            var imageSource = bitmap.GetImageSource();           

            sw.Stop();

            return SaveImage(imageSource, sw.Elapsed, settingsSummary, exportDirectory);
        }
                
        private string SaveImage(ImageSource imageSource, TimeSpan runTime, string settingsSummary, string exportDirectory)
        {
            if (!Directory.Exists(exportDirectory))
                Directory.CreateDirectory(exportDirectory);

            var fileName = DateTime.Now.ToString("ddMMyyy_HHmmssfff");
            var imageFileName = Path.Combine(exportDirectory, fileName + ".png");
            var bitmap = (WriteableBitmap)imageSource;

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            
            using (var fileStream = File.OpenWrite(imageFileName))
            {
                encoder.Save(fileStream);
            }

            var sb = new StringBuilder();
            sb.AppendLine(settingsSummary);
            sb.AppendLine($"Image generated in {runTime} and saved as '{imageFileName}'.");

            var output = sb.ToString();
            WriteOutput(output);

            var textFileName = Path.Combine(exportDirectory, fileName + ".txt");
            File.WriteAllText(textFileName, output);

            return imageFileName;
        }

        public Vector3 GetColor(float x, float y)
        {
            var pixel = new Vector2(x, y);
            var ray = CreateEyeRay(pixel);
            return CalcColor(ray, PathTracingMaxBounces, ReflectionBounces);
        }
        
        private Vector3 CalcColor(Ray ray, int pathTracingLimit, int reflectionLimit)
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

                if (!PathTracing)
                {
                    if (sphere.Brightness > 0)
                        rgb += material * sphere.Brightness;

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
                            var reflectionColor = CalcColor(reflectionRay, 0, --reflectionLimit);
                            var reflection = Vector3.Multiply(reflectionColor, reflectionMaterial) * 0.25f;
                            rgb += reflection;
                        }
                    }

                    for (int i = 0; i < _lightSources.Length; i++)
                    {
                        var lightSource = _lightSources[i];

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
                                rgb += diffuse;
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
                }
                else
                {
                    if (pathTracingLimit > 0)
                    {
                        var numberOfRandomRays = 0;

                        bool isBounced = pathTracingLimit < PathTracingMaxBounces;

                        if (!isBounced)
                        {
                            numberOfRandomRays = PathTracingRays;
                        }
                        else
                        {
                            var russianRoulette = (float)_random.NextDouble();

                            if (russianRoulette > 0.2f)
                            {
                                numberOfRandomRays = 1;
                            }
                        }

                        if (numberOfRandomRays > 0)
                        {
                            // Source: https://www.scratchapixel.com/lessons/3d-basic-rendering/global-illumination-path-tracing/global-illumination-path-tracing-practical-implementation

                            Vector3 indirectDiffuse = Vector3.Zero;

                            Vector3 ntVec = Vector3.Zero;

                            if (Math.Abs(nVecNorm.X) > Math.Abs(nVecNorm.Y))
                                ntVec = (new Vector3(nVecNorm.Z, 0, -nVecNorm.X) / (float)Math.Sqrt(nVecNorm.X * nVecNorm.X + nVecNorm.Z * nVecNorm.Z));
                            else
                                ntVec = (new Vector3(0, -nVecNorm.Z, nVecNorm.Y) / (float)Math.Sqrt(nVecNorm.Y * nVecNorm.Y + nVecNorm.Z * nVecNorm.Z));

                            var nbVec = Vector3.Cross(nVecNorm, ntVec);

                            for (int i = 0; i < numberOfRandomRays; i++)
                            {
                                var cosTheta = (float)_random.NextDouble();
                                var phi = (float)(_random.NextDouble() * 2 * Math.PI);

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
                                indirectDiffuse += CalcColor(randomRay, pathTracingLimit - 1, 0) * cosTheta;
                            }

                            indirectDiffuse /= numberOfRandomRays * _pdf;
                            
                            rgb += Vector3.Multiply(indirectDiffuse, material) * sphere.Reflectiveness;
                        }
                        else
                        {
                            rgb += material * sphere.Brightness;
                        }
                    }
                    else
                    {
                        rgb += material * sphere.Brightness;
                    }
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

                if (current.Items.Length > 0)
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
                for (int i = 0; i < _spheres.Length; i++)
                {
                    var sphere = _spheres[i];
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

                var cosTheta = (float)_random.NextDouble();
                var phi = (float)(_random.NextDouble() * 2 * Math.PI);

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
                for (int i = 0; i < _spheres.Length; i++)                
                {
                    var sphere = _spheres[i];
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
 