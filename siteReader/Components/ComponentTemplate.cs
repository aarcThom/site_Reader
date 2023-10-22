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

            // clear the UI if cloud input disconnected. RedrawCanvas() only needed for custom UI components that
            // need to reset (ie. graphs) when cloud input disconnected. If you don't have a custom UI, you can just
            // leave this at return or reset whatever values needed.
            if (CldInput == false)
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