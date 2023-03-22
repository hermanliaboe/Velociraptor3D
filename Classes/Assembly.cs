using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class Assembly
    {
        public List<BeamElement> BeamList;
        public List<Support> SupportList;
        public List<Load> LoadList;
        public List<Node> NodeList;


        public Assembly() { }

        public Assembly(List<BeamElement> beamList, List<Support> supportList, List<Load> loadList, List<Node> nodeList)
        {
            this.BeamList = beamList;
            this.SupportList = supportList;
            this.LoadList = loadList;
            this.NodeList = nodeList;
        }
    }
}
