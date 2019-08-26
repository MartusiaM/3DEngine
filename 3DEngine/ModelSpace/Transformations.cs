using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using MathNet.Numerics.LinearAlgebra;

namespace _3DEngine.ModelSpace
{
    class Transformations
    {
        public Matrix<float> rotateX;
        public Matrix<float> rotateY;
        public Matrix<float> translate;

        public Transformations()
        {
            float [,] table = new float[,]
               { { 1,0,0,0 },
               { 0,1,0,0 },
                { 0,0,1,0 },
                { 0,0,0,1 }  };

            translate = Matrix<float>.Build.DenseOfArray(table);
            rotateX = Matrix<float>.Build.DenseOfArray(table);
            rotateY = Matrix<float>.Build.DenseOfArray(table);

            scale(-10); // odsuniecie obiektu od kamery
        }
        public void scale(float value)
        {
            translate[3, 2] = value;
        }

        public void rotateXAxe(float value)
        {
            //value - kat w radianach
            rotateX[1, 1] = (float)Math.Cos(value);
            rotateX[1, 2] = (float)(-Math.Sin(value));
            rotateX[2, 1] = (float)Math.Sin(value);
            rotateX[2, 2] = (float)Math.Cos(value);
        }

        public void rotateYAxe(double value)
        {
            //value - kat w radianach
            rotateY[0, 0] = (float)Math.Cos(value);
            rotateY[0, 2] = (float)Math.Sin(value);
            rotateY[2, 0] = (float)(-Math.Sin(value));
            rotateY[2, 2] = (float)Math.Cos(value);
        }

    }
}
