using Comgr.CourseProject.Lib;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Comgr.CourseProject.UI
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class PartAWindow : Window
    {
        public PartAWindow()
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

            ShowCornellBox(multipleLightSources: true, coloredLight: true, lotsOfSpheres: false, proceduralTexture: true, bitmapTexture: true);
        }

        private void ShowGradient()
        {
            // ** Gradient Green to Red **
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var gradient = new GradientRectangle(Conversions.FromRGB(0, 1.0f, 0), Conversions.FromRGB(1.0f, 0, 0));
            Image.Source = gradient.GetBitmap((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);
        }

        private void ShowCornellBox(bool multipleLightSources, bool coloredLight, bool lotsOfSpheres, bool proceduralTexture, bool bitmapTexture)
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

            if (!lotsOfSpheres)
            {
                ITexture procTexture = null;
                if (proceduralTexture)
                    procTexture = new CheckerProceduralTexture();

                ITexture bmpTexture = null;
                if (bitmapTexture)
                    bmpTexture = new BitmapTexture(@"Resources\arroway.de_tiles-29_d100.jpg", BitmapTextureMode.PlanarProjection);

                scene.Spheres.Add(new Sphere("f", new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Colors.Yellow, procTexture));
                scene.Spheres.Add(new Sphere("g", new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Colors.LightCyan, bmpTexture));
            }
            else
            {
                int numberOfSpheres = 4096;
                var random = new Random(Seed: 0); // use same seed, so that we can compare results

                for (int i = 0; i < numberOfSpheres; i++)
                {
                    var x = (float)(random.NextDouble() * 2) - 1;
                    var y = (float)(random.NextDouble() * 2) - 1;
                    var z = (float)(random.NextDouble() * 2) - 1;
                    var r = 0.01f;
                    var c = new Color() { ScA = 1, ScR = (float)random.NextDouble(), ScG = (float)random.NextDouble(), ScB = (float)random.NextDouble() };

                    scene.Spheres.Add(new Sphere($"gen{i}", new Vector3(x, y, z), r, c));
                }
            }

            // Question: How should I position the light source?

            if (!multipleLightSources)
            {
                // 1 Light Source
                scene.LightSources.Add(new LightSource("w", new Vector3(0, -0.9f, 0), coloredLight ? Colors.LightSalmon : Colors.White));
            }
            else
            {
                // 3 Light Sources
                scene.LightSources.Add(new LightSource("c", new Vector3(0.5f, -0.9f, 0.3f), coloredLight ?  Colors.Cyan.ChangIntensity(0.5f) : Colors.White.ChangIntensity(0.5f)));
                scene.LightSources.Add(new LightSource("m", new Vector3(-0.5f, -0.9f, 0.3f), coloredLight ? Colors.Magenta.ChangIntensity(0.5f) : Colors.White.ChangIntensity(0.5f)));
                scene.LightSources.Add(new LightSource("y", new Vector3(0, -0.9f, -0.6f), coloredLight ? Colors.Yellow.ChangIntensity(0.5f) : Colors.White.ChangIntensity(0.5f)));
            }

            var dpiScale = VisualTreeHelper.GetDpi(this);
            Image.Source = scene.GetImage((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);

            // Question: Why is there a gap on the right side?
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
