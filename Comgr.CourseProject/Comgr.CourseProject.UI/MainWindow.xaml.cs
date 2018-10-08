using Comgr.CourseProject.Lib;
using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Comgr.CourseProject.UI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateImage();
        }

        private void UpdateImage()
        {
            // ShowGradient();

            ShowCornellBox();
        }

        private void ShowGradient()
        {
            // ** Gradient Green to Red **
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var gradient = new GradientRectangle(Conversions.FromRGB(0, 1.0f, 0), Conversions.FromRGB(1.0f, 0, 0));
            Image.Source = gradient.GetBitmap((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);
        }

        private void ShowCornellBox()
        {
            var eye = new Vector3(0, 0, -4);
            var lookAt = new Vector3(0, 0, 6);

            // Question: Why do I have to take half of it to make it somehow look right?
            var fieldOfView = 36f;

            var scene = new Scene(eye, lookAt, fieldOfView);
            scene.Spheres.Add(new Sphere("a", new Vector3(-1001, 0, 0), 1000f, Colors.Red));
            scene.Spheres.Add(new Sphere("b", new Vector3(1001, 0, 0), 1000f, Colors.Blue));
            scene.Spheres.Add(new Sphere("c", new Vector3(0, 0, 1001), 1000f, Colors.White));
            scene.Spheres.Add(new Sphere("d", new Vector3(0, -1001, 0), 1000f, Colors.White));
            scene.Spheres.Add(new Sphere("e", new Vector3(0, 1001, 0), 1000f, Colors.White));
            scene.Spheres.Add(new Sphere("f", new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Colors.Yellow));
            scene.Spheres.Add(new Sphere("g", new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Colors.LightCyan));

            // Question: How do I position the light source?
            scene.LightSources.Add(new LightSource("w", new Vector3(0, -0.9f, 0), Colors.White));

            var dpiScale = VisualTreeHelper.GetDpi(this);
            Image.Source = scene.GetImage((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);

            // Question: Why is there a gap on the right side?
        }
    }
}
