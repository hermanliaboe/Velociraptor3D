using System;
using System.Collections.Generic;
using FEM3D.Classes;
using FEM3D.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM3D.Components
{
    public class CreateLoad3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateLoad3D class.
        /// </summary>
        public CreateLoad3D()
          : base("CreateLoad", "Nickname",
              "Creates a load at given point with given load vector.",
              "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            Vector3d nullVec = new Vector3d(0.0, 0.0, 0.0);
            pManager.AddPointParameter("Point", "pt", "Attact point for force vector", GH_ParamAccess.item);
            pManager.AddVectorParameter("Force Vec", "Fvec", "Vector to decribe sice and angle of force", GH_ParamAccess.item, nullVec);
            pManager.AddVectorParameter("Moment Vec", "Mvec", "Vector to decribe sice and rotation of moment", GH_ParamAccess.item, nullVec);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Loads", "Loads", "List of loads", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d loadPt = new Point3d();
            Vector3d forceVec = new Vector3d();
            Vector3d momentVec = new Vector3d();

            Vector3d nullVec = new Vector3d(0.0, 0.0, 0.0);

            DA.GetData(0, ref loadPt);
            DA.GetData(1, ref forceVec);
            DA.GetData(2, ref momentVec);


            List<Load> loadList = new List<Load>();
            Load load = new Load(loadPt, forceVec, momentVec);
            loadList.Add(load);

            DA.SetDataList(0, loadList);


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
                return Resources.Load__1_;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0E4B592E-E29C-409D-BDA6-F303872DAEC2"); }
        }
    }
}