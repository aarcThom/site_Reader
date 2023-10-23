using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Components.Clouds;

namespace siteReader.Components
{
    public class ImportShapeFile : SiteReaderBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================
        public ImportShapeFile()
          : base("Import Shape File", "ImpShp",
              "Import a shape File", "GIS")
        {

            //IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {

        }

        //PREVIEW AND UI ==============================================================================================

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("F0284E1D-C6B6-4C98-8CEC-200F07B2D234");
    }
}