using Comgr.CourseProject.Lib;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            ShowCornellBox(multipleLightSources: true);
        }

        private void ShowGradient()
        {
            // ** Gradient Green to Red **
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var gradient = new GradientRectangle(Conversions.FromRGB(0, 1.0f, 0), Conversions.FromRGB(1.0f, 0, 0));
            Image.Source = gradient.GetBitmap((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);
        }

        private void ShowCornellBox(bool multipleLightSources)
        {
            var eye = new Vector3(0, 0, -4);
            var lookAt = new Vector3(0, 0, 6);
            var fieldOfView = 36f;

            var scene = new Scene(eye, lookAt, fieldOfView, gammaCorrect: false);
            scene.Spheres.Add(new Sphere("a", new Vector3(-1001, 0, 0), 1000f, Colors.Red));
            scene.Spheres.Add(new Sphere("b", new Vector3(1001, 0, 0), 1000f, Colors.Blue));
            scene.Spheres.Add(new Sphere("c", new Vector3(0, 0, 1001), 1000f, Colors.White));
            scene.Spheres.Add(new Sphere("d", new Vector3(0, -1001, 0), 1000f, Colors.White));
            scene.Spheres.Add(new Sphere("e", new Vector3(0, 1001, 0), 1000f, Colors.White));
            scene.Spheres.Add(new Sphere("f", new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Colors.Yellow));
            scene.Spheres.Add(new Sphere("g", new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Colors.LightCyan));

            // Question: How should I position the light source?

            if (!multipleLightSources)
            {
                // 1 Light Source
                scene.LightSources.Add(new LightSource("w", new Vector3(0, -0.9f, 0), Colors.White));
            }
            else
            {
                // 3 Light Sources
                scene.LightSources.Add(new LightSource("c", new Vector3(0.5f, -0.9f, 0.3f), Colors.Cyan.ChangIntensity(0.5f)));
                scene.LightSources.Add(new LightSource("m", new Vector3(-0.5f, -0.9f, 0.3f), Colors.Magenta.ChangIntensity(0.5f)));
                scene.LightSources.Add(new LightSource("y", new Vector3(0, -0.9f, -0.6f), Colors.Yellow.ChangIntensity(0.5f)));
            }

            var dpiScale = VisualTreeHelper.GetDpi(this);
            Image.Source = scene.GetImage((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);

            // Question: Why is there a gap on the right side?
            // Question: How should I nudge?
            // Question: Why are the edge of my spheres black?
            // Question: Why do my reflections have more depth?
            // Question: You didn't gamma correct in your examples? 
            // Question: Some shadows are not overlapped? Why is that? For e.g. right down corner?
        }
    }

    public static class ColorExtesions
    {
        public static Color ChangIntensity(this Color color, float intensity)
        {
            return new Color() { ScA = 1f, ScB = color.ScB * intensity, ScG = color.ScG * intensity, ScR = color.ScR * intensity };
        }
    }
}
