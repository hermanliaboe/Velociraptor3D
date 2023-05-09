using System;
using System.Collections.Generic;
using FEM3D.Classes;
using FEM3D.Properties;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Geometry;
using Rhino.Geometry;

namespace FEM3D.Components
{
    public class CreateBeamElements3D : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateBeamElements3D class.
        /// </summary>
        public CreateBeamElements3D()
          : base("CreateBeamElements", "Nickname",
              "Line to element with two nodes",
              "Masters3D", "Model3D")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "ls", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("CrossSection", "cs", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("3D", "3D", "if True 3D 12DOF element, if False 2D 6DOF element", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Alpha", "", "Rotation about local x-axis", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements", "els", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes", "ns", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Line> lines = new List<Line>();
            CrossSection cs = new CrossSection();
            bool dof = true;
            double alpha = 0.0;
            DA.GetDataList(0, lines);
            DA.GetData(1, ref cs);
            DA.GetData(2, ref dof);
            DA.GetData(3, ref alpha);

            List<BeamElement> beams = new List<BeamElement>();
            Dictionary<Point3d, Node> existingNodes = new Dictionary<Point3d, Node>();
            List<Node> nodes = new List<Node>();

            int idc = 0; // element global ID count
            int bidc = 0; // beam ID count
            foreach (Line line in lines)
            {
                Point3d stPt = line.From;
                Point3d ePt = line.To;

                BeamElement element = new BeamElement(bidc, line);

                double l = element.Length;

                double cx = Math.Round((ePt.X - stPt.X) / l, 9);
                double cy = Math.Round((ePt.Y - stPt.Y) / l, 9);
                double cz = Math.Round((ePt.Z - stPt.Z) / l, 9);

                double c1 = Math.Round(Math.Cos(alpha), 9);
                double s1 = Math.Round(Math.Sin(alpha), 9);
                double cxz = Math.Round(Math.Sqrt(Math.Pow(cx, 2.0) + Math.Pow(cz, 2.0)), 9);


                if (Math.Round(cx, 9) == 0.0 && Math.Round(cz, 9) == 0.0)
                {
                    var xl = new Vector3d(0, cy, 0);
                    var yl = new Vector3d(-cy * c1, 0, s1);
                    var zl = new Vector3d(cy * s1, 0, c1);

                    element.xl = xl; element.yl = yl; element.zl = zl;

                    
                }
                else
                {
                    var xl = new Vector3d(cx, cy, cz);
                    var yl = new Vector3d(Math.Round((-cx * cy * c1 - cz * s1) / cxz, 9), Math.Round(cxz * c1, 9), Math.Round((-cy * cz * c1 + cx * s1) / cxz, 9));
                    var zl = new Vector3d(Math.Round((cx * cy * s1 - cz * c1) / cxz, 9), Math.Round(-cxz * s1, 9), Math.Round((cy * cz * s1 + cx * c1) / cxz, 9));

                    element.xl = xl; element.yl = yl; element.zl = zl;

                }

                /*
                var lineVec = line.Direction;
                var planeNormal = new Rhino.Geometry.Plane(stPt, lineVec);

                var xl = planeNormal.ZAxis;
                var zl = new Rhino.Geometry.Vector3d();
                var unitZ = new Vector3d(0, 0, 1);

                var dotYunitZ = Rhino.Geometry.Vector3d.Multiply(unitZ, planeNormal.YAxis);
                var dotXunitZ = Rhino.Geometry.Vector3d.Multiply(unitZ, planeNormal.XAxis);

                if (dotYunitZ != 0)
                {
                    zl = planeNormal.YAxis;
                }

                else
                {
                    zl = planeNormal.XAxis;
                }
                
                if (zl.Z <  0)
                {
                    zl.Reverse();
                }

                var yl = Rhino.Geometry.Vector3d.CrossProduct(xl, zl);
                yl.Unitize();
                yl.Reverse();

             

                element.xl = xl;
                element.yl = yl;
                element.zl = zl;

                */

                bidc++;
                if (existingNodes.ContainsKey(stPt))
                {
                    element.StartNode = existingNodes[stPt];
                }
                else
                {
                    Node sNode = new Node(0, idc, stPt);
                    existingNodes.Add(stPt, sNode);
                    element.StartNode = sNode;
                    nodes.Add(sNode);
                    idc++;
                }
                if (existingNodes.ContainsKey(ePt))
                {
                    element.EndNode = existingNodes[ePt];
                }
                else
                {
                    Node eNode = new Node(0, idc, ePt);
                    existingNodes.Add(ePt, eNode);
                    element.EndNode = eNode;
                    nodes.Add(eNode);
                    idc++;
                }
                element.Height = cs.Height;
                element.Width = cs.Width;
                element.YoungsMod = cs.YoungsMod;
                element.Rho = cs.Rho;
                element.ShearMod = cs.ShearMod;
                element.Alpha = alpha;
                if (dof) { element.ElDof = 12; }
                else { element.ElDof = 6; }
                beams.Add(element);
            }

            DA.SetDataList(0, beams);
            DA.SetDataList(1, nodes);


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
                return Resources.beam;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("AFD7E891-D643-4CA7-981D-3989CD1E253D"); }
        }
    }
}