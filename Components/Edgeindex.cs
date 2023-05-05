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
    public class Edgeindex : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the _3Dcomp class.
        /// </summary>
        public Edgeindex()
          : base("3Dcomp", "Nickname",
              "Creation of edge indexes for the ML",
              "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Beams", "beams", "Input for all beams", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("EdgeIndex List", "Edges", "All the edge indexes for ML", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BeamElement> beams = new List<BeamElement>();
            DA.GetDataList(0, beams);

            List<string> edges = new List<string>();

            foreach (BeamElement beam in beams) {
                int s = beam.StartNode.GlobalID;
                int e = beam.EndNode.GlobalID;
                string s1 = '(' + s.ToString() + ',' + e.ToString() + ')';
                var s2 = '(' + e.ToString() + ',' + s.ToString() + ')';
                edges.Add(s1);
                edges.Add(s2);
            }

            DA.SetDataList(0, edges);
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
                return Resources.edgeindex;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("83725416-8FAA-477E-929F-5096487B0670"); }
        }
    }
}