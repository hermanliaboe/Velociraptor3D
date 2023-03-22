using FEM.Classes;
using GH_IO.Serialization;
using Rhino.Display;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LA = MathNet.Numerics.LinearAlgebra;

namespace FEM.Classes
{
    internal class Matrices
    {
        public LA.Matrix<double> GlobalK;
        public LA.Matrix<double> GlobalM;
        public LA.Matrix<double> GlobalC;
        public LA.Matrix<double> GlobalN;
        public LA.Matrix<double> GlobalF;

        public Matrices(int dof, List<BeamElement> elements)
        {
            this.GlobalK = BuildGlobalK(dof, elements);


        }

        public Matrices()
        {
        }

        // Creates force vector ###################################################################################
        public LA.Matrix<double> BuildForceVector(List<Load> loads, int dof)
        {
            LA.Matrix<double> forceVec = LA.Matrix<double>.Build.Dense(dof, 1, 0);

            foreach (Load load in loads)
            {
                forceVec[load.NodeID * 3, 0] = load.ForceVector.X;
                forceVec[load.NodeID * 3 + 1, 0] = load.ForceVector.Z;
                forceVec[load.NodeID * 3 + 2, 0] = load.MomentVector.Y;
            }

            return forceVec;
        }



        // Created global K-matrix ###################################################################################
        public LA.Matrix<double> BuildGlobalK(int dof, List<BeamElement> elements)
        {
            LA.Matrix<double> globalK = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            foreach (var element in elements)
            {
                int nDof = 3;
                //Retrive element k from function
                LA.Matrix<double> ke = GetKel(element);

                //Get nodeID and *3 to get globalK placement
                int idS = element.StartNode.GlobalID * nDof;
                int idE = element.EndNode.GlobalID * nDof;

                //divide the element matrix into four matrices
                LA.Matrix<double> ke11 = ke.SubMatrix(0, nDof, 0, nDof);
                LA.Matrix<double> ke12 = ke.SubMatrix(0, nDof, nDof, nDof);
                LA.Matrix<double> ke21 = ke.SubMatrix(nDof, nDof, 0, nDof);
                LA.Matrix<double> ke22 = ke.SubMatrix(nDof, nDof, nDof, nDof);

                //Puts the four matrices into the correct place in globalK (yes, correct buddy)
                for (int r = 0; r < nDof; r++)
                {
                    for (int c = 0; c < nDof; c++)
                    {
                        globalK[idS + r, idS + c] = globalK[idS + r, idS + c] + ke11[r, c];
                        globalK[idS + r, idE + c] = globalK[idS + r, idE + c] + ke12[r, c];
                        globalK[idE + r, idS + c] = globalK[idE + r, idS + c] + ke21[r, c];
                        globalK[idE + r, idE + c] = globalK[idE + r, idE + c] + ke22[r, c];
                    }
                }
            }

            return globalK;
        }




