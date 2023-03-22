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
        public bool Ry;
        
        public Support() { }

        public Support(Point3d point, bool tx, bool tz, bool ry) 
        {
            this.Point = point;
            this.Tx = tx;
            this.Tz = tz;
            this.Ry = ry;
            

        }
    }
}
