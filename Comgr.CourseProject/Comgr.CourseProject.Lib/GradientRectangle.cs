using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Comgr.CourseProject.Lib
{
    public class GradientRectangle
    {
        private Color _fromColor;
        private Color _toColor;

        public GradientRectangle(Color fromColor, Color toColor)
        {
            _fromColor = fromColor;
            _toColor = toColor;
        }

        public WriteableBitmap GetBitmap(int width, int height, double dpiX, double dpiY)
        {
            var fromRGB = Conversions.FromColor(_fromColor);
            var toRGB = Conversions.FromColor(_toColor);

            // 4 channels (blue, green, red, alpha). Order of channels is important!
            int channels = 4;

            byte[] pixels = new byte[width * height * channels];
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var rgb = Vector3.Lerp(fromRGB, toRGB, i / (float)width);
                    var color = Conversions.FromRGB(rgb, gammaCorrection: j < (height / 2));

                    color.Clamp();

                    var pos = (j * width * channels) + (i * channels);

                    // blue channel
                    pixels[pos + 0] = color.B;

                    // green channel
                    pixels[pos + 1] = color.G;

                    // red channel
                    pixels[pos + 2] = color.R;

                    // alpha channel
                    pixels[pos + 3] = byte.MaxValue;
                }
            }

            var bitmap = new WriteableBitmap(width, height, dpiX, dpiY, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * channels, 0);

            return bitmap;
        }
    }
}
