using Comgr.CourseProject.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Comgr.CourseProject.UI
{
    public partial class PartBWindow : Window
    {
        public PartBWindow()
        {
            InitializeComponent();
            this.Loaded += PartBWindow_Loaded;
        }

        private void PartBWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _screenWidth = Image.Width;
            _screenHeight = Image.Height;

            var dpiScale = VisualTreeHelper.GetDpi(this);
            _pixelsPerInchX = dpiScale.PixelsPerInchX;
            _pixelsPerInchY = dpiScale.PixelsPerInchY;

            var triangles = GetTriangles((int)_screenWidth, (int)_screenHeight);
            var lightSources = new LightSource[]
            {
                new LightSource("w", new Vector3(0, -10, 0), Colors.White)
            };

            _scene = new SceneB((int)_screenWidth, (int)_screenHeight, _pixelsPerInchX, _pixelsPerInchY, triangles, lightSources);

            _sw = Stopwatch.StartNew();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private double _screenWidth;
        private double _screenHeight;
        private double _pixelsPerInchX;
        private double _pixelsPerInchY;

        private SceneB _scene;

        private Stopwatch _sw;
        private int _rotationInDegrees = 0;

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            _rotationInDegrees += 5;
            if (_rotationInDegrees > 360)
                _rotationInDegrees = 0;

            var transformation = Matrix4x4.Identity;
            transformation *= RotateXYZ(_rotationInDegrees);
            transformation *= Matrix4x4.CreateTranslation(0, 0, 5);

            _scene.Transformation = transformation;

            Image.Source = _scene.GetImage();
        }

        private static float DegreesToRadians(float degree) => (float)(Math.PI / 180) * degree;

        public Matrix4x4 RotateXYZ(float angleInDegrees)
        {
            var radians = DegreesToRadians(angleInDegrees);
            var transform =
            Matrix4x4.CreateRotationX(radians)
            * Matrix4x4.CreateRotationY(radians)
            * Matrix4x4.CreateRotationZ(radians);

            return transform;
        }

        private Triangle[] GetTriangles(int screenWidth, int screenHeight)
        {
            var triangles = new List<Triangle>();

            var points = new Vector3[]
            {
                // top
                new Vector3(-1, -1, -1),
                new Vector3(+1, -1, -1),
                new Vector3(+1, +1, -1),
                new Vector3(-1, +1, -1),

                //bottom                
                new Vector3(-1, -1, +1),
                new Vector3(+1, -1, +1),
                new Vector3(+1, +1, +1),
                new Vector3(-1, +1, +1)
            };


            var triangleIdx = new List<(int, int, int, int)>
            {
                (0, 1, 2, 3), // top
                (0, 2, 3, 3),

                (7, 6, 5, 2), // bottom
                (7, 5, 4, 2),

                (0, 3, 7, 1), // left
                (0, 7, 4, 1),

                (2, 1, 5, 0), // right                
                (2, 5, 6, 0),

                (3, 2, 6, 5), // front
                (3, 6, 7, 5),

                (1, 0, 4, 4), // back                
                (1, 4, 5, 4)
            };

            var colors = new Vector3[]
            {
                new Vector3(1, 0, 0), // red
                new Vector3(0, 1, 0), // green
                new Vector3(0, 0, 1) // blue
            };

            var normals = new Vector3[]
            {
                Vector3.UnitX,
                -Vector3.UnitX,
                Vector3.UnitY,
                -Vector3.UnitY,
                Vector3.UnitZ,
                -Vector3.UnitZ
            };

            var random = new Random();

            foreach (var t in triangleIdx)
            {
                var v1 = new Vertex(points[t.Item1], colors[random.Next(0, colors.Length)], screenWidth, screenHeight);
                var v2 = new Vertex(points[t.Item2], colors[random.Next(0, colors.Length)], screenWidth, screenHeight);
                var v3 = new Vertex(points[t.Item3], colors[random.Next(0, colors.Length)], screenWidth, screenHeight);
                var n = normals[t.Item4];

                triangles.Add(new Triangle(v1, v2, v3, n));
            }

            return triangles.ToArray();
        }
    }
}
