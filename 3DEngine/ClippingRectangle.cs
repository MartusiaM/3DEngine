using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using _3DEngine;
using System.Windows.Media;

namespace _2DInterface
{

    class ClippingRectangle
    {

    }

    struct Code
    {
        //kod bitowy
        public uint all;
        public uint leftB;
        public uint rightB;
        public uint topB;
        public uint bottomB;

        public static bool operator==(Code a, Code b)
        {
            if (a.all == b.all && a.leftB==b.leftB && a.rightB==b.rightB && a.topB==b.topB && a.bottomB==b.bottomB) return true;
            else return false;
        }
        public static bool operator !=(Code a, Code b)
        {
            return !(a == b);
        }

        public override bool Equals(Object a)
        {
            return this == (Code)a;
        }

    }
}
