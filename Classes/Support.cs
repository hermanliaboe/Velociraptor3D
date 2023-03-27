using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
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

        public Support(Point3d point, bool tx, bool tz, bool ty, bool ry, bool Rx, bool Ry) 
        {
            this.Point = point;
            this.Tx = tx;
            this.Tz = tz;
            this.Ty = ty;
            this.Ry = ry;
            this.Rx = Rx;
            this.Rz = Ry;
        }
    }
}
