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
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private readonly Stopwatch _sw = Stopwatch.StartNew();

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (_sw.Elapsed.Seconds > 1)
            {
                var width = Image.Width;
                var height = Image.Height;
                var dpiScale = VisualTreeHelper.GetDpi(this);

                var triangles = GetTriangles((float)width, (float)height);
                var scene = new SceneB(triangles);

                Image.Source = scene.GetImage((int)width, (int)height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);

                _sw.Restart();
            }
        }

        DateTime start = DateTime.Now;

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

            var translate = new Vector3(0, 0, 5 + (float)Math.Cos((DateTime.Now - start).TotalSeconds));
            //var translate = new Vector3(0, 0, 5);

            foreach (var t in triangleIdx)
            {
                var v1 = points[t.Item1] + translate;
                var x1 = width * v1.X / v1.Z + width / 2;
                var y1 = width * v1.Y / v1.Z + height / 2;
                var p1 = new Vector2(x1, y1);                                

                var v2 = points[t.Item2] + translate;
                var x2 = width * v2.X / v2.Z + width / 2;
                var y2 = width * v2.Y / v2.Z + height / 2;
                var p2 = new Vector2(x2, y2);
                
                var v3 = points[t.Item3] + translate;
                var x3 = width * v3.X / v3.Z + width / 2;
                var y3 = width * v3.Y / v3.Z + height / 2;
                var p3 = new Vector2(x3, y3);

                triangles.Add(new Triangle(p1, p2, p3));
            }

            return triangles.ToArray();
        }

        //private void DrawWireframeCube()
        //{
        //    var width = this.Width;
        //    var height = this.Height;

        //    Canvas.Children.Clear();

        //    var points = new Vector3[]
        //    {
        //        // top
        //        new Vector3(-1, -1, -1),
        //        new Vector3(+1, -1, -1),
        //        new Vector3(+1, +1, -1),
        //        new Vector3(-1, +1, -1),

        //        //bottom                
        //        new Vector3(-1, -1, +1),
        //        new Vector3(+1, -1, +1),
        //        new Vector3(+1, +1, +1),
        //        new Vector3(-1, +1, +1)
        //    };
            

        //    var triangleIdx = new List<(int, int, int)>
        //    {
        //        (0, 1, 2), // top
        //        (0, 2, 3),

        //        (7, 6, 5), // bottom
        //        (7, 5, 4),
                
        //        (0, 3, 7), // left
        //        (0, 7, 4),
                
        //        (2, 1, 5), // right                
        //        (2, 5, 6),
                
        //        (3, 2, 6), // front
        //        (3, 6, 7),
                
        //        (1, 0, 4), // back                
        //        (1, 4, 5),
        //    };

        //    var translate = new Vector3(0, 0, 5+(float)Math.Cos((DateTime.Now-start).TotalSeconds));
        //    //var translate = new Vector3(0, 0, 5);

        //    foreach (var t in triangleIdx)
        //    {
        //        var p = new Polygon();
        //        p.Stroke = Brushes.Black;

        //        var v1 = points[t.Item1] + translate;
        //        var x1 = width * v1.X / v1.Z + width / 2;
        //        var y1 = width * v1.Y / v1.Z + height / 2;
        //        var p1 = new Point(x1, y1);

        //        p.Points.Add(p1);

        //        var v2 = points[t.Item2] + translate;
        //        var x2 = width * v2.X / v2.Z + width / 2;
        //        var y2 = width * v2.Y / v2.Z + height / 2;
        //        var p2 = new Point(x2, y2);
                
        //        p.Points.Add(p2);

        //        var v3 = points[t.Item3] + translate;
        //        var x3 = width * v3.X / v3.Z + width / 2;
        //        var y3 = width * v3.Y / v3.Z + height / 2;
        //        var p3 = new Point(x3, y3);

        //        p.Points.Add(p3);

        //        Canvas.Children.Add(p);
        //    }
        //}

        //private void DrawRandomTriangle()
        //{
        //    var width = this.Width;
        //    var height = this.Height;

        //    Canvas.Children.Clear();

        //    var p = new Polygon();
        //    p.Stroke = Brushes.Black;
        //    p.Points.Add(GetRandomPoint(width, height));
        //    p.Points.Add(GetRandomPoint(width, height));
        //    p.Points.Add(GetRandomPoint(width, height));

        //    Canvas.Children.Add(p);
        //}

        //private Point GetRandomPoint(double maxWidth, double maxHeight)
        //{
        //    var x = Math.Round(_random.NextDouble() * maxWidth);
        //    var y = Math.Round(_random.NextDouble() * maxHeight);

        //    return new Point(x, y);
        //}
    }
}
