using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using FEM.Classes;
using FEM.Properties;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace FEM.Components
{
    public class AssembleModel : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AssembleModel class.
        /// </summary>
        public AssembleModel()
          : base("AssembleModel", "Nickname",
              "Assembles elements, loads and supports into an Assembly object.",
              "Masters", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //trying some stuff here
            pManager.AddGenericParameter("Beams", "beams", "Input for all beams", GH_ParamAccess.list);
            pManager.AddGenericParameter("Supports", "sups", "Input for all supports", GH_ParamAccess.list);
            pManager.AddGenericParameter("Loads", "loads", "Input for all loads", GH_ParamAccess.list);
            pManager.AddGenericParameter("Nodes", "nodes", "Input for all nodes", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Modell", "modell", "Assembled modell", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<BeamElement> beams = new List<BeamElement>();
            List<Support> supports = new List<Support>();
            List<Load> loads = new List<Load>();
            List<Node> nodes = new List<Node>();


            DA.GetDataList(0,  beams);
            DA.GetDataList(1,  supports);
            DA.GetDataList(2,  loads);
            DA.GetDataList(3, nodes);


            //Check for where the support is located, and if found the correct beam gets new BC

            foreach (Support sup in supports)
            {
                foreach (BeamElement b in beams) 
                {
                    Node startNode = b.StartNode;
                    if (startNode.Point == sup.Point)
                    {
                        startNode.XBC = sup.Tx;
                        startNode.ZBC = sup.Tz;
                        startNode.RyBC = sup.Ry;
                    }

                    Node endNode = b.EndNode;
                    if (endNode.Point == sup.Point)
                    {
                        endNode.XBC = sup.Tx;
                        endNode.ZBC = sup.Tz    ;
                        endNode.RyBC = sup.Ry;
                    }
                }                    
             }

            foreach (Load load in loads)
            {
                foreach (Node node in nodes)
                {
                    if (node.Point == load.Point)
                    {
                        load.NodeID = node.GlobalID;
                    }
                }
            }

            Assembly assembly = new Assembly(beams, supports, loads, nodes);

            DA.SetData(0, assembly);
        }

    

            
        

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                //return Resources.assembly.bmp;
                
                return Resources.assembly;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8F0FF416-72DD-4B79-9F02-1A30BCEE2AE9"); }
        }
    }
}