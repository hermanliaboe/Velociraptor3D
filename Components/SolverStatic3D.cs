using FEM3D.Classes;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;

using LA = MathNet.Numerics.LinearAlgebra;
using Rhino.Commands;
using Rhino.Render;
using System.IO;
using Grasshopper.Kernel.Types;
using FEM3D.Properties;
using Grasshopper.GUI;
using MathNet.Numerics.Interpolation;
using Grasshopper.Kernel.Geometry;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System.Numerics;

namespace FEM3D.Components
{
    public class SolverStatic3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SolverStatic3D class.
        /// </summary>
        public SolverStatic3D()
          : base("Static FEM3DSolver", "femmern",
            "FEM3D solver with Newmark method",
            "Masters3D", "FEM3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly", "ass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Scale", "Scale", "", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("item", "item", "item", GH_ParamAccess.item);
            pManager.AddGenericParameter("global K", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("global Ksup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("force Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("displacements Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("displacements List", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("displacements Node z", "", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("new lines", "lines", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Disp matrix", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Classes.Assembly model = new Classes.Assembly();
            double scale = 0.0;

            DA.GetData(0, ref model);
            DA.GetData(1, ref scale);


            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;




            int dof = model.NodeList.Count * 6;


            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildGlobalKsup(dof, globalK, supports, nodes);
            LA.Matrix<double> forceVec = matrices.BuildForceVector(loads, dof);

            LA.Matrix<double> displacements = globalKsup.Solve(forceVec);
            LA.Matrix<double> nodalForces = globalK.Multiply(displacements);


            List<string> dispList = new List<string>();

            for (int i = 0; i < dof; i = i + 6)
            {
                var nodeDisp = "{" + displacements[i, 0] + ", " + displacements[i + 1, 0] + ", " + displacements[i + 2, 0] +
                    displacements[i + 3, 0] + ", " + displacements[i + 4, 0] + ", " + displacements[i + 5, 0] + ", " + "}";
                dispList.Add(nodeDisp);
            }

            Rhino.Geometry.Matrix dispMatrix = new Rhino.Geometry.Matrix(dof/6, 6);
            int count = 0;
            for (int j = 0; j < dof ; j = j + 6)
            {
                dispMatrix[count, 0] = displacements[j,0];
                dispMatrix[count, 1] = displacements[j + 1,0];
                dispMatrix[count, 2] = displacements[j + 2, 0];
                dispMatrix[count, 3] = displacements[j + 3, 0];
                dispMatrix[count, 4] = displacements[j + 4, 0];
                dispMatrix[count, 5] = displacements[j + 5, 0];
                count++;
            }

            List<double> dispNode = new List<double>();
            for (int i = 0; i < dof; i = i + 6)
            {
                var nodeDisp = displacements[i + 2, 0];
                dispNode.Add(nodeDisp);
            }

            Rhino.Geometry.Matrix rhinoMatrix = new Rhino.Geometry.Matrix(dof, dof);
            for (int i = 0; i < globalKsup.RowCount; i++)
            {
                for (int j = 0; j < globalKsup.ColumnCount; j++)
                {
                    rhinoMatrix[i, j] = globalKsup[i, j];
                }
            }

            //List<NurbsCurve> lineList1 = new List<NurbsCurve>();
            getNewGeometry(scale, displacements, elements, out List<NurbsCurve> lineList1);

            //DA.SetData(0, item);
            DA.SetData(1, globalK);
            DA.SetData(2, globalKsup);
            DA.SetData(3, forceVec);
            DA.SetData(4, displacements);
            DA.SetDataList(5, dispList);
            DA.SetDataList(6, dispNode);
            DA.SetDataList(7, lineList1);
            DA.SetData(8, dispMatrix);
        }



        void getNewGeometry(double scale, LA.Matrix<double> displacements, List<BeamElement> beams, out List<NurbsCurve> lineList)
        {
            List<Line> linelist2 = new List<Line>();
            List<NurbsCurve> linelist3 = new List<NurbsCurve>();

            int i = 6;

            foreach (BeamElement beam in beams)
            {
                Vector3d v1 = new Vector3d(0, 0, 0);
                Vector3d v2 = new Vector3d(0, 0, 0);

                int startId = beam.StartNode.GlobalID;
                double X1 = beam.StartNode.Point.X;
                double Y1 = beam.StartNode.Point.Y;
                double Z1 = beam.StartNode.Point.Z;

                int endId = beam.EndNode.GlobalID;
                double X2 = beam.EndNode.Point.X;
                double Y2 = beam.EndNode.Point.Y;
                double Z2 = beam.EndNode.Point.Z;

                double x1 = displacements[startId * i, 0];
                double y1 = displacements[startId * i + 1 , 0];
                double z1 = displacements[startId * i + 2, 0];
                //double r1 = displacements[startId * i + 2, 0];
                Point3d sP = new Point3d(X1 + x1 * scale, Y1 + y1 * scale, Z1 + z1 * scale);

                double x2 = displacements[endId * i, 0];
                double y2 = displacements[endId * i + 1, 0];
                double z2 = displacements[endId * i + 2, 0];
                Point3d eP = new Point3d(X2 + x2 * scale, Y2 + y2 * scale, Z2 + z2 * scale);

                Vector3d yVec = new Vector3d(0, 1, 0);

                Vector3d sV1 = new Vector3d((eP.X - sP.X), eP.Y - sP.Y, eP.Z - sP.Z);
                v1 = v1 + sV1;

                Vector3d sV2 = new Vector3d((eP.X - sP.X), Y2 - Y1, eP.Z - sP.Z);
                v2 = v2 + sV2;
                /*
                if (beam.StartNode.RyBC == true && beam.StartNode.RzBC == true)
                {
                    Vector3d sV1 = new Vector3d((X2 - X1), Y2 - Y1, Z2 - Z1);
                    //scale1 = 0;
                    //sV1.Rotate(r1 * scale1, yVec);
                    v1 = v1 + sV1;
                }
                else if ((beam.StartNode.RyBC == true && beam.StartNode.RzBC == false))
                {
                    Vector3d sV1 = new Vector3d((eP.X - sP.X), eP.Y - sP.Y, eP.Z - sP.Z);
                    //  sV1.Rotate(r1 * scale1, yVec);
                    v1 = v1 + sV1;
                }

                if (beam.EndNode.RyBC == true)
                {
                    Vector3d sV2 = new Vector3d((X2 - X1), Y2 - Y1, Z2 - Z1);
                    //scale2 = 0;
                    //sV2.Rotate(r2 * scale2, yVec);
                    v2 = v2 + sV2;
                }
                else
                {
                    Vector3d sV2 = new Vector3d((eP.X - sP.X), Y2 - Y1, eP.Z - sP.Z);
                    // sV2.Rotate(r2 * scale2, yVec);
                    v2 = v2 + sV2;
                }
                
                */
                List<Point3d> pts = new List<Point3d>() { sP, eP };
                NurbsCurve nc = NurbsCurve.CreateHSpline(pts, sV1, sV2);
                linelist3.Add(nc);
            }
            lineList = linelist3;
        }





        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;

                return Resources.SolverStatic_main_;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("604AA6C5-1676-4BB9-B740-599D5882AEA9"); }
        }
    }
}