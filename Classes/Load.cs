using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class Load
    {
        public Point3d Point;
        public Vector3d ForceVector;
        public Vector3d MomentVector;
        public int NodeID;


        public Load(){ }
    
        public Load(Point3d point, Vector3d fVector, Vector3d mVector) 
        {
            this.Point = point;
            this.ForceVector = fVector;
            this.MomentVector = mVector;
        }
    }
}
