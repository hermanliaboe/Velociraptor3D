using System;
using System.Collections.Generic;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class CreateBeamElements : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Beam class.
        /// </summary>
        public CreateBeamElements()
          : base("CreateBeamElements", "Nickname",
              "Line to element with two nodes",
              "Masters", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "ls", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("CrossSection", "cs","",GH_ParamAccess.item) ;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements","els","", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes","ns","", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List <Line> lines = new List<Line>();
            CrossSection cs = new CrossSection();
            DA.GetDataList(0, lines);
            DA.GetData(1, ref cs);

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
                double l = 0;

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
            get { return new Guid("36B2609B-0264-4FD9-AFE9-631B3E6CACB5"); }
        }
    }
}