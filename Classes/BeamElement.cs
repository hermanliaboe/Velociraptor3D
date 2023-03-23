using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class BeamElement
    {
        public string Type;
        public int Id;
        public int Dof;
        public List<Node> Nodes;
        public Node StartNode;
        public Node EndNode;
        public Line Line;
        public double Length;
        public double Height;
        public double Width;
        public double YoungsMod;
        public double Rho;
        public double ShearMod;

        public BeamElement() { }

        public BeamElement(int id, Line line)
        {
            this.Id = id;
            this.Line = line;
            this.Length= GetElementLength(line);
        }

        public double GetElementLength(Line line)
        {
            //gets length of element
            // is not the line.From the start node??
            Point3d startNode = line.To;
            double z1 = startNode.Z;
            double x1 = startNode.X;
            double y1 = startNode.Y;

            Point3d endNode = line.From;
            double z2 = endNode.Z;
            double x2 = endNode.X;
            double y2 = endNode.Y;

            double l = (Math.Sqrt(Math.Pow(z2 - z1, 2.0) + Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0)));

            return l;
        }



    }
}
