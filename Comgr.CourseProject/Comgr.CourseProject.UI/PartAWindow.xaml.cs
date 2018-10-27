using Comgr.CourseProject.Lib;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Comgr.CourseProject.UI
{
    public partial class PartAWindow : Window
    {
        private CancellationTokenSource _cts;

        public PartAWindow()
        {
            InitializeComponent();

            Start.Click += Start_Click;
            Cancel.Click += Cancel_Click;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_cts == null)
            {
                try
                {
                    await ShowCornellBox(multipleLightSources: false, coloredLight: false, lotsOfSpheres: false, proceduralTexture: false, bitmapTexture: false, pathTracing: true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    _cts = null;
                    Status.Text = "Status: Ready";
                    SetControlsEnabled(true);
                }
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
            }
        }
        
        private void ShowGradient()
        {
            // ** Gradient Green to Red **
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var gradient = new GradientRectangle(Conversions.FromRGB(0, 1.0f, 0), Conversions.FromRGB(1.0f, 0, 0));
            Image.Source = gradient.GetBitmap((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);
        }

        private async Task ShowCornellBox(bool multipleLightSources, bool coloredLight, bool lotsOfSpheres, bool proceduralTexture, bool bitmapTexture, bool pathTracing)
        {
            var eye = new Vector3(0, 0, -4);
            var lookAt = new Vector3(0, 0, 6);
            var fieldOfView = 36f;

            var colorBrightness = 1f;

            var logger = new Action<string>(msg =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Output.Text += msg + Environment.NewLine;

                    Output.CaretIndex = Output.Text.Length;
                    Output.ScrollToEnd();
                }, System.Windows.Threading.DispatcherPriority.Background);
            });

            var scene = new Scene(logger, eye, lookAt, fieldOfView);
            scene.Spheres.Add(new Sphere("a", new Vector3(-1001, 0, 0), 1000f, Colors.Red.ChangIntensity(colorBrightness), isWall: true));
            scene.Spheres.Add(new Sphere("b", new Vector3(1001, 0, 0), 1000f, Colors.Blue.ChangIntensity(colorBrightness), isWall: true));
            scene.Spheres.Add(new Sphere("c", new Vector3(0, 0, 1001), 1000f, Colors.White.ChangIntensity(colorBrightness), isWall: true));
            scene.Spheres.Add(new Sphere("d", new Vector3(0, -1001, 0), 1000f, Colors.White.ChangIntensity(colorBrightness), isWall: true));
            scene.Spheres.Add(new Sphere("e", new Vector3(0, 1001, 0), 1000f, Colors.White.ChangIntensity(colorBrightness), isWall: true));

            if (!lotsOfSpheres)
            {
                ITexture procTexture = null;
                if (proceduralTexture)
                    procTexture = new CheckerProceduralTexture();

                ITexture bmpTexture = null;
                if (bitmapTexture)
                    bmpTexture = new BitmapTexture(@"Resources\arroway.de_tiles-29_d100.jpg", BitmapTextureMode.PlanarProjection);

                scene.Spheres.Add(new Sphere("f", new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Colors.Yellow.ChangIntensity(colorBrightness), procTexture));
                scene.Spheres.Add(new Sphere("g", new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Colors.LightCyan.ChangIntensity(colorBrightness), bmpTexture));
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

            if (pathTracing)
            {
                if (!multipleLightSources)
                {
                    scene.Spheres.Add(new Sphere("w", new Vector3(0, -6.99f, 0), 6f, Colors.White.ChangIntensity(colorBrightness), isEmissive: true));
                }
                else
                {
                    // ToDo
                }
            }
            else
            {
                if (!multipleLightSources)
                {
                    // 1 Light Source
                    scene.LightSources.Add(new LightSource("w", new Vector3(0, -0.9f, 0), (coloredLight ? Colors.LightSalmon : Colors.White).ChangIntensity(colorBrightness)));
                }
                else
                {
                    // 3 Light Sources
                    scene.LightSources.Add(new LightSource("c", new Vector3(0.5f, -0.9f, 0.3f), (coloredLight ? Colors.Cyan.ChangIntensity(0.5f) : Colors.White.ChangIntensity(0.5f)).ChangIntensity(colorBrightness)));
                    scene.LightSources.Add(new LightSource("m", new Vector3(-0.5f, -0.9f, 0.3f), (coloredLight ? Colors.Magenta.ChangIntensity(0.5f) : Colors.White.ChangIntensity(0.5f)).ChangIntensity(colorBrightness)));
                    scene.LightSources.Add(new LightSource("y", new Vector3(0, -0.9f, -0.6f), (coloredLight ? Colors.Yellow.ChangIntensity(0.5f) : Colors.White.ChangIntensity(0.5f)).ChangIntensity(colorBrightness)));
                }
            }

            SetControlsEnabled(false);

            scene.AntiAliasing = bool.Parse(AntiAliasing.Text);
            scene.AntiAliasingSampleSize = int.Parse(AntiAliasingSampleSize.Text);
            scene.Parallelize = bool.Parse(Parallelize.Text);
            scene.GammaCorrect = bool.Parse(GammaCorrect.Text);
            scene.DiffuseLambert = bool.Parse(DiffuseLambert.Text);
            scene.SpecularPhong = bool.Parse(SpecularPhong.Text);
            scene.SpecularPhongFactor = int.Parse(SpecularPhongFactor.Text);
            scene.Reflection = bool.Parse(Reflection.Text);
            scene.ReflectionBounces = int.Parse(ReflectionBounces.Text);
            scene.Shadows = bool.Parse(Shadows.Text);
            scene.SoftShadows = bool.Parse(SoftShadows.Text);
            scene.SoftShadowFeelers = int.Parse(SoftShadowFeelers.Text);
            scene.AccelerationStructure = bool.Parse(AccelerationStructure.Text);
            scene.PathTracing = bool.Parse(PathTracing.Text);
            scene.PathTracingRays = int.Parse(PathTracingRays.Text);
            scene.PathTracingMaxBounces = int.Parse(PathTracingMaxBounces.Text);

            Image.Source = null;
            Output.Text = "";
            Status.Text = "Status: Running";
            
            var width = 600;
            var height = 600;
            var dpiScale = VisualTreeHelper.GetDpi(this);

            _cts = new CancellationTokenSource();

            var imageFileName = await Task.Factory.StartNew(() => scene.GetImageFileName(width, height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY, _cts.Token), TaskCreationOptions.LongRunning);
            if (!string.IsNullOrEmpty(imageFileName))
            {
                Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imageFileName));
            }
            
            SetControlsEnabled(true);

            await Task.Delay(TimeSpan.FromSeconds(5));

            // Question: Why is there a gap on the right side?
            // Question: Why are the edge of my spheres black?
            // Question: Why do my reflections have more depth?
            // Question: You didn't gamma correct in your examples? 
            // Question: Some shadows are not overlapped? Why is that? For e.g. right down corner?
        }

        private void SetControlsEnabled(bool enabled)
        {
            AntiAliasing.IsEnabled = enabled;
            AntiAliasingSampleSize.IsEnabled = enabled;
            Parallelize.IsEnabled = enabled;
            GammaCorrect.IsEnabled = enabled;
            DiffuseLambert.IsEnabled = enabled;
            SpecularPhong.IsEnabled = enabled;
            SpecularPhongFactor.IsEnabled = enabled;
            Reflection.IsEnabled = enabled;
            ReflectionBounces.IsEnabled = enabled;
            Shadows.IsEnabled = enabled;
            SoftShadows.IsEnabled = enabled;
            SoftShadowFeelers.IsEnabled = enabled;
            AccelerationStructure.IsEnabled = enabled;
            PathTracing.IsEnabled = enabled;
            PathTracingRays.IsEnabled = enabled;
            PathTracingMaxBounces.IsEnabled = enabled;
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
