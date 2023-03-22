using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace FEM3D
{
    public class FEM3DInfo : GH_AssemblyInfo
    {
        public override string Name => "FEM3D";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("6228b52e-8c80-4f55-bf9c-b621cbae7c76");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}