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
        private bool _isRunning = true;

        public PartBWindow()
        {
            InitializeComponent();
            this.Loaded += PartBWindow_Loaded;
            this.KeyUp += PartBWindow_KeyUp;
        }

        private void PartBWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                _isRunning = !_isRunning;
            }
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
                new LightSource("w", new Vector3(0.5f, 0.5f, -5), Colors.White)
            };

            _scene = new SceneB((int)_screenWidth, (int)_screenHeight, _pixelsPerInchX, _pixelsPerInchY, triangles, lightSources);

            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private double _screenWidth;
        private double _screenHeight;
        private double _pixelsPerInchX;
        private double _pixelsPerInchY;

        private SceneB _scene;

        private int rotationInDegrees = 0;

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                rotationInDegrees += 10;
                if (rotationInDegrees > 360)
                    rotationInDegrees = 0;

                var transform = RotateXYZ(rotationInDegrees)
                    * Matrix4x4.CreateTranslation(0, 0, 5);

                _scene.ApplyTransform(transform);

                Image.Source = _scene.GetImage();
            }
        }

        private static float DegreesToRadians(float degree) => (float)(Math.PI / 180) * degree;

        public Matrix4x4 RotateX(float angleInDegrees)
        {
            var radians = DegreesToRadians(angleInDegrees);
            var transform =
            Matrix4x4.CreateRotationX(radians);

            return transform;
        }

        public Matrix4x4 RotateY(float angleInDegrees)
        {
            var radians = DegreesToRadians(angleInDegrees);
            var transform =
            Matrix4x4.CreateRotationY(radians);

            return transform;
        }

        public Matrix4x4 RotateZ(float angleInDegrees)
        {
            var radians = DegreesToRadians(angleInDegrees);
            var transform =
            Matrix4x4.CreateRotationZ(radians);

            return transform;
        }

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
                (0, 1, 2, 2), // top
                (0, 2, 3, 2),

                (7, 6, 5, 3), // bottom
                (7, 5, 4, 3),

                (0, 3, 7, 0), // left
                (0, 7, 4, 0),

                (2, 1, 5, 1), // right                
                (2, 5, 6, 1),

                (3, 2, 6, 4), // front
                (3, 6, 7, 4),

                (1, 0, 4, 5), // back                
                (1, 4, 5, 5)
            };

            var colors = new Vector3[]
            {
                new Vector3(1, 0, 0), // red
                new Vector3(0, 1, 0), // green
                new Vector3(0, 0, 1) // blue
            };

            var normals = new Vector3[]
            {
                -Vector3.UnitX,
                Vector3.UnitX,
                -Vector3.UnitY,
                Vector3.UnitY,
                -Vector3.UnitZ,
                Vector3.UnitZ
            };

            var random = new Random();

            foreach (var t in triangleIdx)
            {
                var v1 = new Vertex(points[t.Item1], colors[random.Next(0, colors.Length)], screenWidth, screenHeight);
                var v2 = new Vertex(points[t.Item2], colors[random.Next(0, colors.Length)], screenWidth, screenHeight);
                var v3 = new Vertex(points[t.Item3], colors[random.Next(0, colors.Length)], screenWidth, screenHeight);
                var n = normals[t.Item4];

                triangles.Add(new Triangle(v1, v2, v3));
            }

            return triangles.ToArray();
        }
    }
}
