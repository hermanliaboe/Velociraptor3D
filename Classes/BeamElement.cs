using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LA = MathNet.Numerics.LinearAlgebra;

namespace FEM3D.Classes
{
    internal class BeamElement
    {
        public string Type;
        public int Id;
        public int ElDof;
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
        public LA.Matrix<double> kel;
        public List<double> ForceList;
        public List<double> LocalDisp;
        public List<double> GlobalDisp;
        public double Alpha;
        public Vector3d xl;
        public Vector3d yl;
        public Vector3d zl;
        public double Iy;
        public double Iz;
        public double J;
        public double A;
        public LA.Matrix<double> T;
        public List<double> localForceErrors;

        
        public BeamElement() { }

        public BeamElement(int id, Line line)
        {
            this.Id = id;
            this.Line = line;
            this.Length = GetElementLength(line);
           
        }

        public void SetLocalDisplacementList(LA.Matrix<double> disp)
        {
            List<double> dispList = new List<double>();
            for (int i = 0; i < disp.RowCount; i++)
            {
                dispList.Add(disp[i, 0]);
            }
            this.LocalDisp = dispList;
        }

        public void SetGlobalDisplacementList(LA.Matrix<double> disp)
        {
            List<double> dispList = new List<double>();
            for (int i = 0; i < disp.RowCount; i++)
            {
                dispList.Add(disp[i, 0]);
            }
            this.GlobalDisp = dispList;
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

            double l = Math.Round((Math.Sqrt(Math.Pow(z2 - z1, 2.0) + Math.Pow(x2 - x1, 2.0) + Math.Pow(y2 - y1, 2.0))), 9);

            return l;
        }
    }
        
}
