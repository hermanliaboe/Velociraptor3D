using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

namespace FEM3D.Components
{
    public class Checky : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Checky()
          : base("Checky", "Checky",
              "Description",
              "Masters3D", "FEM3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Displacement - Velo", "Disp-Velo", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Beam Forces - Velo", "Forces-Velo", "", GH_ParamAccess.item);

            pManager.AddGenericParameter("Translation - Kara", "Trans-Kara", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Rotations - Kara", "Rot-Kara", "", GH_ParamAccess.list);
            
            pManager.AddGenericParameter("N - Kara", "N-Kara", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Vy - Kara", "Vy-Kara", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Vz - Kara", "Vz-Kara", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mt - Kara", "Mt-Kara", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("My - Kara", "My-Kara", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mz - Kara", "Mz-Velo", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Node chooser", "Node N", "", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Beam chooser", "beam N", "", GH_ParamAccess.item, 0);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Error Displacements", "eDisp", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Error Forces", "eForce", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Beam Disp", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Beam Force", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            LA.Double.DenseMatrix dispV = new LA.Double.DenseMatrix(2);
            LA.Double.DenseMatrix bfV = new LA.Double.DenseMatrix(2);
            List<string> transK = new List<string>();
            List<string> rotK = new List<string>();
            List<double> nK =  new List<double>();
            List<double> vyK = new List<double>();
            List<double> vzK = new List<double>();
            List<double> mtK = new List<double>();
            List<double> myK = new List<double>();
            List<double> mzK = new List<double>();
            int nN = 0;
            int bN = 0;

            DA.GetData(0, ref dispV);
            DA.GetData(1, ref bfV);
            DA.GetDataList(2, transK);
            DA.GetDataList(3, rotK);
           
            DA.GetDataList(4, nK);
            DA.GetDataList(5, vyK);
            DA.GetDataList(6, vzK);
            DA.GetDataList(7, mtK);
            DA.GetDataList(8, myK);
            DA.GetDataList(9, mzK);
            DA.GetData(10, ref nN);
            DA.GetData(11, ref bN);


            //Create displacement list for Karamaba
            List<double> dispK = new List<double>();
            for (int i = 0; i < transK.Count; i++)
            {
                transK[i] = transK[i].Trim('{', '}');
                rotK[i] = rotK[i].Trim('{', '}');

                // split the string into substrings using a comma as the delimiter
                string[] tKs = transK[i].Split(',');
                string[] rKs = rotK[i].Split(',');

                // convert each substring to a double and add it to a list
                for (int j = 0; j < 3; j++)
                {
                    double number1;
                    if (double.TryParse(tKs[j].Trim(), out number1)){
                        dispK.Add(number1*1000); }
 
                }
                for (int k = 0; k < 3; k++)
                {
                    double number;
                    if (double.TryParse(rKs[k].Trim(), out number))
                    {
                        dispK.Add(number);
                    }
                }
            }

            //Make error displacement
            List<double> eTx   = new List<double>();
            List<double> eTy = new List<double>();
            List<double> eTz = new List<double>();
            List<double> eRx = new List<double>();
            List<double> eRy = new List<double>();
            List<double> eRz = new List<double>();
            List<double> eD    = new List<double>();

            double eTx0 = 0;
            double eTy0 = 0;
            double eTz0 = 0;
            double eRx0 = 0;
            double eRy0 = 0;
            double eRz0 = 0;
            double errorAvg = 0;

            // compare the displacements six by six
            for (int i = 0; i < dispK.Count; i += 6)
            {
                eTx0 += errorFunc(dispV[i + 0, 0], dispK[i + 0]);
                eTy0 += errorFunc(dispV[i + 1, 0], dispK[i + 1]);
                eTz0 += errorFunc(dispV[i + 2, 0], dispK[i + 2]);
                eRx0 += errorFunc(dispV[i + 3, 0], dispK[i + 3]);
                eRy0 += errorFunc(dispV[i + 4, 0], dispK[i + 4]);
                eRz0 += errorFunc(dispV[i + 5, 0], dispK[i + 5]);

                eTx.Add(errorFunc(dispV[i + 0, 0], dispK[i + 0]));
                eTy.Add(errorFunc(dispV[i + 1, 0], dispK[i + 1]));
                eTz.Add(errorFunc(dispV[i + 2, 0], dispK[i + 2]));
                eRx.Add(errorFunc(dispV[i + 3, 0], dispK[i + 3]));
                eRy.Add(errorFunc(dispV[i + 4, 0], dispK[i + 4]));
                eRz.Add(errorFunc(dispV[i + 5, 0], dispK[i + 5]));
                
            }

            eD.Add(eTx0 / eTx.Count);
            eD.Add(eTy0 / eTy.Count);
            eD.Add(eTz0 / eTz.Count);
            eD.Add(eRx0 / eRx.Count);
            eD.Add(eRy0 / eRy.Count);
            eD.Add(eRz0 / eRz.Count);


            //Forces
            double eN = 0;
            double eVy = 0;
            double eVz = 0;
            double eMt = 0;
            double eMy = 0;
            double eMz = 0;

            List<double> eF = new List<double>();

            int c = 0;
            for (int i = 0; i < bfV.ColumnCount; i++)
            {
                eN += errorFunc(bfV[0, i],   nK[i*2]*1000);
                eVy += errorFunc(bfV[1, i], vyK[i*2]*1000);
                eVz += errorFunc(bfV[2, i], vzK[i*2]*1000);
                eMt += errorFunc(bfV[3, i], mtK[i*2]*1000000);
                eMy += errorFunc(bfV[4, i], myK[i*2]*1000000);
                eMz += errorFunc(bfV[5, i], mzK[i*2]*1000000);

                eN  += errorFunc(bfV[6+0, i],  nK[i * 2+1]*1000);
                eVy += errorFunc(bfV[6+1, i], vyK[i * 2+1]*1000);
                eVz += errorFunc(bfV[6+2, i], vzK[i * 2+1]*1000);
                eMt += errorFunc(bfV[6+3, i], mtK[i * 2+1]*1000000);
                eMy += errorFunc(bfV[6+4, i], myK[i * 2+1]*1000000);
                eMz += errorFunc(bfV[6+5, i], mzK[i * 2+1]*1000000);
                c+=2;
            }
            eF.Add(eN /  c);
            eF.Add(eVy / c);
            eF.Add(eVz / c);
            eF.Add(eMt / c);
            eF.Add(eMy / c);
            eF.Add(eMz / c);



            List<double> eNode = new List<double>();
            List<double> eBeam = new List<double>();



            eNode.Add(errorFunc(dispV[nN * 6 , 0], dispK[nN * 6]));
            eNode.Add(errorFunc(dispV[nN*6 + 1, 0], dispK[nN*6 + 1]));
            eNode.Add(errorFunc(dispV[nN*6 + 2, 0], dispK[nN*6 + 2]));
            eNode.Add(errorFunc(dispV[nN*6 + 3, 0], dispK[nN*6 + 3]));
            eNode.Add(errorFunc(dispV[nN*6 + 4, 0], dispK[nN*6 + 4]));
            eNode.Add(errorFunc(dispV[nN*6 + 5, 0], dispK[nN*6 + 5]));



            eBeam.Add(errorFunc(bfV[0, bN], nK[bN * 2] * 1000));
            eBeam.Add(errorFunc(bfV[1, bN], vyK[bN * 2] * 1000));
            eBeam.Add(errorFunc(bfV[2, bN], vzK[bN * 2] * 1000));
            eBeam.Add(errorFunc(bfV[3, bN], mtK[bN * 2] * 1000000));
            eBeam.Add(errorFunc(bfV[4, bN], myK[bN * 2] * 1000000));
            eBeam.Add(errorFunc(bfV[5, bN], mzK[bN * 2] * 1000000));
            eBeam.Add(errorFunc(bfV[6 + 0, bN], nK[bN * 2 + 1] * 1000));
            eBeam.Add(errorFunc(bfV[6 + 1, bN], vyK[bN * 2 + 1] * 1000));
            eBeam.Add(errorFunc(bfV[6 + 2, bN], vzK[bN * 2 + 1] * 1000));
            eBeam.Add(errorFunc(bfV[6 + 3, bN], mtK[bN * 2 + 1] * 1000000));
            eBeam.Add(errorFunc(bfV[6 + 4, bN], myK[bN * 2 + 1] * 1000000));
            eBeam.Add(errorFunc(bfV[6 + 5, bN], mzK[bN * 2 + 1] * 1000000));





            DA.SetDataList(0, eD);
            DA.SetDataList(1, eF);
            DA.SetDataList(2, eNode);
            DA.SetDataList(3, eBeam);


        }




        public double errorFunc(double V, double K)
        {
            double error = 0;
            if (V != 0)
            {
                error = Math.Abs(  (Math.Abs(V) - Math.Abs(K)) / V);

            }

            return error;
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
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4335E395-E504-4C59-8F76-B3D423DBF8BE"); }
        }
    }
}