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
    /// <summary>
    /// Interaktionslogik für PartBWindow.xaml
    /// </summary>
    public partial class PartBWindow : Window
    {
        public PartBWindow()
        {
            InitializeComponent();
            this.Loaded += PartBWindow_Loaded;
        }

        private void PartBWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _width = Image.Width;
            _height = Image.Height;

            var dpiScale = VisualTreeHelper.GetDpi(this);
            _pixelsPerInchX = dpiScale.PixelsPerInchX;
            _pixelsPerInchY = dpiScale.PixelsPerInchY;

            var triangles = GetTriangles((float)_width, (float)_height);
            _scene = new SceneB(triangles, (int)_width, (int)_height, _pixelsPerInchX, _pixelsPerInchY);

            _sw = Stopwatch.StartNew();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private double _width;
        private double _height;
        private double _pixelsPerInchX;
        private double _pixelsPerInchY;

        private SceneB _scene;

        private Stopwatch _sw;
        private int _rotationInDegrees = 0;

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_sw.Elapsed.TotalMilliseconds >= 0)
            {
                _rotationInDegrees += 5;
                if (_rotationInDegrees > 360)
                    _rotationInDegrees = 0;

                var transformation = Matrix4x4.Identity;
                transformation *= RotateXYZ(_rotationInDegrees);
                transformation *= Matrix4x4.CreateTranslation(0, 0, 5);

                _scene.Transformation = transformation;

                Image.Source = _scene.GetImage();

                _sw.Restart();
            }
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

        private Triangle[] GetTriangles(float width, float height)
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


            var triangleIdx = new List<(int, int, int)>
            {
                (0, 1, 2), // top
                (0, 2, 3),

                (7, 6, 5), // bottom
                (7, 5, 4),

                (0, 3, 7), // left
                (0, 7, 4),

                (2, 1, 5), // right                
                (2, 5, 6),

                (3, 2, 6), // front
                (3, 6, 7),

                (1, 0, 4), // back                
                (1, 4, 5),
            };

            foreach (var t in triangleIdx)
            {
                var v1 = points[t.Item1];
                var v2 = points[t.Item2];
                var v3 = points[t.Item3];

                triangles.Add(new Triangle(v1, v2, v3));
            }

            return triangles.ToArray();
        }
    }
}
