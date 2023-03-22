using System;
using System.Collections.Generic;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class CreateSupport : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Support class.
        /// </summary>
        public CreateSupport()
          : base("CreateSupport", "sups.",
              "Creates supports at given point, with given conditions. ",
              "Masters", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "pt", "Add support point here", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Tx", "Tx", "Is the support fixed for Tx?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Tz", "Tz", "Is the support fixed for Tz?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Ry", "Ry", "Is the support fixed for Ry?", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Supports", "Sups.", "Single support point for the construction", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)

        {
            Point3d supPt = new Point3d();
            var tx = false;
            var tz = false;
            var ry = false;

            DA.GetData(0, ref supPt);
            DA.GetData(1, ref tx);
            DA.GetData(2, ref tz);
            DA.GetData(3, ref ry);

            List<Support> supportList = new List<Support>();
            Support support = new Support(supPt, tx, tz, ry);
            supportList.Add(support);

            DA.SetDataList(0,supportList);
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
                return Resources.supp;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("276750FA-AB18-4130-8FAB-A88531394EDA"); }
        }
    }
}