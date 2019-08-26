using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using SharpDX;
using _3DEngine.ModelSpace;
using MathNet.Numerics.LinearAlgebra;

namespace _3DEngine
{
    public class Scene : IRaster
    {
        public WriteableBitmap scene; //bitmapa na ktorej odrysowujemy
        public int width, height; //wymiary bitmapy

        public System.Windows.Media.Color sceneColor;
        public System.Windows.Media.Color elementColor;

        public Matrix<float> projectionMatrix; //macierz projekcji
        private byte[] scenePixels;//kolory pikseli bitmapy
        private readonly float[] zBuffer; //wartosci z pikseli

        private object[] lockBuffer;//zrownoleglenie

        public Scene(int w, int h, System.Windows.Media.Color c)
        {
            sceneColor = c;
            elementColor = System.Windows.Media.Color.FromArgb(255, 255, 255, 255);
            scene = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            scenePixels = new byte[w * h * 4];
            zBuffer = new float[w * h];
            width = w;
            height = h;

            //macierz projekcji
            float e = (float)(1.0 / Math.Tan(90 * Math.PI / 180.0 / 2));
            float a = h / w;
            float f = 100, n = 0.1F;
            float[,] values = new float[,] { { e, 0, 0, 0 }, { 0, e / a, 0, 0 }, { 0, 0,
                -(f + n) / (f - n), -2 * f * n / (f - n) } , { 0, 0, -1, 0 } };

            projectionMatrix = Matrix<float>.Build.DenseOfArray(values);

            //lock
            lockBuffer = new object[w * h];
            for (var i = 0; i < lockBuffer.Length; i++)
            {
                lockBuffer[i] = new object();
            }
        }

        public void setPixel(int x, int y, float z, System.Windows.Media.Color c)
        {
            if (x < width && y < height && x >= 0 && y >= 0)
            {
                var ind = x + y * width;
                var pixInd = ind * 4;

                lock (lockBuffer[ind])
                {

                    if (zBuffer[ind] >= z)
                    {
                        zBuffer[ind] = z;

                        scenePixels[pixInd] = (byte)(c.B);
                        scenePixels[pixInd + 1] = (byte)(c.G);
                        scenePixels[pixInd + 2] = (byte)(c.R);
                        scenePixels[pixInd + 3] = (byte)255;

                    }
                }
            }

        }

        public void clear()
        {
            //czyszczenie tablicy pikseli
            for (var i = 0; i < scenePixels.Length; i += 4)
            {
                scenePixels[i] = 0;
                scenePixels[i + 1] = 0;
                scenePixels[i + 2] = 0;
                scenePixels[i + 3] = 255;
            }

            //czyszczenie z-buffora
            for (var i = 0; i < zBuffer.Length; i++)
            {
                zBuffer[i] = float.MaxValue;
            }

        }

        public void swapBuffer()
        {
            //zrzucanie backbuffora (tablicy pikseli) na bitmape
            int widthInBytes = 4 * width;
            scene.WritePixels(new Int32Rect(0, 0, width, height), scenePixels, widthInBytes, 0);
        }
        
    }
}
