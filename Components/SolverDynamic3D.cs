using FEM3D.Classes;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;
using MathNet.Numerics;
using MathNet.Numerics.OdeSolvers;
using MathNet.Numerics.LinearAlgebra.Factorization;
using LA = MathNet.Numerics.LinearAlgebra;


using Rhino.Commands;
using Rhino.Render;
using System.IO;
using Grasshopper.Kernel.Types;
using System.Numerics;
using System.Linq;
using GH_IO;
using MathNet.Numerics.LinearAlgebra.Complex;
using FEM3D.Properties;
using MathNet.Numerics.LinearAlgebra;

namespace FEM3D.Components
{
    public class SolverDynamic3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SolverDynamic3D class.
        /// </summary>
        public SolverDynamic3D()
          : base("DynamicSolver3D", "femmern",
            "FEM3D solver with Newmark method",
            "Masters3D", "FEM3D")
        {
        }



        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly", "ass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("Time Step", "", "Time step for the Newmark method. Default 0.01", GH_ParamAccess.item, 0.01);
            pManager.AddNumberParameter("Beta", "", "Beta value for the Newmark method. Default 1/4 (average acceleration)", GH_ParamAccess.item, 1.0 / 4.0);
            pManager.AddNumberParameter("Gamma", "", "Gamme value for the Newmark method. Default 1/2 (average acceleration)", GH_ParamAccess.item, 1.0 / 2.0);
            pManager.AddNumberParameter("Time", "", "Run time for the Newmark method. Default 5 seconds", GH_ParamAccess.item, 5.0);
            pManager.AddNumberParameter("Damping", "", "Damping parameter for the structure. Default 0.05", GH_ParamAccess.item, 0.05);
            pManager.AddGenericParameter("d0", "d0", "Initial displacement DenseMatrix(dof, 1). 'Displacements Vec' from static solver.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("f0 zero","","set to true if you want zero force applied", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Global Stiffness Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Stiffness Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Applied Force Vector", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Lumped Mass Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Lumped Mass Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Consistent Mass Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Consistent Mass Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Damping Matrix", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Global Damping Matrix reduced", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Displacements", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Velocity", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Nodal Forces", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Natural Frequencies [Hz]", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Natural Frequencies [Hz], sorted", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("z Displacements", "", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("z Displacemtns RM", "", "", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Classes.Assembly model = new Classes.Assembly();
            double T = 5.0;
            double dt = 0.01;
            double beta = 1.0 / 4.0;
            double gamma = 1.0 / 2.0;
            double damping = 0.05;
            bool fBool = false;
            DA.GetData(0, ref model);
            DA.GetData(1, ref dt);
            DA.GetData(2, ref beta);
            DA.GetData(3, ref gamma);
            DA.GetData(4, ref T);
            DA.GetData(5, ref damping);
            DA.GetData(7, ref fBool);

            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;
            int dof = nodes.Count * 6;

            //Creation of matrices
            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildSupMat(dof, globalK, supports, nodes);

            LA.Matrix<double> globalLumpedM = matrices.BuildGlobalM(dof, elements, true);
            LA.Matrix<double> globalConsistentM = matrices.BuildGlobalM(dof, elements, false);
            LA.Matrix<double> globalLumpedMsup = matrices.BuildSupMat(dof, globalLumpedM, supports, nodes);
            LA.Matrix<double> globalConsistentMsup = matrices.BuildSupMat(dof, globalConsistentM, supports, nodes);

            LA.Matrix<double> globalC = matrices.BuildC(globalConsistentM, globalK, damping, 0.1, 100);
            LA.Matrix<double> supC = matrices.BuildSupMat(dof, globalC, supports, nodes);
            LA.Matrix<double> f0 = matrices.BuildForceVector(loads, dof);

            //Usage of newmark

            LA.Matrix<double> d0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);

