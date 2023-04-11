

using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using FEM3D.Classes;
using FEM3D.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM3D.Components
{
    public class AddCrossSection3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddCrossSection3D class.
        /// </summary>
        public AddCrossSection3D()
         : base("AddCrossSection", "Nickname",
             "Description",
             "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("height", "h", "", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("width", "w", "", GH_ParamAccess.item, 1);
            pManager.AddNumberParameter("YoungsModulus", "E", "", GH_ParamAccess.item, 210000);
            pManager.AddNumberParameter("Density", "rho", "", GH_ParamAccess.item, 0.00000785);
            pManager.AddNumberParameter("ShearModulus", "ShearModulus", "", GH_ParamAccess.item, 81000.0);

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
            double shearMod = 0.0;

            DA.GetData(0, ref height);
            DA.GetData(1, ref width);
            DA.GetData(2, ref youngsMod);
            DA.GetData(3, ref rho);
            DA.GetData(4, ref shearMod);
            CrossSection cs = new CrossSection(height, width, youngsMod, rho, shearMod);
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
            get { return new Guid("FAB05AA4-674F-49A3-9CFB-0A83C2E5FD7E"); }
        }
    }
}