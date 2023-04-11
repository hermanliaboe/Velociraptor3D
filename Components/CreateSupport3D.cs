using System;
using System.Collections.Generic;
using FEM3D.Classes;
using FEM3D.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM3D.Components
{
    public class CreateSupport3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateSupport3D class.
        /// </summary>
        public CreateSupport3D()
          : base("CreateSupport", "sups.",
              "Creates supports at given point, with given conditions. ",
              "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "pt", "Add support point here", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Tx", "Tx", "Is the support fixed for Tx?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Ty", "Ty", "Is the support fixed for Ty?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Tz", "Tz", "Is the support fixed for Tz?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Rx", "Rx", "Is the support fixed for Rx?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Ry", "Ry", "Is the support fixed for Ry?", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Rz", "Rz", "Is the support fixed for Rz?", GH_ParamAccess.item, true);
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
            var ty = false;
            var rx = false;
            var rz = false;

            DA.GetData(0, ref supPt);
            DA.GetData(1, ref tx);
            DA.GetData(2, ref ty);
            DA.GetData(3, ref tz);
            DA.GetData(4, ref rx);
            DA.GetData(5, ref ry);
            DA.GetData(6, ref rz);

            List<Support> supportList = new List<Support>();
            Support support = new Support(supPt, tx, ty, tz, rx, ry, rz);
            supportList.Add(support);

            DA.SetDataList(0, supportList);
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
            get { return new Guid("CC6076C0-52DC-43C4-82D8-3545689F6D13"); }
        }
    }
}