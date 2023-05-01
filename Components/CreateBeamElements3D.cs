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
            DA.GetDataList(0, lines);
            DA.GetData(1, ref cs);
            DA.GetData(2, ref dof);

            List<BeamElement> beams = new List<BeamElement>();
            Dictionary<Point3d, Node> existingNodes = new Dictionary<Point3d, Node>();
            List<Node> nodes = new List<Node>();

            int idc = 0; // element global ID count
            int bidc = 0; // beam ID count
            foreach (Line line in lines)
            {
                Point3d stPt = line.From;
                Point3d ePt = line.To;
                var lineVec = line.Direction;
                var planeNormal = new Rhino.Geometry.Plane(stPt, lineVec);

                var xl = planeNormal.ZAxis;
                var yl = new Rhino.Geometry.Vector3d();

                if (planeNormal.YAxis.Z != 0)
                {
                    yl = planeNormal.YAxis;
                }
                else
                {
                    yl = planeNormal.XAxis;
                }

                if (yl.Z < 0)
                {
                    yl.Reverse();
                }

                var zl = Rhino.Geometry.Vector3d.CrossProduct(xl, yl);
                zl.Unitize();
                

                BeamElement element = new BeamElement(bidc, line, xl, yl, zl);

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