using System;
using System.Collections.Generic;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components.Deconstructors
{
    public class DeconstructNode : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructNode class.
        /// </summary>
        public DeconstructNode()
          : base("DeconstructNode", "Nickname",
              "Deconstructs Node object.",
              "Masters", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Node", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Point", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("localID", "", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("globalID", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Tx", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Tz", "", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ry", "", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Node node = new Node();
            DA.GetData(0, ref node);

            DA.SetData(0, node.Point);
            DA.SetData(1, node.LocalID);
            DA.SetData(2, node.GlobalID);
            DA.SetData(3, node.XBC);
            DA.SetData(4, node.ZBC);
            DA.SetData(5, node.RyBC);
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
                return Resources.nodedec__1_;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("32C046DD-08A6-47DF-809A-9BAC097ADEF4"); }
        }
    }
}