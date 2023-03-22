using System;
using System.Collections.Generic;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class AddCrossSection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddCrossSection class.
        /// </summary>
        public AddCrossSection()
          : base("AddCrossSection", "Nickname",
              "Description",
              "Masters", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("height","h","",GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("width", "w", "", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("YoungsModulus", "E", "", GH_ParamAccess.item, 210000);
            pManager.AddNumberParameter("Density", "rho", "", GH_ParamAccess.item, 0.00000785);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("CrossSection", "cs", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double height = 0.0;
            double width = 0.0;
            double youngsMod = 0.0;
            double rho = 0.0;

            DA.GetData(0, ref height);
            DA.GetData(1, ref width);
            DA.GetData(2, ref youngsMod);
            DA.GetData(3, ref rho);
            CrossSection cs = new CrossSection(height, width, youngsMod, rho);
            DA.SetData(0, cs);
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
                return Resources.crosssec;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AA969851-B713-4717-8F39-E12B47BA9C0C"); }
        }
    }
}