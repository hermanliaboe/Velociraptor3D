using System;
using System.Collections.Generic;
using LA = MathNet.Numerics.LinearAlgebra;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEM3D.Classes;

namespace FEM3D.Components
{
    public class TimeHistory3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TimeHistory class.
        /// </summary>
        public TimeHistory3D()
          : base("TimeHistory", "Nickname",
              "Description",
              "Masters3D", "FEM3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Displacements", "", "Displacements output from SolverDynamic", GH_ParamAccess.item);
            pManager.AddGenericParameter("Node", "", "Node of interest", GH_ParamAccess.item);
            pManager.AddIntegerParameter("DOF", "", "Degree of freedom of interest. x=0, y=1, z=2, rx=3, ry=4, rz=5", GH_ParamAccess.item, 2);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Displacements", "", "Displacements for chosen DOF.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var displacementsDOF = LA.Matrix<double>.Build.Dense(0, 0);
            var nodePlot = new Node();
            int dof = 0;
            DA.GetData(0, ref displacementsDOF);
            DA.GetData(1, ref nodePlot);
            DA.GetData(2, ref dof);

            List<double> displacementsPlot = new List<double>();

            for (int i = 0; i < displacementsDOF.ColumnCount; i++)
            {
                displacementsPlot.Add(displacementsDOF[nodePlot.GlobalID * 6 + dof, i]);
            }

            DA.SetDataList(0, displacementsPlot);
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("844B02A4-4A7B-41CD-BF52-FA911D29BDD2"); }
        }
    }
}