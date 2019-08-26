using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace _3DEngine.ModelSpace
{
    public class Texture
    {
        BitmapImage painting;
        private byte[] texture;
        public int width;
        public int height;

        public Texture(BitmapImage bmp)
        {
            painting = bmp;
            width = bmp.PixelWidth;
            height = bmp.PixelHeight;
            var stride = width * ((bmp.Format.BitsPerPixel + 7) / 8);

            texture = new byte[height * stride];
            painting.CopyPixels(texture, stride, 0);
        }

        public Color Map(float x, float y)
        {
            if (texture == null)
            {
                //biala sciana jak nie wczytano tekstury
                return System.Windows.Media.Color.FromRgb(255, 255, 255);
            }

            int u = Math.Abs((int)(x % width));
            int v = Math.Abs((int)(y  % height));

            int pos = (u + v * width) * 4;
            byte b = texture[pos];
            byte g = texture[pos + 1];
            byte r = texture[pos + 2];

            return System.Windows.Media.Color.FromRgb(r, g, b);
        }

    }
}