            LA.Matrix<double> v0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);
            DA.GetData(6, ref d0);




            Newmark(beta, gamma, dt, globalConsistentMsup, globalKsup, supC, f0, d0, v0, T, fBool, out LA.Matrix<double> displacements, out LA.Matrix<double> velocities);
            LA.Matrix<double> nodalForces = LA.Matrix<double>.Build.Dense(displacements.RowCount, displacements.ColumnCount);
            for (int i = 0; i < displacements.ColumnCount; i++)
            {
                nodalForces.SetSubMatrix(0, i, globalK.Multiply(displacements.SubMatrix(0, dof, i, 1)));
            }

            LA.Matrix<double> zDisplacements =  zDisp(displacements);


            var eigs = EigenValues(globalKsup, globalConsistentMsup);
            var natFreq = LA.Matrix<double>.Build.Dense(1, eigs.ColumnCount, 0);
            // Sort the natFreq matrix from smallest to largest using an array
            
            
            for (int i = 0; i < eigs.ColumnCount; i++)
            {
                natFreq[0, i] = Math.Sqrt(eigs[0, i]);
            }

            var sortedNatFreqArray = natFreq.ToRowMajorArray();
            Array.Sort(sortedNatFreqArray);

            // Convert the sorted array back into a MathNet.Numerics.LinearAlgebra.Matrix<double>
            var sortedNatFreqMatrix = Matrix<double>.Build.Dense(1, sortedNatFreqArray.Length);
            sortedNatFreqMatrix.SetRow(0, sortedNatFreqArray);


            Rhino.Geometry.Matrix rhinoMatrixK = CreateRhinoMatrix(globalK);
            Rhino.Geometry.Matrix rhinoMatrixKred = CreateRhinoMatrix(globalKsup);
            Rhino.Geometry.Matrix rhinoMatrixAppF = CreateRhinoMatrix(f0);
            Rhino.Geometry.Matrix rhinoMatrixLumpedM = CreateRhinoMatrix(globalLumpedM);
            Rhino.Geometry.Matrix rhinoMatrixLumpedMred = CreateRhinoMatrix(globalLumpedMsup);
            Rhino.Geometry.Matrix rhinoMatrixConsistentM = CreateRhinoMatrix(globalConsistentM);
            Rhino.Geometry.Matrix rhinoMatrixConsistentMred = CreateRhinoMatrix(globalConsistentMsup);
            Rhino.Geometry.Matrix rhinoMatrixC = CreateRhinoMatrix(globalC);
            Rhino.Geometry.Matrix rhinoMatrixCred = CreateRhinoMatrix(supC);
            Rhino.Geometry.Matrix rhinoMatrixNatFreq = CreateRhinoMatrix(natFreq);
            Rhino.Geometry.Matrix rhinoMatrixSortedNatFreq = CreateRhinoMatrix(sortedNatFreqMatrix);
            Rhino.Geometry.Matrix rhinoMatrixzDisplacements = CreateRhinoMatrix(zDisplacements);

            DA.SetData(0, rhinoMatrixK);
            DA.SetData(1, rhinoMatrixKred);
            DA.SetData(2, rhinoMatrixAppF);
            DA.SetData(3, rhinoMatrixLumpedM);
            DA.SetData(4, rhinoMatrixLumpedMred);
            DA.SetData(5, rhinoMatrixConsistentM);
            DA.SetData(6, rhinoMatrixConsistentMred);
            DA.SetData(7, rhinoMatrixC);
            DA.SetData(8, rhinoMatrixCred);
            DA.SetData(9, displacements);
            DA.SetData(10, velocities);
            DA.SetData(11, nodalForces);
            DA.SetData(12, rhinoMatrixNatFreq);
            DA.SetData(13, rhinoMatrixSortedNatFreq);
            DA.SetData(14, zDisplacements);
            DA.SetData(15, rhinoMatrixzDisplacements);
        }


        void Newmark(double beta, double gamma, double dt, LA.Matrix<double> M, LA.Matrix<double> K, LA.Matrix<double> C,
           LA.Matrix<double> f0, LA.Matrix<double> d0, LA.Matrix<double> v0, double T, bool fBool, out LA.Matrix<double> displacements,
           out LA.Matrix<double> velocities)
        {
            // d0 and v0 inputs are (dof, 1) matrices
            int dof = K.RowCount;
            var d = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            var v = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            var a = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            LA.Matrix<double> fTime = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);


            d.SetSubMatrix(0, dof, 0, 1, d0);
            v.SetSubMatrix(0, dof, 0, 1, v0);

            if (fBool)
            {
                fTime.SetSubMatrix(0, dof, 0, 1, f0);
            }


            // Initial calculation
            LA.Matrix<double> mInv = M.Inverse();
            var a0 = mInv.Multiply(f0 - C.Multiply(v0) - K.Multiply(d0));
            a.SetSubMatrix(0, dof, 0, 1, a0);

            for (int n = 0; n < d.ColumnCount - 1; n++)
            {
                // predictor step
                var dPred = d.SubMatrix(0, dof, n, 1) + dt * v.SubMatrix(0, dof, n, 1) + 0.5 * (1 - 2 * beta) * Math.Pow(dt, 2) * a.SubMatrix(0, dof, n, 1);
                var vPred = v.SubMatrix(0, dof, n, 1) + (1 - gamma) * dt * a.SubMatrix(0, dof, n, 1);

                // solution step
                // if force is a function of time, set F_n+1 to updated value (not f0)
                //fTime.SetSubMatrix(0,dof,n+1,1, fZeros);
                var fPrime = fTime.SubMatrix(0, dof, n + 1, 1) - C.Multiply(vPred) - K.Multiply(dPred);
                var mPrime = M + gamma * dt * C + beta * Math.Pow(dt, 2) * K;
                LA.Matrix<double> mPrimeInv = mPrime.Inverse();
                a.SetSubMatrix(0, n + 1, mPrimeInv.Multiply(fPrime));

                // connector step
                d.SetSubMatrix(0, dof, n + 1, 1, dPred + beta * Math.Pow(dt, 2) * a.SubMatrix(0, dof, n + 1, 1));
                v.SetSubMatrix(0, dof, n + 1, 1, vPred + gamma * dt * a.SubMatrix(0, dof, n + 1, 1));
            }
            velocities = v;
            displacements = d;
        }

        public Rhino.Geometry.Matrix CreateRhinoMatrix(LA.Matrix<double> matrix)
        {
            Rhino.Geometry.Matrix rhinoMatrix = new Rhino.Geometry.Matrix(matrix.RowCount, matrix.ColumnCount);
            for (int i = 0; i < matrix.RowCount; i++)
            {
                for (int j = 0; j < matrix.ColumnCount; j++)
                {
                    rhinoMatrix[i, j] = matrix[i, j];
                }
            }
            return rhinoMatrix;
        }

        public LA.Matrix<double> EigenValues(LA.Matrix<double> K, LA.Matrix<double> M)
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


        public LA.Matrix<double> zDisp(LA.Matrix<double> disp)
        {
            int n = disp.RowCount / 6;
            var zDisp = LA.Matrix<double>.Build.Dense(n, disp.ColumnCount);

            for (int i = 0; i < n; i++)
            {
                var dispNodei = disp.SubMatrix(i*6 + 2, 1, 0, disp.ColumnCount);
                zDisp.SetSubMatrix(i,0, dispNodei);
            }
            return zDisp;
        }




        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resouurces.IconForThisComponent;
                return Resources.SolverDyn;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0638E268-4278-4782-B52E-97E6D75E8CA4"); }
        }
    }
}