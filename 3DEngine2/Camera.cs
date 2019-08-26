using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;
using SharpDX;
using SharpDX.Mathematics;
using MathNet.Numerics.LinearAlgebra;

namespace _3DEngine
{
    public class Camera
    {
        public Vector<float> Position;
        public Vector<float> Target;
    }
}
