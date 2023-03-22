using FEM.Classes;
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
using FEM.Properties;
using System.Numerics;
using System.Linq;
using GH_IO;
using MathNet.Numerics.LinearAlgebra.Complex;

namespace FEM.Components
{
    public class DynamicSolver : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public DynamicSolver()
          : base("DynamicSolver", "femmern",
            "FEM solver with Newmark method",
            "Masters", "FEM")
        {
        }



        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Assembly", "ass", "", GH_ParamAccess.item);

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
            pManager.AddNumberParameter("Natural Frequencies [Hz]", "", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            Classes.Assembly model = new Classes.Assembly();
            DA.GetData(0, ref model);

            List<Load> loads = model.LoadList;
            List<BeamElement> elements = model.BeamList;
            List<Support> supports = model.SupportList;
            List<Node> nodes = model.NodeList;
            int dof = nodes.Count * 3;

            //Creation of matrices
            Matrices matrices = new Matrices();

            LA.Matrix<double> globalK = matrices.BuildGlobalK(dof, elements);
            LA.Matrix<double> globalKsup = matrices.BuildSupMat(dof, globalK, supports, nodes);

            LA.Matrix<double> globalLumpedM = matrices.BuildGlobalM(dof, elements, true);
            LA.Matrix<double> globalConsistentM = matrices.BuildGlobalM(dof, elements, false);
            LA.Matrix<double> globalLumpedMsup = matrices.BuildSupMat(dof, globalLumpedM, supports, nodes);   
            LA.Matrix<double> globalConsistentMsup = matrices.BuildSupMat(dof, globalConsistentM, supports, nodes);

            LA.Matrix<double> globalC = matrices.BuildC(globalLumpedM,globalKsup,0.05,0.1,100);
            LA.Matrix<double> supC = matrices.BuildSupMat(dof, globalC, supports, nodes);
            LA.Matrix<double> f0 = matrices.BuildForceVector(loads, dof);
           
            //Usage of newmark

            double T = 5.0;
            double dt = 0.01;
            double beta = 1.0 / 4.0;
            double gamma = 1.0 / 2.0;

            LA.Matrix<double> d0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);
            LA.Matrix<double> v0 = LA.Matrix<double>.Build.Dense(dof, 1, 0);


            Newmark(beta, gamma, dt, globalConsistentMsup, globalKsup, supC, f0, d0,v0,T, out LA.Matrix<double> displacements, out LA.Matrix<double> velocities);
            LA.Matrix<double> nodalForces = LA.Matrix<double>.Build.Dense(displacements.RowCount, displacements.ColumnCount);
            for (int i = 0;i < displacements.ColumnCount; i++)
            {
                nodalForces.SetSubMatrix(0, i, globalK.Multiply(displacements.SubMatrix(0, dof, i, 1)));
            }
            
            var eigs = EigenValues(globalKsup, globalConsistentMsup);
            var natFreq = new List<double>();
            for (int i = 0; i < eigs.ColumnCount; i++)
            {
                natFreq.Add(Math.Sqrt(eigs[0, i])/ (2 * Math.PI));
            }

            Rhino.Geometry.Matrix rhinoMatrixK = CreateRhinoMatrix(globalK);
            Rhino.Geometry.Matrix rhinoMatrixKred = CreateRhinoMatrix(globalKsup);
            Rhino.Geometry.Matrix rhinoMatrixAppF = CreateRhinoMatrix(f0);
            Rhino.Geometry.Matrix rhinoMatrixLumpedM = CreateRhinoMatrix(globalLumpedM);
            Rhino.Geometry.Matrix rhinoMatrixLumpedMred = CreateRhinoMatrix(globalLumpedMsup);
            Rhino.Geometry.Matrix rhinoMatrixConsistentM = CreateRhinoMatrix(globalConsistentM);
            Rhino.Geometry.Matrix rhinoMatrixConsistentMred = CreateRhinoMatrix(globalConsistentMsup);
            Rhino.Geometry.Matrix rhinoMatrixC = CreateRhinoMatrix(globalC);
            Rhino.Geometry.Matrix rhinoMatrixCred = CreateRhinoMatrix(supC);

      
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
            DA.SetDataList(12, natFreq);
        }


        void Newmark(double beta, double gamma, double dt, LA.Matrix<double> M, LA.Matrix<double> K, LA.Matrix<double> C, 
           LA.Matrix<double> f0, LA.Matrix<double> d0, LA.Matrix<double> v0, double T, out LA.Matrix<double> displacements, 
           out LA.Matrix<double> velocities)
        {
            // d0 and v0 inputs are (dof, 1) matrices
            int dof = K.RowCount;
            var d = LA.Matrix<double>.Build.Dense(dof ,((int)(T / dt)), 0);
            var v = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            var a = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);
            LA.Matrix<double> fTime = LA.Matrix<double>.Build.Dense(dof, ((int)(T / dt)), 0);


            d.SetSubMatrix(0,dof, 0, 1, d0);
            v.SetSubMatrix(0, dof, 0, 1 , v0);
            fTime.SetSubMatrix(0, dof, 0, 1 , f0);


            // Initial calculation
            LA.Matrix<double> mInv = M.Inverse();
            var a0 = mInv.Multiply(f0 - C.Multiply(v0) - K.Multiply(d0));
            a.SetSubMatrix(0,dof,0,1, a0);

            for (int n = 0; n < d.ColumnCount-1; n++)
            {
                // predictor step
                var dPred = d.SubMatrix(0,dof, n, 1) + dt * v.SubMatrix(0, dof, n, 1) + 0.5*(1 - 2*beta)*Math.Pow(dt, 2)*a.SubMatrix(0, dof, n, 1);
                var vPred = v.SubMatrix(0, dof, n, 1) + (1 - gamma) * dt * a.SubMatrix(0, dof, n, 1);

                // solution step
                // if force is a function of time, set F_n+1 to updated value (not f0)
                //fTime.SetSubMatrix(0,dof,n+1,1, fZeros);
                var fPrime = fTime.SubMatrix(0,dof, n+1, 1) - C.Multiply(vPred) - K.Multiply(dPred);
                var mPrime = M + gamma * dt * C + beta * Math.Pow(dt, 2) * K;
                LA.Matrix<double> mPrimeInv = mPrime.Inverse();
                a.SetSubMatrix(0,n + 1, mPrimeInv.Multiply(fPrime));

                // connector step
                d.SetSubMatrix(0, dof, n+1,1, dPred + beta * Math.Pow(dt, 2) * a.SubMatrix(0,dof,n+1,1));
                v.SetSubMatrix(0, dof, n+1,1, vPred + gamma * dt * a.SubMatrix(0,dof,n+1,1));
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



        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.SolverDyn;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("6A936E0F-DBB8-4FAA-8CF4-132EB5724007"); }
        }
    }
}