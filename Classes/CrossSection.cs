using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM.Classes
{
    internal class CrossSection
    {
        public double Height;
        public double Width;
        public double YoungsMod;
        public double Rho;

        public CrossSection() { }

        public CrossSection(double height, double width, double youngsMod, double rho)
        {
            this.Height = height;
            this.Width = width;
            this.YoungsMod = youngsMod;
            this.Rho = rho;
        }
    }
}
