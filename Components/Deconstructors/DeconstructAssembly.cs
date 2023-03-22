using System;
using System.Collections.Generic;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components.Deconstructors
{
    public class DeconstructAssembly : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeconstructAssembly class.
        /// </summary>
        public DeconstructAssembly()
          : base("DeconstructAssembly", "Nickname",
              "Deconstructs Assembly object",
              "Masters", "Deconstructors")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("AssembledModell", "Ass.mod", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("beams", "sNode", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("supports", "sup", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "loads", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes","nodes","",GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Assembly assembly = new Assembly();


            DA.GetData(0, ref assembly);

            DA.SetDataList(0, assembly.BeamList);
            DA.SetDataList(1, assembly.SupportList);
            DA.SetDataList(2, assembly.LoadList);
            DA.SetDataList(3, assembly.NodeList);
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
                return Resources.assemblydec__1_;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("76643FD3-90C4-403B-9F60-BC9EB36A8D3B"); }
        }
    }
}