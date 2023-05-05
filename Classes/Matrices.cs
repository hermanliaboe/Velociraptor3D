using FEM3D.Classes;
using GH_IO.Serialization;
using MathNet.Numerics.LinearAlgebra;
using Rhino.Display;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using LA = MathNet.Numerics.LinearAlgebra;

namespace FEM3D.Classes
{
    internal class Matrices
    {
        public LA.Matrix<double> GlobalK;
        public LA.Matrix<double> GlobalM;
        public LA.Matrix<double> GlobalC;
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
                forceVec[load.NodeID * 6, 0] = load.ForceVector.X;
                forceVec[load.NodeID * 6 + 1, 0] = load.ForceVector.Y;
                forceVec[load.NodeID * 6 + 2, 0] = load.ForceVector.Z;
                forceVec[load.NodeID * 6 + 3, 0] = load.MomentVector.X;
                forceVec[load.NodeID * 6 + 4, 0] = load.MomentVector.Y;
                forceVec[load.NodeID * 6 + 5, 0] = load.MomentVector.Z;

            }

            return forceVec;
        }



        // Created global K-matrix ###################################################################################
        public LA.Matrix<double> BuildGlobalK(int dof, List<BeamElement> elements)
        {
            LA.Matrix<double> globalK = LA.Matrix<double>.Build.Dense(dof, dof, 0);
            //var kEl = LA.Matrix<double>.Build.Dense(dof/12,dof/12,0);
            foreach (var element in elements)
            {
                int nDof = element.ElDof/2;
                //Retrive element k from function
                LA.Matrix<double> ke = GetKel(element);
                //kEl = ke;

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
            int dof = beam.ElDof;  //how many dof per element

            
            //gets length of element
            Node startNode = beam.StartNode;
            double z1 = startNode.Point.Z;
            double x1 = startNode.Point.X;
            double y1 = startNode.Point.Y;

            Node endNode = beam.EndNode;
            double z2 = endNode.Point.Z;
            double x2 = endNode.Point.X;
            double y2 = endNode.Point.Y;

            double l = beam.Length;

            //Define standard k for two node beam element. 
            LA.Matrix<double> kEl = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            double E = beam.YoungsMod;
            double G = beam.ShearMod;
            double h = beam.Height;
            double w = beam.Width;
            double A = h * w;
            double Iy = (1.0 / 12.0) * Math.Pow(h, 3.0) * w;
            double Iz = (1.0 / 12.0) * Math.Pow(w, 3.0) * h;  
            double J = (w * h * (Math.Pow(w, 2.0) + Math.Pow(h, 2.0))) / 12.0; //Polar area moment of inertia

            double k1 = (E * A) / l;
            double k2 = 12.0 * (E * Iz) / Math.Pow(l, 3.0);
            double k3 = 6.0 * (E * Iz) / Math.Pow(l, 2.0);
            double k4 = 4.0 * (E * Iz) / l;
            double k5 = 2.0 * (E * Iz) / l;
            double k6 = 12.0 * (E * Iy) / Math.Pow(l, 3.0);
            double k7 = 6.0 * (E * Iy) / Math.Pow(l, 2.0);
            double k8 = 4.0 * (E * Iy) / l;
            double k9 = 2.0 * (E * Iy) / l;
            double k10 = (G * J) / l;


            kEl[0, 0] = kEl[6, 6] = k1;
            kEl[6, 0] = kEl[0, 6] = -k1;
            kEl[1, 1] = kEl[7, 7] = k2;
            kEl[1, 7] = kEl[7, 1] = -k2;
            kEl[1, 5] = kEl[5, 1] = kEl[1, 11] = kEl[11, 1] = k3;
            kEl[5, 7] = kEl[7, 5] = kEl[7, 11] = kEl[11, 7] = -k3;
            kEl[5, 5] = kEl[11, 11] = k4;
            kEl[5, 11] = kEl[11, 5] = k5;
            kEl[2, 2] = kEl[8, 8] = k6;
            kEl[3, 3] = kEl[9, 9] = k10;
            kEl[3, 9] = kEl[9, 3] = -k10;
            kEl[2, 8] = kEl[8, 2] = -k6;
            kEl[4, 8] = kEl[8, 4] = kEl[8, 10] = kEl[10, 8] = k7;
            kEl[2, 4] = kEl[4, 2] = kEl[2, 10] = kEl[10, 2] = -k7;
            kEl[4, 4] = kEl[10, 10] = k8;
            kEl[4, 10] = kEl[10, 4] = k9;

            beam.kel = kEl;
            //Creates T-matrix to adjust element to global axis
            LA.Matrix<double> T = TransformationMatrix(beam);
            LA.Matrix<double> tT = T.Transpose();
            LA.Matrix<double> tTk = tT.Multiply(kEl);
            LA.Matrix<double> tTkt = tTk.Multiply(T);
            return tTkt;
        }

        public LA.Matrix<double> TransformationMatrix(BeamElement beam)
        {

            var p1 = beam.Line.From;
            var p2 = beam.Line.To;

            double alpha = beam.Alpha;

            double l = beam.Length;

            double cx = (p2.X - p1.X) / l;
            double cy = (p2.Y - p1.Y) / l;
            double cz = (p2.Z - p1.Z) / l;

            double c1 = Math.Cos(alpha);
            double s1 = Math.Sin(alpha);
            double cxz = Math.Round(Math.Sqrt(Math.Pow(cx, 2.0) + Math.Pow(cz, 2.0)), 6);

            Matrix<double> t;
            if (Math.Round(cx, 6) == 0 && Math.Round(cz, 6) == 0)
            {
                t = Matrix<double>.Build.DenseOfArray(new double[,] {
                    { 0, cy, 0},
                    { -cy*c1, 0, s1},
                    { cy*s1, 0, c1}
                });
            }
            else 
            {
                t = Matrix<double>.Build.DenseOfArray(new double[,] {
                    { cx, cy, cz},
                    { (-cx*cy*c1 - cz*s1) / cxz, cxz*c1, (-cy*cz*c1 + cx*s1) / cxz},
                    { (cx*cy*s1 - cz*c1) / cxz, -cxz*s1, (cy*cz*s1 + cx*c1) / cxz}
                });
            }

            var T = t.DiagonalStack(t);
            T = T.DiagonalStack(T);

            return T;

        }

        public List<double> GetForceList(LA.Matrix<double> forces)
        {
            List<double> Forcelist = new List<double>();
            for (int i = 0; i < forces.RowCount; i++)
            {
                Forcelist.Add(forces[i, 0]);
            }
            return Forcelist;
        }

        public static Matrix<double> CrossProduct(Matrix<double> a, Matrix<double> b)
        {
            if (a.RowCount != 1 || b.RowCount != 1 || a.ColumnCount != 3 || b.ColumnCount != 3)
            {
                throw new ArgumentException("Both matrices must be 1x3.");
            }

            double a2b3 = a[0, 1] * b[0, 2];
            double a3b2 = a[0, 2] * b[0, 1];
            double a3b1 = a[0, 2] * b[0, 0];
            double a1b3 = a[0, 0] * b[0, 2];
            double a1b2 = a[0, 0] * b[0, 1];
            double a2b1 = a[0, 1] * b[0, 0];

            return Matrix<double>.Build.DenseOfArray(new double[,] {
                { a2b3 - a3b2, a3b1 - a1b3,a1b2 - a2b1 }
            });
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
                            globalKsup.SetSubMatrix(idN*6, 0, row);
                            globalKsup.SetSubMatrix(0, idN*6, col);
                            globalKsup[idN*6, idN*6] = 1;
                        }
                        if (support.Ty == true)
                        {
                            globalKsup.SetSubMatrix(idN * 6 + 1, 0, row);
                            globalKsup.SetSubMatrix(0, idN * 6 + 1 , col);
                            globalKsup[idN * 6 + 1, idN * 6 + 1] = 1;
                        }
                        if (support.Tz == true)
                        {
                            globalKsup.SetSubMatrix(idN*6 +2, 0, row);
                            globalKsup.SetSubMatrix(0, idN*6 +2, col);
                            globalKsup[idN*6 +2, idN*6 +2] = 1;
                        }
                        if (support.Rx == true)
                        {
                            globalKsup.SetSubMatrix(idN * 6 + 3, 0, row);
                            globalKsup.SetSubMatrix(0, idN * 6 + 3, col);
                            globalKsup[idN * 6 + 3, idN * 6 + 3] = 1;
                        }
                        if (support.Ry == true)
                        {
                            globalKsup.SetSubMatrix(idN * 6 + 4, 0, row);
                            globalKsup.SetSubMatrix(0, idN * 6 + 4, col);
                            globalKsup[idN * 6 + 4, idN * 6 + 4] = 1;
                        }
                        if (support.Rz == true)
                        {
                            globalKsup.SetSubMatrix(idN * 6 + 5, 0, row);
                            globalKsup.SetSubMatrix(0, idN * 6 + 5, col);
                            globalKsup[idN * 6 + 5, idN * 6 + 5] = 1;
                        }
                    }
                }
            }
            return globalKsup;
        }

        public LA.Matrix<double> GetMel(BeamElement beam, bool lumped)
        {
            int dof = beam.ElDof;  // dof per element

            //gets length of element
            Node startNode = beam.StartNode;
            double z1 = startNode.Point.Z;
            double x1 = startNode.Point.X;
            double y1 = startNode.Point.Y;

            Node endNode = beam.EndNode;
            double z2 = endNode.Point.Z;
            double x2 = endNode.Point.X;
            double y2 = endNode.Point.Y;
            double l = beam.Length;
            double mTot = beam.Height * beam.Width * beam.Rho * beam.Length;
            double m = beam.Height * beam.Width * beam.Rho;
            double A = beam.Height * beam.Width;
            double Ix = (1.0 / 12.0) * m * (Math.Pow(beam.Height, 2.0) + Math.Pow(beam.Width, 2.0));
            double rx2 = Ix / A;
            double a = l / 2.0;


            // mass element matrix of right size filled with zeros
            LA.Matrix<double> mEl = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            mEl[0, 0] = mEl[6, 6] = Ix; //70.0;
            mEl[0, 6] = mEl[6, 0] = 35.0;
            mEl[1, 1] = mEl[2, 2] = mEl[7, 7] = mEl[8, 8] = 78.0;
            mEl[1, 5] = mEl[8, 10] = mEl[10, 8] = mEl[5, 1] = 22.0 * a;
            mEl[1, 7] = mEl[7, 1] = mEl[2, 8] = mEl[8, 2] = 27.0;
            mEl[1, 11] = mEl[11, 1] = mEl[4, 8] = mEl[8, 4] = -13.0 * a;
            mEl[2, 10] = mEl[10, 2] = mEl[5, 7] = mEl[7, 5] = 13.0 * a;
            mEl[3, 3] = mEl[9, 9] = 70.0 * rx2;
            mEl[3, 9] = mEl[9, 3] = -35.0 * rx2;
            mEl[4, 4] = mEl[5, 5] = mEl[10, 10] = mEl[11, 11] = 8.0 * Math.Pow(a, 2.0);
            mEl[4, 10] = mEl[10, 4] = mEl[5, 11] = mEl[11, 5] = -6.0 * Math.Pow(a, 2.0);
            mEl[7, 11] = mEl[11, 7] = mEl[2, 4] = mEl[4, 2] = -22.0 * a;

            mEl *= ((m * a) / 105.0);


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

            LA.Matrix<double> T = TransformationMatrix(beam);
            LA.Matrix<double> tT = T.Transpose();
            LA.Matrix<double> tTm = tT.Multiply(mEl);
            LA.Matrix<double> tTmt = tTm.Multiply(T);
            return tTmt;
        }
        public LA.Matrix<double> BuildGlobalM(int dof, List<BeamElement> elements, bool lumped)
        {
            // the only difference between this function and BuildGlobalK is that
            // BuildGlobalM accounts for lumped mass. Consider making this one function later
            LA.Matrix<double> globalM = LA.Matrix<double>.Build.Dense(dof, dof, 0);

            foreach (var element in elements)
            {
                int nDof = element.ElDof/2; //dof per node
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
                            supMatrix.SetSubMatrix(idN * 6, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 6, col);
                            supMatrix[idN * 6, idN * 6] = 1;
                        }
                        if (support.Ty == true)
                        {
                            supMatrix.SetSubMatrix(idN * 6 + 1, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 6 + 1, col);
                            supMatrix[idN * 6 + 1, idN * 6 + 1] = 1;
                        }
                        if (support.Tz == true)
                        {
                            supMatrix.SetSubMatrix(idN * 6 + 2, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 6 + 2, col);
                            supMatrix[idN * 6 + 2, idN * 6 + 2] = 1;
                        }
                        if (support.Rx == true)
                        {
                            supMatrix.SetSubMatrix(idN * 6 + 3, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 6 + 3, col);
                            supMatrix[idN * 6 + 3, idN * 6 + 3] = 1;
                        }
                        if (support.Ry == true)
                        {
                            supMatrix.SetSubMatrix(idN * 6 + 4, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 6 + 4, col);
                            supMatrix[idN * 6 + 4, idN * 6 + 4] = 1;
                        }
                        if (support.Rz == true)
                        {
                            supMatrix.SetSubMatrix(idN * 6 + 5, 0, row);
                            supMatrix.SetSubMatrix(0, idN * 6 + 5, col);
                            supMatrix[idN * 6 + 5, idN * 6 + 5] = 1;
                        }
                    }
                }
            }
            return supMatrix;
        }


        public LA.Matrix<double> BuildC(LA.Matrix<double> M, LA.Matrix<double> K, double zeta, double wi, double wj)
        {
            //LA.Matrix<double> W = EigenValue(K, M);
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

    }
}