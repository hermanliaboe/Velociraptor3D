using System;
using System.Collections.Generic;
using FEM3D.Classes;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM3D.Components
{
    public class SetLocalAxis : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetLocalAxis class.
        /// </summary>
        public SetLocalAxis()
          : base("SetLocalAxis", "Nickname",
              "Set elements local axis",
              "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("xl", "", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("yl", "", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("zl", "", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BeamElement> elements = new List<BeamElement>();
            List<Vector3d> xl = new List<Vector3d>();
            List<Vector3d> yl = new List<Vector3d>();
            List<Vector3d> zl = new List<Vector3d>();

            DA.GetDataList(0, elements);
            DA.GetDataList(1, xl);
            DA.GetDataList(2, yl);
            DA.GetDataList(3, zl);


            for (int i  = 0; i < elements.Count; i++)
            {
                elements[i].xl = xl[i];
                elements[i].yl = yl[i];
                elements[i].zl = zl[i];
            }

            DA.SetDataList(0, elements);
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
            get { return new Guid("5D9EEEA7-D89B-4DF6-9A8F-E2C4CF141EDC"); }
        }
    }
}