using Comgr.CourseProject.Lib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
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
                    await ShowCornellBox();
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
            else
            {
                Image.Source = null;
                Output.Text = null;
            }
        }
        
        private void ShowGradient()
        {
            // ** Gradient Green to Red **
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var gradient = new GradientRectangle(Conversions.FromRGB(0, 1.0f, 0), Conversions.FromRGB(1.0f, 0, 0));
            Image.Source = gradient.GetBitmap((int)this.Width, (int)this.Height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY);
        }

        private async Task ShowCornellBox()
        {
            var eye = new Vector3(0, 0, -4);
            var lookAt = new Vector3(0, 0, 6);
            var fieldOfView = 36f;

            var logger = new Action<string>(msg =>
            {
                Dispatcher.InvokeAsync(() =>
                {
                    Output.Text += msg + Environment.NewLine;

                    Output.CaretIndex = Output.Text.Length;
                    Output.ScrollToEnd();
                }, System.Windows.Threading.DispatcherPriority.Background);
            });

            var spheres = new List<Sphere>();
            var lightSources = new List<LightSource>();

            spheres.Add(new Sphere("a", new Vector3(-1001, 0, 0), 1000f, Colors.Red, isWall: true, brightness: 0));
            spheres.Add(new Sphere("b", new Vector3(1001, 0, 0), 1000f, Colors.Blue, isWall: true, brightness: 0));
            spheres.Add(new Sphere("c", new Vector3(0, 0, 1001), 1000f, Colors.White, isWall: true, brightness: 0));
            spheres.Add(new Sphere("d", new Vector3(0, -1001, 0), 1000f, Colors.White, isWall: true, brightness: 0));
            spheres.Add(new Sphere("e", new Vector3(0, 1001, 0), 1000f, Colors.White, isWall: true, brightness: 0));

            var lotsOfSpheres = bool.Parse(LotsOfSpheres.Text);
            var proceduralTexture = bool.Parse(ProceduralTexture.Text);
            var bitmapTexture = bool.Parse(BitmapTexture.Text);
            var multipleLightSources = bool.Parse(MultipleLightSources.Text);
            var coloredLight = bool.Parse(ColoredLight.Text);

            if (!lotsOfSpheres)
            {
                ITexture procTexture = null;
                if (proceduralTexture)
                    procTexture = new CheckerProceduralTexture();

                ITexture bmpTexture = null;
                if (bitmapTexture)
                    bmpTexture = new BitmapTexture(@"Resources\arroway.de_tiles-29_d100.jpg", BitmapTextureMode.PlanarProjection);
                
                spheres.Add(new Sphere("f", new Vector3(-0.6f, 0.7f, -0.6f), 0.3f, Colors.Yellow, texture: procTexture, brightness: 0));
                spheres.Add(new Sphere("g", new Vector3(0.3f, 0.4f, 0.3f), 0.6f, Colors.LightCyan, texture: bmpTexture, brightness: bitmapTexture ? 1 : 0));
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

                    spheres.Add(new Sphere($"gen{i}", new Vector3(x, y, z), r, c));
                }
            }

            if (bool.Parse(PathTracing.Text))
            {
                var lightBrightness = float.Parse(PathTracingLightBrightness.Text);

                if (!multipleLightSources)
                {
                    spheres.Add(new Sphere("w", new Vector3(0, -10.99f, 0), 10f, Colors.White, brightness: lightBrightness, reflectiveness: 1f));
                }
                else
                {
                    spheres.Add(new Sphere("c", new Vector3(0.5f, -6.99f, 0.3f), 6f, (coloredLight ? Colors.Cyan : Colors.White), brightness: lightBrightness / 3, reflectiveness: 1f));
                    spheres.Add(new Sphere("m", new Vector3(-0.5f, -6.99f, 0.3f), 6f, (coloredLight ? Colors.Magenta : Colors.White), brightness: lightBrightness / 3, reflectiveness: 1f));
                    spheres.Add(new Sphere("y", new Vector3(0, -6.99f, -0.6f), 6f, (coloredLight ? Colors.Yellow : Colors.White), brightness: lightBrightness / 3, reflectiveness: 1f));
                }
            }
            else
            {
                if (!multipleLightSources)
                {
                    // 1 Light Source
                    lightSources.Add(new LightSource("w", new Vector3(0, -0.9f, 0), (coloredLight ? Colors.LightSalmon : Colors.White)));
                }
                else
                {
                    // 3 Light Sources
                    lightSources.Add(new LightSource("c", new Vector3(0.5f, -0.9f, 0.3f), (coloredLight ? Colors.Cyan : Colors.White).ChangIntensity(0.5f)));
                    lightSources.Add(new LightSource("m", new Vector3(-0.5f, -0.9f, 0.3f), (coloredLight ? Colors.Magenta : Colors.White).ChangIntensity(0.5f)));
                    lightSources.Add(new LightSource("y", new Vector3(0, -0.9f, -0.6f), (coloredLight ? Colors.Yellow : Colors.White).ChangIntensity(0.5f)));
                }
            }

            SetControlsEnabled(false);
                        
            var scene = new SceneA(logger, eye, lookAt, fieldOfView, spheres.ToArray(), lightSources.ToArray());

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
            
            var width = (int)Image.Width;
            var height = (int)Image.Height;
            var dpiScale = VisualTreeHelper.GetDpi(this);

            _cts = new CancellationTokenSource();

            var exportDirectory = Path.GetFullPath(@".\RayTracingResults\");
            var settingsSummary = GetSettingsSummary();

            var imageFileName = await Task.Factory.StartNew(() => scene.GetImageFileName(width, height, dpiScale.PixelsPerInchX, dpiScale.PixelsPerInchY, _cts.Token, settingsSummary, exportDirectory), TaskCreationOptions.LongRunning);
            if (!string.IsNullOrEmpty(imageFileName))
            {
                Image.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imageFileName));
            }
            
            SetControlsEnabled(true);
        }

        private void SetControlsEnabled(bool enabled)
        {
            MultipleLightSources.IsEnabled = enabled;
            ColoredLight.IsEnabled = enabled;
            LotsOfSpheres.IsEnabled = enabled;
            ProceduralTexture.IsEnabled = enabled;
            BitmapTexture.IsEnabled = enabled;
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
            PathTracingLightBrightness.IsEnabled = enabled;
        }

        private string GetSettingsSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{nameof(MultipleLightSources)}: {MultipleLightSources.Text}");
            sb.AppendLine($"{nameof(ColoredLight)}: {ColoredLight.Text}");
            sb.AppendLine($"{nameof(LotsOfSpheres)}: {LotsOfSpheres.Text}");
            sb.AppendLine($"{nameof(ProceduralTexture)}: {ProceduralTexture.Text}");
            sb.AppendLine($"{nameof(BitmapTexture)}: {BitmapTexture.Text}");
            sb.AppendLine($"{nameof(AntiAliasing)}: {AntiAliasing.Text}");
            sb.AppendLine($"{nameof(AntiAliasingSampleSize)}: {AntiAliasingSampleSize.Text}");
            sb.AppendLine($"{nameof(Parallelize)}: {Parallelize.Text}");
            sb.AppendLine($"{nameof(GammaCorrect)}: {GammaCorrect.Text}");
            sb.AppendLine($"{nameof(DiffuseLambert)}: {DiffuseLambert.Text}");
            sb.AppendLine($"{nameof(SpecularPhong)}: {SpecularPhong.Text}");
            sb.AppendLine($"{nameof(SpecularPhongFactor)}: {SpecularPhongFactor.Text}");
            sb.AppendLine($"{nameof(Reflection)}: {Reflection.Text}");
            sb.AppendLine($"{nameof(ReflectionBounces)}: {ReflectionBounces.Text}");
            sb.AppendLine($"{nameof(Shadows)}: {Shadows.Text}");
            sb.AppendLine($"{nameof(SoftShadows)}: {SoftShadows.Text}");
            sb.AppendLine($"{nameof(SoftShadowFeelers)}: {SoftShadowFeelers.Text}");
            sb.AppendLine($"{nameof(AccelerationStructure)}: {AccelerationStructure.Text}");
            sb.AppendLine($"{nameof(PathTracing)}: {PathTracing.Text}");
            sb.AppendLine($"{nameof(PathTracingRays)}: {PathTracingRays.Text}");
            sb.AppendLine($"{nameof(PathTracingMaxBounces)}: {PathTracingMaxBounces.Text}");
            sb.AppendLine($"{nameof(PathTracingLightBrightness)}: {PathTracingLightBrightness.Text}");

            return sb.ToString();
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
