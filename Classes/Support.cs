using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM3D.Classes
{
    internal class Support
    {
        public Point3d Point;
        public bool Tx;
        public bool Tz;
        public bool Ty;
        public bool Ry;
        public bool Rx;
        public bool Rz;
        
        public Support() { }

        public Support(Point3d point, bool tx, bool ty, bool tz, bool rx, bool ry, bool rz) 
        {
            this.Point = point;
            this.Tx = tx;
            this.Tz = tz;
            this.Ty = ty;
            this.Ry = ry;
            this.Rx = rx;
            this.Rz = rz;
        }
    }
}
