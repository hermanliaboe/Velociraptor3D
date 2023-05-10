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
            double n = 0.0;
            pManager.AddGenericParameter("Beams", "beams", "", GH_ParamAccess.list);
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

            pManager.AddNumberParameter("Beam chooser", "beam n", "", GH_ParamAccess.item, n);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Error Displacements AVG", "eDisp", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Error Forces AVG", "eForce", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Displacement Kara", "disp Kara", "", GH_ParamAccess.list);

            //pManager.AddGenericParameter("Error Force Beam n", "", "", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Velociraptor Beam n", "", "", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Karamba Beam n", "", "", GH_ParamAccess.list);

            pManager.AddGenericParameter("Full error disp n", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Full error force n", "", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("BeamElement", "beam", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Worst error - force ", "Worst", "", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BeamElement> beams = new List<BeamElement>();
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
            double nN0 = 0.0;
            double bN0 = 0.0;

            DA.GetDataList(0, beams);
            DA.GetData(1, ref dispV);
            DA.GetData(2, ref bfV);
            DA.GetDataList(3, transK);
            DA.GetDataList(4, rotK);
           
            DA.GetDataList(5, nK);
            DA.GetDataList(6, vyK);
            DA.GetDataList(7, vzK);
            DA.GetDataList(8, mtK);
            DA.GetDataList(9, myK);
            DA.GetDataList(10, mzK);
            DA.GetData(11, ref bN0);

            int bN = ((int)bN0);

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
            /*
            List<double> eNl  = new List<double>();
            List<double> eVyl = new List<double>();
            List<double> eVzl = new List<double>();
            List<double> eMtl = new List<double>();
            List<double> eMyl = new List<double>();
            List<double> eMzl = new List<double>();
            */


            double eN = 0;
            double eVy = 0;
            double eVz = 0;
            double eMt = 0;
            double eMy = 0;
            double eMz = 0;

            List<double> eF = new List<double>();
            LA.Matrix<double> fErrTot = LA.Matrix<double>.Build.Dense(13, beams.Count);

            int c = 0;
            for (int i = 0; i < bfV.ColumnCount; i++)
            {
                LA.Matrix<double> fErr = LA.Matrix<double>.Build.Dense(13, 1,0.0);

                fErr[0, 0] = errorFunc(bfV[0, i], nK[i * 2] * 1000);
                fErr[1, 0] = errorFunc(bfV[1, i], vyK[i * 2] * 1000);
                fErr[2, 0] = errorFunc(bfV[2, i], vzK[i * 2] * 1000);
                fErr[3, 0] = errorFunc(bfV[3, i], mtK[i * 2] * 1000000);
                fErr[4, 0] = errorFunc(bfV[4, i], myK[i * 2] * 1000000);
                fErr[5, 0] = errorFunc(bfV[5, i], mzK[i * 2] * 1000000);
                
                fErr[6, 0] = errorFunc(bfV[6 + 0, i], nK[i * 2 + 1] * 1000);
                fErr[7, 0] = errorFunc(bfV[6 + 1, i], vyK[i * 2 + 1] * 1000);
                fErr[8, 0] = errorFunc(bfV[6 + 2, i], vzK[i * 2 + 1] * 1000);
                fErr[9, 0] = errorFunc(bfV[6 + 3, i], mtK[i * 2 + 1] * 1000000);
                fErr[10, 0]= errorFunc(bfV[6 + 4, i], myK[i * 2 + 1] * 1000000);
                fErr[11, 0]= errorFunc(bfV[6 + 5, i], mzK[i * 2 + 1] * 1000000);

                eN +=  fErr[0, 0];
                eVy += fErr[1, 0];
                eVz += fErr[2, 0];
                eMt += fErr[3, 0];
                eMy += fErr[4, 0];
                eMz += fErr[5, 0];
                                ;
                eN  += fErr[6, 0];
                eVy += fErr[7, 0];
                eVz += fErr[8, 0];
                eMt += fErr[9, 0];
                eMy += fErr[10, 0];
                eMz += fErr[11, 0];

                fErr[12, 0] = fErr.ColumnSums()[0];

                fErrTot.SetSubMatrix(0, 13, i, 1, fErr);
                c+=2;

            }

            eF.Add(eN /  c);
            eF.Add(eVy / c);
            eF.Add(eVz / c);
            eF.Add(eMt / c);
            eF.Add(eMy / c);
            eF.Add(eMz / c);

            //making disp error
            List<double> dBeam = new List<double>();
            List<double> dVelo = new List<double>();
            List<double> dKara = new List<double>();

            int sn = beams[bN].StartNode.GlobalID;
            int en = beams[bN].EndNode.GlobalID;

            dBeam.Add(errorFunc(dispV[6 * sn + 0, 0], dispK[6 * sn + 0]));
            dBeam.Add(errorFunc(dispV[6 * sn + 1, 0], dispK[6 * sn + 1]));
            dBeam.Add(errorFunc(dispV[6 * sn + 2, 0], dispK[6 * sn + 2]));
            dBeam.Add(errorFunc(dispV[6 * sn + 3, 0], dispK[6 * sn + 3]));
            dBeam.Add(errorFunc(dispV[6 * sn + 4, 0], dispK[6 * sn + 4]));
            dBeam.Add(errorFunc(dispV[6 * sn + 5, 0], dispK[6 * sn + 5]));
            dBeam.Add(errorFunc(dispV[6 * en + 0, 0], dispK[6 * en + 0]));
            dBeam.Add(errorFunc(dispV[6 * en + 1, 0], dispK[6 * en + 1]));
            dBeam.Add(errorFunc(dispV[6 * en + 2, 0], dispK[6 * en + 2]));
            dBeam.Add(errorFunc(dispV[6 * en + 3, 0], dispK[6 * en + 3]));
            dBeam.Add(errorFunc(dispV[6 * en + 4, 0], dispK[6 * en + 4]));
            dBeam.Add(errorFunc(dispV[6 * en + 5, 0], dispK[6 * en + 5]));

            dVelo.Add(dispV[6 * sn + 0, 0]);
            dVelo.Add(dispV[6 * sn + 1, 0]);
            dVelo.Add(dispV[6 * sn + 2, 0]);
            dVelo.Add(dispV[6 * sn + 3, 0]);
            dVelo.Add(dispV[6 * sn + 4, 0]);
            dVelo.Add(dispV[6 * sn + 5, 0]);
            dVelo.Add(dispV[6 * en + 0, 0]);
            dVelo.Add(dispV[6 * en + 1, 0]);
            dVelo.Add(dispV[6 * en + 2, 0]);
            dVelo.Add(dispV[6 * en + 3, 0]);
            dVelo.Add(dispV[6 * en + 4, 0]);
            dVelo.Add(dispV[6 * en + 5, 0]);

            dKara.Add(dispK[6 * sn + 0]);
            dKara.Add(dispK[6 * sn + 1]);
            dKara.Add(dispK[6 * sn + 2]);
            dKara.Add(dispK[6 * sn + 3]);
            dKara.Add(dispK[6 * sn + 4]);
            dKara.Add(dispK[6 * sn + 5]);
            dKara.Add(dispK[6 * en + 0]);
            dKara.Add(dispK[6 * en + 1]);
            dKara.Add(dispK[6 * en + 2]);
            dKara.Add(dispK[6 * en + 3]);
            dKara.Add(dispK[6 * en + 4]);
            dKara.Add(dispK[6 * en + 5]);

            List<string> combinedListD = new List<string>();
            for (int i = 0; i < dBeam.Count; i++)
            {
                string element = Math.Round(dBeam[i], 3).ToString() + " ->   " + Math.Round(dVelo[i], 8).ToString() + "   :   " + Math.Round(dKara[i], 8).ToString();
                combinedListD.Add(element);
            }


            //Making forces
            List<double> fBeam = new List<double>();
            List<double> fVelo = new List<double>();
            List<double> fKara = new List<double>();

         
            fBeam.Add(errorFunc(bfV[0, bN], nK[bN * 2] * 1000));
            fBeam.Add(errorFunc(bfV[1, bN], vyK[bN * 2] * 1000));
            fBeam.Add(errorFunc(bfV[2, bN], vzK[bN * 2] * 1000));
            fBeam.Add(errorFunc(bfV[3, bN], mtK[bN * 2] * 1000000));
            fBeam.Add(errorFunc(bfV[4, bN], myK[bN * 2] * 1000000));
            fBeam.Add(errorFunc(bfV[5, bN], mzK[bN * 2] * 1000000));
            fBeam.Add(errorFunc(bfV[6 + 0, bN], nK[bN * 2 + 1] * 1000));
            fBeam.Add(errorFunc(bfV[6 + 1, bN], vyK[bN * 2 + 1] * 1000));
            fBeam.Add(errorFunc(bfV[6 + 2, bN], vzK[bN * 2 + 1] * 1000));
            fBeam.Add(errorFunc(bfV[6 + 3, bN], mtK[bN * 2 + 1] * 1000000));
            fBeam.Add(errorFunc(bfV[6 + 4, bN], myK[bN * 2 + 1] * 1000000));
            fBeam.Add(errorFunc(bfV[6 + 5, bN], mzK[bN * 2 + 1] * 1000000));

            //Making fVelo
            fVelo.Add(bfV[0, bN]);
            fVelo.Add(bfV[1, bN]);
            fVelo.Add(bfV[2, bN]);
            fVelo.Add(bfV[3, bN]);
            fVelo.Add(bfV[4, bN]);
            fVelo.Add(bfV[5, bN]);
            fVelo.Add(bfV[6 + 0, bN]);
            fVelo.Add(bfV[6 + 1, bN]);
            fVelo.Add(bfV[6 + 2, bN]);
            fVelo.Add(bfV[6 + 3, bN]);
            fVelo.Add(bfV[6 + 4, bN]);
            fVelo.Add(bfV[6 + 5, bN]);

            //Making fkara
            fKara.Add(nK[bN * 2] * 1000);
            fKara.Add(vyK[bN * 2] * 1000);
            fKara.Add(vzK[bN * 2] * 1000);
            fKara.Add(mtK[bN * 2] * 1000000);
            fKara.Add(myK[bN * 2] * 1000000);
            fKara.Add(mzK[bN * 2] * 1000000);
            fKara.Add(nK[bN * 2 + 1] * 1000);
            fKara.Add(vyK[bN * 2 + 1] * 1000);
            fKara.Add(vzK[bN * 2 + 1] * 1000);
            fKara.Add(mtK[bN * 2 + 1] * 1000000);
            fKara.Add(myK[bN * 2 + 1] * 1000000);
            fKara.Add(mzK[bN * 2 + 1] * 1000000);



            //Joing force error lits
            List<string> combinedListF = new List<string>();
            for (int i = 0; i < fBeam.Count; i++)
            {
                string element = Math.Round(fBeam[i],3).ToString() + " ->   " + Math.Round(fVelo[i],6).ToString() + "   :   " + Math.Round(fKara[i],6).ToString();
                combinedListF.Add(element);
            }

            

            for (int i = 0; i < fErrTot.ColumnCount; i++)
            {

            }



















            DA.SetDataList(0, eD);
            //DA.SetDataList(1, eF);
            DA.SetDataList(1, eF);

            DA.SetDataList(2, dispK);
            //DA.SetDataList(3, fVelo);
            //DA.SetDataList(4, fKara);

            DA.SetDataList(3, combinedListD);
            DA.SetDataList(4, combinedListF);
            DA.SetData(5, beams[bN]);
        }




        public double errorFunc(double V, double K)
        {
            double error = 0;
            if (V < 1 * Math.Pow(10, -4) && K < 1 * Math.Pow(10, -4)){
                error = 0;
            }
            else
            {
                V += 0.00000001;
                K += 0.00000001;
                //double error = 0.0;
                error = 100 * Math.Abs((Math.Abs(V) - Math.Abs(K)) / V);
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