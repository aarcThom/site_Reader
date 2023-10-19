using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace siteReader.Components
{
    public class ComponentTemplate : CloudBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================
        public ComponentTemplate()
          : base("ComponentName", "Nickname",
              "Description", "Subcategory")
        {

            IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            // clear the UI if cloud input disconnected. Leave blank excepting the return if not needed
            if (_cldInput == false)
            {
                //CLEAR UI DATA HERE
                //Grasshopper.Instances.RedrawCanvas();
                return;
            }
        }

        //PREVIEW AND UI ==============================================================================================

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("31D0F86A-21AA-4AB1-A071-EB77551C4B70");
    }
}