using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class BVHNode
    {
        public const int MinPartitionSize = 2;

        private List<Sphere> _items;
        private Sphere _boundingSphere;

        public BVHNode(Sphere[] spheres)
        {
            if (spheres == null
                || spheres.Length == 0)
                throw new ArgumentNullException(nameof(spheres));

            _items = new List<Sphere>(spheres);

            _boundingSphere = CalculateBoundingSphere(spheres);
        }

        public int Size => _items.Count;

        public BVHNode Left { get; set; }

        public BVHNode Right { get; set; }

        public IList<Sphere> Items => _items;

        public Sphere BoundingSphere => _boundingSphere;
        
        private Sphere CalculateBoundingSphere(IList<Sphere> spheres)
        {
            if (spheres.Count == 1)
            {
                return new Sphere($"Bounding Sphere", spheres[0].Center, spheres[0].Radius, Colors.Transparent);
            }
            else if (spheres.Count > 1)
            {
                Vector3 min = Vector3.Zero;
                var minLength = float.MaxValue;

                Vector3 max = Vector3.Zero;
                var maxLength = float.MinValue;

                foreach (var sphere in spheres)
                {
                    var center = sphere.Center;
                    var length = center.Length();

                    if (length < minLength)
                    {
                        min = center;
                        minLength = length;
                    }

                    if (length > maxLength)
                    {
                        max = center;
                        maxLength = length;
                    }
                }

                var boundingCenter = (min + max) / 2;
                var boundingRadius = 0f;

                foreach (var sphere in spheres)
                {
                    var d = Vector3.Distance(boundingCenter, sphere.Center);
                    var r = d + sphere.Radius;
                    if (r > boundingRadius)
                        boundingRadius = r;
                }

                return new Sphere($"Bounding Sphere", boundingCenter, boundingRadius, Colors.Transparent);
            }

            return null;
        }

        private static int[] GetSplitOrder(Vector3 v)
        {
            var splitOrder = new List<Tuple<float, int>>()
            {
                new Tuple<float, int>(v.X, 0),
                new Tuple<float, int>(v.Y, 1),
                new Tuple<float, int>(v.Z, 2)
            };

            return splitOrder.OrderByDescending(t => t.Item1).Select(t => t.Item2).ToArray();
        }

        public static BVHNode BuildTopDown(IEnumerable<Sphere> spheres, Action<string> logger, bool print = false)
        {
            BVHNode rootNode = new BVHNode(spheres.ToArray());
            BuildTopDown(rootNode, logger);

            if (print)
            {
                RecursivePrint(rootNode, logger);
            }

            return rootNode;
        }

        private static void BuildTopDown(BVHNode node, Action<string> logger)
        {
            int iterations = 0;

            var queue = new Queue<BVHNode>();
            queue.Enqueue(node);

            BVHNode current = null;

            while (queue.Count > 0)
            {
                ++iterations;

                current = queue.Dequeue();

                if (current.Items.Count > MinPartitionSize)
                {
                    var boundingSphere = current.BoundingSphere;
                    var splitOrder = GetSplitOrder(boundingSphere.Center);

                    foreach (var split_dim in splitOrder)
                    {
                        var leftSpheres = new List<Sphere>();
                        var rightSpheres = new List<Sphere>();

                        var split_coord = 0.5f * ValueAt(boundingSphere.Center, split_dim);

                        foreach (var sphere in current.Items)
                        {
                            if (ValueAt(sphere.Center, split_dim) < split_coord)
                                leftSpheres.Add(sphere);
                            else
                                rightSpheres.Add(sphere);
                        }

                        var split_success 
                            = leftSpheres.Count > 0 
                              && rightSpheres.Count > 0;

                        if (split_success)
                        {   
                            current.Left = new BVHNode(leftSpheres.ToArray());
                            queue.Enqueue(current.Left);

                            current.Right = new BVHNode(rightSpheres.ToArray());
                            queue.Enqueue(current.Right);

                            current.Items.Clear();

                            break;
                        }
                    }
                }
            }

            logger($"BVH constructed in '{iterations}' iterations.");
        }
        
        private static float ValueAt(Vector3 v, int pos)
        {
            if (pos == 0)
                return v.X;
            else if (pos == 1)
                return v.Y;
            else if (pos == 2)
                return v.Z;
            else
                throw new NotSupportedException();
        }

        public static void RecursivePrint(BVHNode node, Action<string> logger, int indent = 0)
        {
            if (node != null)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < indent; i++)
                    sb.Append(" ");

                var indentStr1 = sb.ToString();
                var indentStr2 = indentStr1 + " ";

                var sphere = node.BoundingSphere;
                logger(indentStr1 + FormatSphere(sphere));

                foreach (var item in node.Items)
                    logger(indentStr2 + FormatSphere(item));

                RecursivePrint(node.Left, logger, indent + 2);
                RecursivePrint(node.Right, logger, indent + 2);
            }
        }

        private static string FormatSphere(Sphere sphere) => $"Sphere '{sphere.Name}': Center [{sphere.Center.X:F2}, {sphere.Center.Y:F2}, {sphere.Center.Z:F2}]; Radius {sphere.Radius:F2}";
    }
}
