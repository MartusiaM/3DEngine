using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace _3DEngine.ModelSpace
{
    public class Point3D
    {
        public Vector<float> dim; //współrzędne wierzchołka

        public Vector<double> texturePoint; // wspolrzedne punktu tekstury wierzcholka

        public Point3D()
        {
            dim = Vector<float>.Build.DenseOfArray(new float[] { 0, 0, 0, 1 });
            texturePoint = Vector<double>.Build.DenseOfArray(new double[] { 1, 1 });
        }
        public Point3D(float a, float b, float c, Vector<double> text)
        {
            dim = Vector<float>.Build.DenseOfArray(new float[] { a, b, c, 1 });
            texturePoint = text.Clone();
        }

    }
}
