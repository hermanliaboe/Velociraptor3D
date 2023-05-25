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
            pManager.AddGenericParameter("Global K", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Ksup", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Force Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements Vec", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements List", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Displacements Node z", "", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("New Lines", "lines", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Disp Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Beam Forces", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Forces List, K*u", "", "", GH_ParamAccess.list );
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
            var cholesky = globalKsup.Cholesky();
            
            // ROUND K
            globalKsup = matrices.Round(globalKsup, 15);
            LA.Matrix<double> displacements = cholesky.Solve(forceVec);
            // LA.Matrix<double> displacements = globalKsup.Solve(forceVec);
            displacements = matrices.Round(displacements, 15);
            // round k to 5
            var reactions = globalK.Multiply(displacements);
            LA.Matrix<double> nodalForces = globalK.Multiply(displacements);

            GetBeamForces(displacements, elements, out LA.Matrix<double> beamForces);

            List<string> dispList = new List<string>();

            for (int i = 0; i < dof; i = i + 6)
            {
                var nodeDisp = "{" + Math.Round(displacements[i, 0], 6) + ", " + 
                    Math.Round(displacements[i + 1, 0], 6) + ", " + 
                    Math.Round(displacements[i + 2, 0], 6) + ", " + 
                    Math.Round(displacements[i + 3, 0], 6) + ", " + 
                    Math.Round(displacements[i + 4, 0], 6) + ", " + 
                    Math.Round(displacements[i + 5, 0], 6) + "}";
                dispList.Add(nodeDisp);
            }

            List<string> reactionsList = new List<string>();

            for (int i = 0; i < dof; i = i + 6)
            {
                var nodalForcesGlob = "{" + reactions[i, 0] + ", " + reactions[i + 1, 0] + ", " + reactions[i + 2, 0] + ", " +
                    reactions[i + 3, 0] + ", " + reactions[i + 4, 0] + ", " + reactions[i + 5, 0] + "}";
                reactionsList.Add(nodalForcesGlob);
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
            DA.SetData(0, CreateRhinoMatrix(globalK));
            DA.SetData(1, CreateRhinoMatrix(globalKsup));
            DA.SetData(2, forceVec);
            DA.SetData(3, displacements);
            DA.SetDataList(4, dispList);
            DA.SetDataList(5, dispNode);
            DA.SetDataList(6, lineList1);
            DA.SetData(7, dispMatrix);
            DA.SetData(8, beamForces);
            DA.SetDataList(9, reactionsList);
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
                
                List<Point3d> pts = new List<Point3d>() { sP, eP };
                NurbsCurve nc = NurbsCurve.CreateHSpline(pts, sV1, sV2);
                linelist3.Add(nc);
            }
            lineList = linelist3;
        }

        public Rhino.Geometry.Matrix CreateRhinoMatrix(LA.Matrix<double> matrix)
        {
            Rhino.Geometry.Matrix rhinoMatrix = new Rhino.Geometry.Matrix(matrix.RowCount, matrix.ColumnCount);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    rhinoMatrix[i, j] = matrix[i, j];
                }
            }
            return rhinoMatrix;
        }

        void GetBeamForces(LA.Matrix<double> displacements, List<BeamElement> elements, out LA.Matrix<double> beamForces0)
        {

            int dof = 12;
            int i = 6;
            int j = 0;
            LA.Matrix<double> beamForceMat = LA.Matrix<double>.Build.Dense(dof, elements.Count);

            foreach (BeamElement beam in elements)
            {
                LA.Matrix<double> globalElementDisplacements = LA.Matrix<double>.Build.Dense(dof, 1);

                int startId = beam.StartNode.GlobalID;
                globalElementDisplacements[0, 0] = displacements[startId * i,     0];
                globalElementDisplacements[1, 0] = displacements[startId * i + 1, 0];
                globalElementDisplacements[2, 0] = displacements[startId * i + 2, 0];
                globalElementDisplacements[3, 0] = displacements[startId * i + 3, 0];
                globalElementDisplacements[4, 0] = displacements[startId * i + 4, 0];
                globalElementDisplacements[5, 0] = displacements[startId * i + 5, 0];

                int endId = beam.EndNode.GlobalID;
                globalElementDisplacements[6, 0] = displacements[endId * i,     0];
                globalElementDisplacements[7, 0] = displacements[endId * i + 1, 0];
                globalElementDisplacements[8, 0] = displacements[endId * i + 2, 0];
                globalElementDisplacements[9, 0] = displacements[endId * i + 3, 0];
                globalElementDisplacements[10, 0] = displacements[endId * i + 4, 0];
                globalElementDisplacements[11, 0] = displacements[endId * i + 5, 0];

                Matrices mat = new Matrices();
                LA.Matrix<double> t = mat.TransformationMatrix(beam);
              
                var localElementDisplacements = mat.Round(t*globalElementDisplacements, 15);
                LA.Matrix<double> bf = beam.kel*localElementDisplacements;
                beam.ForceList = mat.GetForceList(bf);
                beam.SetLocalDisplacementList(localElementDisplacements);
                beam.SetGlobalDisplacementList(globalElementDisplacements);
                beamForceMat.SetSubMatrix(0, dof, j, 1, bf);
                j++;
            }

            beamForces0 = beamForceMat;

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