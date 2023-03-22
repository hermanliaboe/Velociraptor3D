using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace FEM.Classes
{
    internal class Node
    {
        public int LocalID;
        public int GlobalID;
        public Point3d Point;
        public bool XBC;
        public bool ZBC;
        public bool RyBC;


        public Node(int localID, Point3d point, int globalID, bool xBC, bool zBC, bool ry) 
        {
            this.LocalID = localID;
            this.Point = point;
            this.GlobalID = globalID;
            this.XBC = xBC;
            this.ZBC = zBC;
            this.RyBC = ry;
        }
        public Node(int localID, int globalID, Point3d point)
        {
            this.LocalID = localID;
            this.GlobalID = globalID;
            this.Point = point;
        }

        public Node() { }

    }
}
