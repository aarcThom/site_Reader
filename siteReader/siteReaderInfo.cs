using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace siteReader
{
    public class siteReaderInfo : GH_AssemblyInfo
    {
        public override string Name => "siteReader";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("9D71A9E6-482C-413D-9E33-E379A2E5AB72");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}