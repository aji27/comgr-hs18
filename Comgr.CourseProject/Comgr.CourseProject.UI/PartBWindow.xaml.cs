using Comgr.CourseProject.Lib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Comgr.CourseProject.UI
{
    public partial class PartBWindow : Window
    {
        private bool _isRunning = true;

        private float _zoom = 5;
        private float _zoomMax = 15;
        private float _zoomMin = -15;

        private static readonly TriangleTexture[] _textures = new TriangleTexture[] 
        {
            new TriangleTexture(@"Resources\TilePattern-n1_UR_1024.png"),
            new TriangleTexture(@"Resources\Tileable_Red_brick_texture_DIFFUSE.jpg"),
            new TriangleTexture(@"Resources\brick_texture1884.jpg_.jpg")
        };

        public PartBWindow()
        {
            InitializeComponent();

            SetControlsEnabled(true);

            Start.Click += Start_Click;
            Cancel.Click += Cancel_Click;

            Image.MouseWheel += Image_MouseWheel;
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_isRunning)
            {
                var delta = -(120f / e.Delta);

                var newZoom = _zoom + delta;
                if (newZoom >= _zoomMin
                    && newZoom <= _zoomMax)
                {
                    _zoom = newZoom;
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _isRunning = false;
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            SetControlsEnabled(true);
            Status.Text = "Status: Ready";
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SetControlsEnabled(false);

                _zoom = 5;

                _screenWidth = Image.Width;
                _screenHeight = Image.Height;

                var dpiScale = VisualTreeHelper.GetDpi(this);
                _pixelsPerInchX = dpiScale.PixelsPerInchX;
                _pixelsPerInchY = dpiScale.PixelsPerInchY;

                var textureIndex = int.Parse(TextureIndex.Text);

                var options = new TriangleOptions()
                {
                    DiffuseLambert = bool.Parse(DiffuseLambert.Text),
                    SpecularPhong = bool.Parse(SpecularPhong.Text),
                    SpecularPhongFactor = float.Parse(SpecularPhongFactor.Text),
                    Texture = textureIndex >= 0 ? _textures[textureIndex] : null,
                    BilinearFilter = bool.Parse(BilinearFilter.Text),
                    GammaCorrect = bool.Parse(GammaCorrect.Text)
                };

                var triangles = GetTriangles((int)_screenWidth, (int)_screenHeight, options);
                var lightSources = new LightSource[]
                {
                    new LightSource("w", new Vector3(0.5f, 0.5f, -5), Colors.White)
                };

                _scene = new SceneB((int)_screenWidth, (int)_screenHeight, _pixelsPerInchX, _pixelsPerInchY, triangles, lightSources, bool.Parse(GammaCorrect.Text), bool.Parse(ZBuffer.Text));

                _isRunning = true;
                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                SetControlsEnabled(true);
            }
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
                var sw = Stopwatch.StartNew();

                rotationInDegrees += 10;
                if (rotationInDegrees > 360)
                    rotationInDegrees = 0;

                var transform = RotateXYZ(rotationInDegrees)
                    * Matrix4x4.CreateTranslation(0, 0, _zoom);

                _scene.ApplyTransform(transform);

                Image.Source = _scene.GetImage();

                sw.Stop();

                Status.Text = $"Status: Running at {(1d / sw.Elapsed.TotalSeconds):F2} fps.";
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

        private Triangle[] GetTriangles(int screenWidth, int screenHeight, TriangleOptions options)
        {
            var triangles = new List<Triangle>();

            var points = new Vector3[]
            {
                new Vector3(-1, -1, -1),
                new Vector3(+1, -1, -1),
                new Vector3(+1, +1, -1),
                new Vector3(-1, +1, -1),              
                new Vector3(-1, -1, +1),
                new Vector3(+1, -1, +1),
                new Vector3(+1, +1, +1),
                new Vector3(-1, +1, +1)
            };

            var triangleIdx = new List<(int, int, int, int, int, int, int)>
            {
                (0, 1, 2, 2, 3, 1, 0), // top
                (0, 2, 3, 2, 3, 0, 2),

                (7, 6, 5, 3, 3, 1, 0), // bottom
                (7, 5, 4, 3, 3, 0, 2),

                (0, 3, 7, 0, 3, 1, 0), // left
                (0, 7, 4, 0, 3, 0, 2),

                (2, 1, 5, 1, 3, 1, 0), // right                
                (2, 5, 6, 1, 3, 0, 2),

                (3, 2, 6, 4, 3, 1, 0), // front
                (3, 6, 7, 4, 3, 0, 2),

                (1, 0, 4, 5, 3, 1, 0), // back                
                (1, 4, 5, 5, 3, 0, 2)
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

            var textureCoordinates = new Vector2[]
            {
                new Vector2(1024, 0),
                new Vector2(1024, 1024),
                new Vector2(0, 0),
                new Vector2(0, 1024)
            };

            var random = new Random();

            foreach (var t in triangleIdx)
            {
                var n = normals[t.Item4];

                bool hasTexture = options.Texture != null;

                var t1 = Vector2.Zero;
                if (hasTexture)
                    t1 = textureCoordinates[t.Item5];

                var v1 = new Vertex(points[t.Item1], colors[random.Next(0, colors.Length)], screenWidth, screenHeight, t1);

                var t2 = Vector2.Zero;
                if (hasTexture)
                    t2 = textureCoordinates[t.Item6];

                var v2 = new Vertex(points[t.Item2], colors[random.Next(0, colors.Length)], screenWidth, screenHeight, t2);

                var t3 = Vector2.Zero;
                if (hasTexture)
                    t3 = textureCoordinates[t.Item7];

                var v3 = new Vertex(points[t.Item3], colors[random.Next(0, colors.Length)], screenWidth, screenHeight, t3);
                
                triangles.Add(new Triangle(v1, v2, v3, options));
            }

            return triangles.ToArray();
        }

        private void SetControlsEnabled(bool enabled)
        {
            TextureIndex.IsEnabled = enabled;
            BilinearFilter.IsEnabled = enabled;
            ZBuffer.IsEnabled = enabled;
            GammaCorrect.IsEnabled = enabled;
            DiffuseLambert.IsEnabled = enabled;
            SpecularPhong.IsEnabled = enabled;
            SpecularPhongFactor.IsEnabled = enabled;

            Start.IsEnabled = enabled;
            Cancel.IsEnabled = !enabled;
        }
    }
}
