using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Comgr.CourseProject.Lib
{
    public class Bitmap
    {
        // 4 channels (blue, green, red, alpha). Order of channels is important.
        private const int ChannelCount = 4; 

        private Color[,] _pixels;

        private double _dpiX;
        private double _dpiY;

        public Bitmap(int width, int height, double dpiX, double dpiY)
        {
            _pixels = new Color[width, height];
            _dpiX = dpiX;
            _dpiY = dpiY;
        }

        public void Set(int x, int y, Color c) => _pixels[x, y] = c;

        public int Width => _pixels.GetLength(0);

        public int Height => _pixels.GetLength(1);

        public ImageSource GetImage()
        {
            byte[] pixels = new byte[Width * Height * ChannelCount];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    var color = _pixels[i, j];

                    color.Clamp();

                    var pos = (j * Width * ChannelCount) + (i * ChannelCount);

                    // blue channel
                    pixels[pos + 0] = color.B;

                    // green channel
                    pixels[pos + 1] = color.G;

                    // red channel
                    pixels[pos + 2] = color.R;

                    // alpha channel
                    pixels[pos + 3] = color.A;
                }
            }

            var bitmap = new WriteableBitmap(Width, Height, _dpiX, _dpiY, PixelFormats.Bgra32, BitmapPalettes.Halftone256);
            bitmap.WritePixels(new Int32Rect(0, 0, Width, Height), pixels, Width * ChannelCount, 0);

            return bitmap;
        }
    }
}
