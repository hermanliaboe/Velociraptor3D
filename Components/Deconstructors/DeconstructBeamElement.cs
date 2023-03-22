using System;
using System.Collections.Generic;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components.Deconstructors
{
    public class DeconstructBeamElement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructBeamElement class.
        /// </summary>
        public DeconstructBeamElement()
          : base("DeconstructBeamElement", "Nickname",
              "Deconstructs BeamElement object",
              "Masters", "Deconstructors")
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
            pManager.AddNumberParameter("height","h","",GH_ParamAccess.item);
            pManager.AddNumberParameter("width", "w", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("YoungsMod", "E", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Length","L","",GH_ParamAccess.item);
            pManager.AddNumberParameter("Material density", "rho", "", GH_ParamAccess.item);
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
            get { return new Guid("369D319E-10A2-41F0-BA4A-88A2F4D67130"); }
        }
    }
}