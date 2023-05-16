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
            pManager.AddNumberParameter("Error % limit", "", "", GH_ParamAccess.item, 25.0) ;

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
            pManager.AddGenericParameter("Worst beams", "", "Beam elements with one force error over 25%, or chosen limit", GH_ParamAccess.list);

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
            double errorLimit = 0.0;

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
            DA.GetData(12, ref errorLimit);

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
                        dispK.Add(Math.Round(number1*1000, 8)); }
 
                }
                for (int k = 0; k < 3; k++)
                {
                    double number;
                    if (double.TryParse(rKs[k].Trim(), out number))
                    {
                        dispK.Add(Math.Round(number, 8));
                    }
                }
            }

            //Calculate displacement errors
            List<double> eTx   = new List<double>();
            List<double> eTy = new List<double>();
            List<double> eTz = new List<double>();
            List<double> eRx = new List<double>();
            List<double> eRy = new List<double>();
            List<double> eRz = new List<double>();
            List<double> avgDisplacementErrors    = new List<double>();

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
                eTx0 += errorFunc(Math.Round(dispV[i + 0, 0], 3), Math.Round(dispK[i + 0], 3));
                eTy0 += errorFunc(Math.Round(dispV[i + 1, 0], 3), Math.Round(dispK[i + 1], 3));
                eTz0 += errorFunc(Math.Round(dispV[i + 2, 0], 3), Math.Round(dispK[i + 2], 3));
                eRx0 += errorFunc(Math.Round(dispV[i + 3, 0], 3), Math.Round(dispK[i + 3], 3));
                eRy0 += errorFunc(Math.Round(dispV[i + 4, 0], 3), Math.Round(dispK[i + 4], 3));
                eRz0 += errorFunc(Math.Round(dispV[i + 5, 0], 3), Math.Round(dispK[i + 5], 3));

                eTx.Add(errorFunc(dispV[i + 0, 0], dispK[i + 0]));
                eTy.Add(errorFunc(dispV[i + 1, 0], dispK[i + 1]));
                eTz.Add(errorFunc(dispV[i + 2, 0], dispK[i + 2]));
                eRx.Add(errorFunc(dispV[i + 3, 0], dispK[i + 3]));
                eRy.Add(errorFunc(dispV[i + 4, 0], dispK[i + 4]));
                eRz.Add(errorFunc(dispV[i + 5, 0], dispK[i + 5]));
                
            }

            avgDisplacementErrors.Add(eTx0 / eTx.Count);
            avgDisplacementErrors.Add(eTy0 / eTy.Count);
            avgDisplacementErrors.Add(eTz0 / eTz.Count);
            avgDisplacementErrors.Add(eRx0 / eRx.Count);
            avgDisplacementErrors.Add(eRy0 / eRy.Count);
            avgDisplacementErrors.Add(eRz0 / eRz.Count);

            double eN = 0;
            double eVy = 0;
            double eVz = 0;
            double eMt = 0;
            double eMy = 0;
            double eMz = 0;

            List<double> avgForceErrors = new List<double>();

            int c = nK.Count;
           
            List<BeamElement> worstBeams = new List<BeamElement>();

            for (int i = 0; i < beams.Count; i++)
            {
                List<double> localForceErrors = new List<double>();

                double localErrorN1 = errorFunc(beams[i].ForceList[0], nK[i * 2] * 1000);
                localForceErrors.Add(localErrorN1);
                double localErrorVy1 = errorFunc(beams[i].ForceList[1], vyK[i * 2] * 1000);
                localForceErrors.Add(localErrorVy1);
                double localErrorVz1 = errorFunc(beams[i].ForceList[2], vzK[i * 2] * 1000);
                localForceErrors.Add(localErrorVz1);
                double localErrorMt1 = errorFunc(beams[i].ForceList[3], mtK[i * 2] * 1000000);
                localForceErrors.Add(localErrorMt1);
                double localErrorMy1 = errorFunc(beams[i].ForceList[4], myK[i * 2] * 1000000);
                localForceErrors.Add(localErrorMy1);
                double localErrorMz1 = errorFunc(beams[i].ForceList[5], mzK[i * 2] * 1000000);
                localForceErrors.Add(localErrorMz1);

                double localErrorN2 = errorFunc(beams[i].ForceList[6], nK[i * 2 + 1] * 1000);
                localForceErrors.Add(localErrorN2);
                double localErrorVy2 = errorFunc(beams[i].ForceList[7], vyK[i * 2 + 1] * 1000);
                localForceErrors.Add(localErrorVy2);
                double localErrorVz2 = errorFunc(beams[i].ForceList[8], vzK[i * 2 + 1] * 1000);
                localForceErrors.Add(localErrorVz2);
                double localErrorMt2 = errorFunc(beams[i].ForceList[9], mtK[i * 2 + 1] * 1000000);
                localForceErrors.Add(localErrorMt1);
                double localErrorMy2 = errorFunc(beams[i].ForceList[10], myK[i * 2 + 1] * 1000000);
                localForceErrors.Add(localErrorMy2);
                double localErrorMz2 = errorFunc(beams[i].ForceList[11], mzK[i * 2 + 1] * 1000000);
                localForceErrors.Add(localErrorMz2);

                foreach (double localForceError in localForceErrors)
                {
                    if (localForceError >= errorLimit)
                    {
                        worstBeams.Add(beams[i]);
                        break;
                    } 
                }
                beams[i].localForceErrors = localForceErrors;


                eN += localErrorN1;
                eVy += localErrorVy1;
                eVz += localErrorVz1;
                eMt += localErrorMt1;
                eMy += localErrorMy1;
                eMz += localErrorMz1;

                eN += localErrorN2;
                eVy += localErrorVy2;
                eVz += localErrorVz2;
                eMt += localErrorMt2;
                eMy += localErrorMy2;
                eMz += localErrorMz2;

                c += 2;
            }

            avgForceErrors.Add(eN /  c);
            avgForceErrors.Add(eVy / c);
            avgForceErrors.Add(eVz / c);
            avgForceErrors.Add(eMt / c);
            avgForceErrors.Add(eMy / c);
            avgForceErrors.Add(eMz / c);

            //making disp error
            List<double> dispErrorsBeam = new List<double>();
            List<double> dVelo = new List<double>();
            List<double> dKara = new List<double>();

            int sn = beams[bN].StartNode.GlobalID;
            int en = beams[bN].EndNode.GlobalID;

            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * sn + 0, 0], 3), Math.Round(dispK[6 * sn + 0], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * sn + 1, 0], 3), Math.Round(dispK[6 * sn + 1], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * sn + 2, 0], 3), Math.Round(dispK[6 * sn + 2], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * sn + 3, 0], 3), Math.Round(dispK[6 * sn + 3], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * sn + 4, 0], 3), Math.Round(dispK[6 * sn + 4], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * sn + 5, 0], 3), Math.Round(dispK[6 * sn + 5], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * en + 0, 0], 3), Math.Round(dispK[6 * en + 0], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * en + 1, 0], 3), Math.Round(dispK[6 * en + 1], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * en + 2, 0], 3), Math.Round(dispK[6 * en + 2], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * en + 3, 0], 3), Math.Round(dispK[6 * en + 3], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * en + 4, 0], 3), Math.Round(dispK[6 * en + 4], 3)));
            dispErrorsBeam.Add(errorFunc(Math.Round(dispV[6 * en + 5, 0], 3), Math.Round(dispK[6 * en + 5], 3)));

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
            for (int i = 0; i < dispErrorsBeam.Count; i++)
            {
                string element = Math.Round(dispErrorsBeam[i], 3).ToString() + " ->   " + Math.Round(dVelo[i], 6).ToString() + "   :   " + Math.Round(dKara[i], 6).ToString();
                combinedListD.Add(element);
            }


            //Making forces
            List<double> fBeam = new List<double>();
            List<double> fVelo = new List<double>();
            List<double> fKara = new List<double>();

         
            fBeam.Add(errorFunc(Math.Round(bfV[0, bN], 6), Math.Round(nK[bN * 2] * 1000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[1, bN], 6), Math.Round(vyK[bN * 2] * 1000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[2, bN], 6), Math.Round(vzK[bN * 2] * 1000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[3, bN], 6), Math.Round(mtK[bN * 2] * 1000000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[4, bN], 6), Math.Round(myK[bN * 2] * 1000000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[5, bN], 6), Math.Round(mzK[bN * 2] * 1000000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[6 + 0, bN], 6), Math.Round(nK[bN * 2 + 1] * 1000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[6 + 1, bN], 6), Math.Round(vyK[bN * 2 + 1] * 1000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[6 + 2, bN], 6), Math.Round(vzK[bN * 2 + 1] * 1000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[6 + 3, bN], 6), Math.Round(mtK[bN * 2 + 1] * 1000000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[6 + 4, bN], 6), Math.Round(myK[bN * 2 + 1] * 1000000, 6)));
            fBeam.Add(errorFunc(Math.Round(bfV[6 + 5, bN], 6), Math.Round(mzK[bN * 2 + 1] * 1000000, 6)));

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
            fKara.Add(Math.Round(nK[bN * 2] * 1000.0, 6));
            fKara.Add(Math.Round(vyK[bN * 2] * 1000.0, 6));
            fKara.Add(Math.Round(vzK[bN * 2] * 1000.0, 6));
            fKara.Add(Math.Round(mtK[bN * 2] * 1000000.0, 6));
            fKara.Add(Math.Round(myK[bN * 2] * 1000000.0, 6));
            fKara.Add(Math.Round(mzK[bN * 2] * 1000000.0, 6));
            fKara.Add(Math.Round(nK[bN * 2 + 1] * 1000.0, 6));
            fKara.Add(Math.Round(vyK[bN * 2 + 1] * 1000.0, 6));
            fKara.Add(Math.Round(vzK[bN * 2 + 1] * 1000.0, 6));
            fKara.Add(Math.Round(mtK[bN * 2 + 1] * 1000000.0, 6));
            fKara.Add(Math.Round(myK[bN * 2 + 1] * 1000000.0, 6));
            fKara.Add(Math.Round(mzK[bN * 2 + 1] * 1000000.0, 6));



            //Joing force error lits
            List<string> combinedListF = new List<string>();
            for (int i = 0; i < fBeam.Count; i++)
            {
                string element = Math.Round(fBeam[i],3).ToString() + " ->   " + Math.Round(fVelo[i],6).ToString() + "   :   " + Math.Round(fKara[i],6).ToString();
                combinedListF.Add(element);
            }


            DA.SetDataList(0, avgDisplacementErrors);
            //DA.SetDataList(1, eF);
            DA.SetDataList(1, avgForceErrors);

            DA.SetDataList(2, dispK);
            //DA.SetDataList(3, fVelo);
            //DA.SetDataList(4, fKara);

            DA.SetDataList(3, combinedListD);
            DA.SetDataList(4, combinedListF);
            DA.SetData(5, beams[bN]);
            DA.SetDataList(6, worstBeams);
        }




        public double errorFunc(double V, double K)
        {
            double error = 0.0;
            
            
            V += 0.00000001;
            K += 0.00000001;
            //double error = 0.0;
            error = Math.Round(100.0 * Math.Abs((Math.Abs(V) - Math.Abs(K)) / V), 3);
               
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