        // Creates k matrix element level #########################################################################
        public LA.Matrix<double> GetKel(BeamElement beam)
        {
            int dof = 6;  //how many dof per element


            //gets length of element
            Node startNode = beam.StartNode;
            double z1 = startNode.Point.Z;
            double x1 = startNode.Point.X;

            Node endNode = beam.EndNode;
            double z2 = endNode.Point.Z;
            double x2 = endNode.Point.X;

            double l = beam.Length;

            //Define standard k for two node beam element. 
            LA.Matrix<double> kEl = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            double E = beam.YoungsMod;
            double h = beam.Height;
            double w = beam.Width;
            double A = h * w;
            double I = (1.0 / 12.0) * Math.Pow(h, 3.0) * w;

            double ealA = (E * A) / l;
            double eilB = 12.0 * (E * I) / Math.Pow(l, 3.0);
            double eilC = 6.0 * (E * I) / Math.Pow(l, 2.0);
            double eilD = 4.0 * (E * I) / l;
            double eilE = 2.0 * (E * I) / l;



            kEl[0, 0] = ealA; kEl[0, 1] = 0;    kEl[0, 2] = 0;     kEl[0, 3] = -ealA; kEl[0, 4] = 0;     kEl[0, 5] = 0;
            kEl[1, 0] = 0;    kEl[1, 1] = eilB; kEl[1, 2] = -eilC; kEl[1, 3] = 0;     kEl[1, 4] = -eilB; kEl[1, 5] = -eilC;
            kEl[2, 0] = 0;    kEl[2, 1] = -eilC;kEl[2, 2] = eilD;  kEl[2, 3] = 0;     kEl[2, 4] = eilC;  kEl[2, 5] = eilE;
            kEl[3, 0] = -ealA;kEl[3, 1] = 0;    kEl[3, 2] = 0;     kEl[3, 3] = ealA;  kEl[3, 4] = 0;     kEl[3, 5] = 0;
            kEl[4, 0] = 0;    kEl[4, 1] = -eilB;kEl[4, 2] = eilC;  kEl[4, 3] = 0;     kEl[4, 4] = eilB;  kEl[4, 5] = eilC;
            kEl[5, 0] = 0;    kEl[5, 1] = -eilC;kEl[5, 2] = eilE;  kEl[5, 3] = 0;     kEl[5, 4] = eilC;  kEl[5, 5] = eilD;


            //Creates T-matrix to adjust element to global axis
            LA.Matrix<double> kT = TransformMatrix(kEl, x1, x2, z1, z2, l);
            return kT;
        }

        public LA.Matrix<double> TransformMatrix(LA.Matrix<double> matrix, double x1, double x2, double z1, double z2, double l)
        {
            LA.Matrix<double> t = LA.Matrix<double>.Build.Dense(matrix.RowCount, matrix.ColumnCount, 0);

            double c = (x2 - x1) / l;
            double s = (z2 - z1) / l;

            t[0, 0] = c; t[0, 1] = s; t[0, 2] = 0; t[0, 3] = 0; t[0, 4] = 0; t[0, 5] = 0;
            t[1, 0] = -s; t[1, 1] = c; t[1, 2] = 0; t[1, 3] = 0; t[1, 4] = 0; t[1, 5] = 0;
            t[2, 0] = 0; t[2, 1] = 0; t[2, 2] = 1; t[2, 3] = 0; t[2, 4] = 0; t[2, 5] = 0;
            t[3, 0] = 0; t[3, 1] = 0; t[3, 2] = 0; t[3, 3] = c; t[3, 4] = s; t[3, 5] = 0;
            t[4, 0] = 0; t[4, 1] = 0; t[4, 2] = 0; t[4, 3] = -s; t[4, 4] = c; t[4, 5] = 0;
            t[5, 0] = 0; t[5, 1] = 0; t[5, 2] = 0; t[5, 3] = 0; t[5, 4] = 0; t[5, 5] = 1;

            LA.Matrix<double> tT = t.Transpose();
            LA.Matrix<double> tm = tT.Multiply(matrix);
            LA.Matrix<double> tmt = tm.Multiply(t);
            return tmt;

        }

        //Creates global K with supports ############################################################################
        public LA.Matrix<double> BuildGlobalKsup(int dof, LA.Matrix<double> globalK, List<Support> supports, List<Node> nodes)
        {
            LA.Matrix<double> globalKsup = globalK.Clone();
            foreach (Support support in supports)
            {
                foreach (Node node in nodes)
                {
                    
                    if (support.Point == node.Point)
                    {
                        LA.Matrix<double> col = LA.Matrix<double>.Build.Dense(dof, 1, 0);
                        LA.Matrix<double> row = LA.Matrix<double>.Build.Dense(1, dof, 0);
                        int idN = node.GlobalID;

                        
                        if (support.Tx == true)
                        {
                            globalKsup.SetSubMatrix(idN*3, 0, row);
                            globalKsup.SetSubMatrix(0, idN*3, col);
                            globalKsup[idN*3, idN*3] = 1;
                        }
                        if (support.Tz == true)
                        {
                            globalKsup.SetSubMatrix(idN * 3 + 1, 0, row);
                            globalKsup.SetSubMatrix(0, idN * 3 + 1 , col);
                            globalKsup[idN * 3 + 1, idN * 3 + 1] = 1;
                        }
                        if (support.Ry == true)
                        {
                            globalKsup.SetSubMatrix(idN*3 +2, 0, row);
                            globalKsup.SetSubMatrix(0, idN*3 +2, col);
                            globalKsup[idN*3 +2, idN*3 +2] = 1;
                        }
                    }
                }
            }
            return globalKsup;
        }

