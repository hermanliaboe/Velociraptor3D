using System;
using System.Collections.Generic;
using FEM3D.Classes;
using FEM3D.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;
using LA = MathNet.Numerics.LinearAlgebra;


namespace FEM3D.Components.Deconstructors
{
    public class DeconstructBeamElement3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructBeamElement3D class.
        /// </summary>
        public DeconstructBeamElement3D()
          : base("DeconstructBeamElement", "Nickname",
              "Deconstructs BeamElement object",
              "Masters3D", "Deconstructors3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("BeamElement", "be", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("startNode", "sNode", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("endNode", "eNode", "", GH_ParamAccess.item);
            pManager.AddLineParameter("line", "l", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("BeamID", "bID", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("height", "h", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("width", "w", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("YoungsMod", "E", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length", "L", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Material density", "rho", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("ShearMod", "J", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Forces", "F", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Local Displacements", "", "List of element displacements in local coordinate system.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Kel", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("xl", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("yl", "", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("zl", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iy", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Iz", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("J", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("A", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("T", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("global displacements", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            BeamElement beam = new BeamElement();
            DA.GetData(0, ref beam);

            DA.SetData(0, beam.StartNode);
            DA.SetData(1, beam.EndNode);
            DA.SetData(2, beam.Line);
            DA.SetData(3, beam.Id);
            DA.SetData(4, beam.Height);
            DA.SetData(5, beam.Width);
            DA.SetData(6, beam.YoungsMod);
            DA.SetData(7, beam.Length);
            DA.SetData(8, beam.Rho);
            DA.SetData(9, beam.ShearMod);
            DA.SetDataList(10, beam.ForceList);
            DA.SetDataList(11, beam.LocalDisp);
            DA.SetData(12, CreateRhinoMatrix(beam.kel));
            DA.SetData(13, beam.xl);
            DA.SetData(14, beam.yl);
            DA.SetData(15, beam.zl);
            DA.SetData(16, beam.Iy);
            DA.SetData(17, beam.Iz);
            DA.SetData(18, beam.J);
            DA.SetData(19, beam.A);
            DA.SetData(20, CreateRhinoMatrix(beam.T));
            DA.SetDataList(21, beam.GlobalDisp);
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.beamdec;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7B8CED19-C55B-4B50-8D8D-E89C739DD7E8"); }
        }
    }
}