using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Params;

namespace siteReader.Components
{
    public class ExtractPoints : SiteReaderBase
    {
        //CONSTRUCTORS ================================================================================================
        public ExtractPoints()
          : base("Extract Points", "To Pts",
              "Convert a Site Reader Cloud to a list of points", "Point Clouds")
        {

            //IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "pts", "A list of 3d Points", GH_ParamAccess.list);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            if (CldInput == false)
            {
                return;
            }

            var pts = Cld.PtCloud.GetPoints().ToList();
            DA.SetDataList(0, pts);

        }

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("2E92AF16-677A-4B31-9C08-95450F91B87F");
    }
}