        public LA.Matrix<double> GetMel(BeamElement beam, bool lumped)
        {
            int dof = 6;  // dof per element

            //gets length of element
            Node startNode = beam.StartNode;
            double z1 = startNode.Point.Z;
            double x1 = startNode.Point.X;

            Node endNode = beam.EndNode;
            double z2 = endNode.Point.Z;
            double x2 = endNode.Point.X;
            double l = beam.Length;
            double mTot = beam.Height * beam.Width * beam.Rho * beam.Length;

            // mass element matrix of right size filled with zeros
            LA.Matrix<double> mEl = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            mEl[0, 0] = mEl[3,3] = 140;
            mEl[0, 3] = mEl[3, 0] = 70;
            mEl[1, 1] = mEl[4, 4] = 156;
            mEl[1, 2] = mEl[2, 1] = 22 * l;
            mEl[1, 4] = mEl[4, 1] = 54;
            mEl[1, 5] = mEl[5, 1] = -13 * l;
            mEl[2, 2] = mEl[5, 5] = 4*Math.Pow(l, 2);
            mEl[2, 4] = mEl[4, 2] = 13 * l;
            mEl[2, 5] = mEl[5, 2] = -3 * Math.Pow(l, 2);
            mEl[4, 5] = mEl[5, 4] = -22 * l;

            mEl *= (mTot / 420);


            if (lumped==true) 
                // if bool lumped is true, sets mEl to lumped mass matrix by
                // summing up the rows of the matrix and placing them on the diagonal
            {
                LA.Matrix<double> mElLumped = LA.Matrix<double>.Build.Dense(mEl.RowCount, mEl.ColumnCount, 0);
                LA.Vector<double> rowSums = mEl.RowSums();
                mElLumped.SetDiagonal(rowSums);
                mEl = mElLumped;
            }

            //Transform to global coordinates
            LA.Matrix<double> mT = TransformMatrix(mEl, x1, x2, z1, z2, l);

            return mT;
        }
        public LA.Matrix<double> BuildGlobalM(int dof, List<BeamElement> elements, bool lumped)
        {
            // the only difference between this function and BuildGlobalK is that
            // BuildGlobalM accounts for lumped mass. Consider making this one function later
            LA.Matrix<double> globalM = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            foreach (var element in elements)
            {
                int nDof = 3; //dof per node
                //Retrive element k from function
                LA.Matrix<double> me = GetMel(element, lumped);

                //Get nodeID and *3 to get globalK placement
                int idS = element.StartNode.GlobalID * nDof;
                int idE = element.EndNode.GlobalID * nDof;

                //divide the element matrix into four matrices
                LA.Matrix<double> me11 = me.SubMatrix(0, nDof, 0, nDof);
                LA.Matrix<double> me12 = me.SubMatrix(0, nDof, nDof, nDof);
                LA.Matrix<double> me21 = me.SubMatrix(nDof, nDof, 0, nDof);
                LA.Matrix<double> ke22 = me.SubMatrix(nDof, nDof, nDof, nDof);

                //Puts the four matrices into the correct place in globalK (yes, correct buddy)
                for (int r = 0; r < nDof; r++)
                {
                    for (int c = 0; c < nDof; c++)
                    {
                        globalM[idS + r, idS + c] = globalM[idS + r, idS + c] + me11[r, c];
                        globalM[idS + r, idE + c] = globalM[idS + r, idE + c] + me12[r, c];
                        globalM[idE + r, idS + c] = globalM[idE + r, idS + c] + me21[r, c];
                        globalM[idE + r, idE + c] = globalM[idE + r, idE + c] + ke22[r, c];
                    }
                }
            }

            return globalM;
        }
        public LA.Matrix<double> BuildSupMat(int dof, LA.Matrix<double> globalM, List<Support> supports, List<Node> nodes)
        {
            LA.Matrix<double> supMatrix = globalM.Clone();
            foreach (Support support in supports)
            {
                foreach (Node node in nodes)
                {

                    if (support.Point == node.Point)
                    {
                        LA.Matrix<double> col = LA.Matrix<double>.Build.Dense(dof, 1, 0);
                        LA.Matrix<double> row = LA.Matrix<double>.Build.Dense(1, dof, 0);
                        int idN = node.GlobalID;


                        if (support.Tx == true)
                        {
                            supMatrix.SetSubMatrix(idN * 3, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 3, col);
                            supMatrix[idN * 3, idN * 3] = 1;
                        }
                        if (support.Tz == true)
                        {
                            supMatrix.SetSubMatrix(idN * 3 + 1, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 3 + 1, col);
                            supMatrix[idN * 3 + 1, idN * 3 + 1] = 1;
                        }
                        if (support.Ry == true)
                        {
                            supMatrix.SetSubMatrix(idN * 3 + 2, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 3 + 2, col);
                            supMatrix[idN * 3 + 2, idN * 3 + 2] = 1;
                        }
                    }
                }
            }
            return supMatrix;
        }


