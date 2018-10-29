using System.Numerics;
using System.Windows.Media;

namespace Comgr.CourseProject.Lib
{
    public class SceneB
    {
        private Triangle[] _triangles;

        public SceneB(Triangle[] triangles)
        {
            _triangles = triangles;
        }
        
        public ImageSource GetImage(int width, int height, double dpiX, double dpiY)
        {
            var bitmap = new BitmapImage(width, height, dpiX, dpiY);
                        
            for (int x = 0; x < width; x++)
            {                
                for (int y = 0; y < height; y++)
                {
                    var rgb = GetColor((float)x, (float)y);

                    var c = Conversions.FromRGB(rgb, gammaCorrection: false);
                    bitmap.Set(x, y, c);
                }
            }

            return bitmap.GetImageSource();
        }                

        public Vector3 GetColor(float x, float y)
        {
            var rgb = Vector3.Zero;

            for (int i = 0; i < _triangles.Length; i++)
            {
                var triangle = _triangles[i];
                rgb += triangle.CalcColor(x, y);
            }

            return rgb;
        }
    }
}
 