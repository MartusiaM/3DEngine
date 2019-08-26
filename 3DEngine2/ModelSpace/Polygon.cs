using MathNet.Numerics.LinearAlgebra;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DEngine.ModelSpace
{
    //pojedyncza sciana modelu
    public class Polygon
    {
        public int indVertex1;
        public int indVertex2;
        public int indVertex3;

        public Vector<float> normal; // wektor normalny sciany

        public System.Windows.Media.Color color;

        public Polygon(int a,int b, int c, Vector<float> normal)
        {
            indVertex1 = a;
            indVertex2 = b;
            indVertex3 = c;
            this.normal = normal.Clone();
            color = System.Windows.Media.Color.FromRgb(255, 255, 255);
        }

        public void colorChange(System.Windows.Media.Color c)
        {
            this.color = c;
        }
    }
}