        public LA.Matrix<double> BuildC(LA.Matrix<double> M, LA.Matrix<double> K, double zeta, double wi, double wj)
        {
            LA.Matrix<double> W = EigenValue(K, M);
            //double wi = W[0, w1];
            //double wj = W[0, w2];

            double a0 = zeta * (2*wi * wj) / (wi + wj);
            double a1 = zeta *(2) / (wi + wj);

            var C = M.Multiply(a0) + K.Multiply(a1);
            
            return C;
        }


        public LA.Matrix<double> EigenValue(LA.Matrix<double> K, LA.Matrix<double> M)
        {
            // Solve the generalized eigenvalue problemv
            var factorizedM = M.QR();
            var factorizedK = factorizedM.Solve(K);
            var evd = factorizedK.Evd(LA.Symmetricity.Asymmetric);

            // Extract the eigenvalues and eigenvectors
            double[] ev = evd.EigenValues.Select(x => x.Real).ToArray();
            LA.Matrix<double> V = evd.EigenVectors;
            var W = LA.Matrix<double>.Build.Dense(1, ev.Length);
            int i = 0;
            foreach (double w in ev)
            {
                W[0, i] = w;
                i++;
            }
            return W;
        }



        //Creates global lumped mass matrix #############################################################################
        //public LA.Matrix<double> BuildMassMatrix(int dof, List<BeamElement> beams)
        //{
        //    LA.Matrix<double> massMatrix = LA.Matrix<double>.Build.Dense(dof, dof, 0);
        //    foreach (BeamElement beam in beams)
        //    {
        //        int nDof = 3;
        //        double l = beam.length;
        //        double mTot = beam.height * beam.width * beam.rho * beam.length;
        //        double m1 = mTot * (l / 2);

        //        massMatrix[beam.startNode.globalID * nDof, beam.startNode.globalID * nDof] += m1;
        //        massMatrix[beam.startNode.globalID * nDof + 1, beam.startNode.globalID * nDof + 1] += m1;
        //        massMatrix[beam.startNode.globalID * nDof + 2, beam.startNode.globalID * nDof + 2] += m1;


        //        massMatrix[beam.endNode.globalID * nDof, beam.endNode.globalID * nDof] += m1;
        //        massMatrix[beam.endNode.globalID * nDof + 1, beam.endNode.globalID * nDof + 1] += m1;
        //        massMatrix[beam.endNode.globalID * nDof + 2, beam.endNode.globalID * nDof + 2] += m1;

        //    }
        //    return massMatrix;
        //}




    }
}