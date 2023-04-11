using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FEM3D.Classes;
using FEM3D.Properties;

using MathNet.Numerics;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Double;

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
            pManager.AddGenericParameter("Input", "inpt", "", GH_ParamAccess.item);
            pManager.AddTextParameter("FilePath", "path", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Write?", "w", "", GH_ParamAccess.item);
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

            DenseMatrix values = new DenseMatrix(1);
            string filePath = "";
            bool write = false;

            DA.GetData(0, ref values);
            DA.GetData(1, ref filePath);
            DA.GetData(2, ref write);


            if (write)
            {
                // Open the file for writing
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Loop through each row of the matrix
                    for (int i = 0; i < values.ColumnCount; i++)
                    {
                        // Loop through each column of the matrix
                        for (int j = 0; j < values.RowCount; j++)
                        {
                            // Write the value at the current row and column to the file
                            writer.Write(values[j, i] + "   ");
                        }
                        // Move to the next line in the file
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