using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Xml.Linq;
using FEM3D.Classes;
using FEM3D.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Complex;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Rhino.Geometry;
using LA = MathNet.Numerics.LinearAlgebra;

using System.IO;

namespace FEM3D.Components
{
    public class ToTextFile3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ToTextFile3D class.
        /// </summary>
        public ToTextFile3D()
          : base("ToTextFile", "Nickname",
              "Description",
              "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            bool key = false;
           
            pManager.AddBooleanParameter("Over-write?", "over wrt?", "", GH_ParamAccess.item, key);
            pManager.AddGenericParameter("DispZ", "inpt", "", GH_ParamAccess.item);
            pManager.AddTextParameter("FilePath DispZ", "path dispZ", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write?", "w", "", GH_ParamAccess.item);
            pManager.AddPointParameter("Points", "pts", "",GH_ParamAccess.list);
            pManager.AddTextParameter("FilePath Pts", "path pts", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            LA.Double.DenseMatrix disp = new LA.Double.DenseMatrix(2);
            string filePathZ = "";
            bool write = false;
            List<Point3d> pts = new List<Point3d>();
            string filePathP = "";
            bool key = false;

            DA.GetData(0, ref key);
            DA.GetData(1, ref disp);
            DA.GetData(2, ref filePathZ);
            DA.GetData(3, ref write);
            DA.GetDataList(4, pts);
            DA.GetData(5, ref filePathP);

            if (write)
            {
                // Open the file for appending if it exists, or create a new file if it doesn't exist
                using (StreamWriter writer = new StreamWriter(filePathZ, key))
                {
                    // Loop through each row of the matrix
                    for (int i = 0; i < disp.RowCount; i++)
                    {
                        // Loop through each column of the matrix
                        for (int j = 0; j < disp.ColumnCount; j++)
                        {
                            // Write the value at the current row and column to the file
                            writer.Write(disp[i, j] + "   ");
                        }
                        // Move to the next line in the file
                        writer.WriteLine();
                    }

                    // Write a blank line to the file to separate the data
                    writer.WriteLine();
                }


                if (write)
                {
                    // Open the file for appending if it exists, or create a new file if it doesn't exist
                    using (StreamWriter writer = new StreamWriter(filePathP, key))
                    {
                        foreach (Point3d p in pts)
                        {
                            writer.Write(p.X + "   "); 
                        }
                        writer.WriteLine();
                        foreach (Point3d p in pts)
                        {
                            writer.Write(p.Y + "   "); 
                        }
                        writer.WriteLine();
                        foreach (Point3d p in pts)
                        {
                            writer.Write(p.Z + "   ");
                        }
                        writer.WriteLine();
                        writer.WriteLine();
                    }

                }

            }

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
                // return Resources
                return Resources.txt;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D3B9C812-A2D1-4EFC-995C-CA2886AB24C8"); }
        }
    }
}