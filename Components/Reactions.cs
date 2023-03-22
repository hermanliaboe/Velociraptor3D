using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using MathNet.Numerics.LinearAlgebra.Double;
using Rhino.Geometry;

namespace FEM.Components
{
    public class Reactions : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Reactions()
          : base("MyComponent1", "Nickname",
              "Description",
              "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("displacements", "disp", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Normal forces", "N", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Shear forces", "V", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Moment forces", "M", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {


            DenseMatrix displacments = new DenseMatrix(1);
            DA.GetData(0,ref displacments);

            int dof = displacments.RowCount / 3;














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
            get { return new Guid("8FDB900D-1FE9-410E-8B56-1E0CB7F25EC0"); }
        }
    }
}