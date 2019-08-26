using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;

namespace _3DEngine
{
    interface IRaster
    {
        void setPixel(int x, int y, float z, Color c);
        void clear();
        void swapBuffer();
    }
}
