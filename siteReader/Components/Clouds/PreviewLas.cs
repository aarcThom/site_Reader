using Grasshopper.Kernel;
using System;
using System.IO;
using Rhino;
using siteReader.Params;
using siteReader.Methods;
using System.Collections.Generic;

namespace siteReader.Components.Clouds
{
    public class PreviewLas : CloudDisplay
    {
        //CONSTRUCTORS ================================================================================================
        public PreviewLas()
          : base("preview LAS", "pvwLAS",
            "Import a preview of a LAS file. It's advised to do this before a full import.",
            "Point Clouds")
        {
            // IconPath = "siteReader.Resources...";
            ImportCld = false;
        }

        //FIELDS ======================================================================================================
        private string _prevPath = string.Empty;

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file Path", "Path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string currentPath = string.Empty;
            if (!DA.GetData(0, ref currentPath)) return;

            var fTypes = new List<string>() { ".las", ".laz" };
            if (!Utility.TestFile(currentPath, fTypes, out string msg))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                return;
            }

            //initial import
            if (_prevPath != currentPath)
            {
                Cld = new AsprCld(currentPath);
                _prevPath = currentPath;

                if (ImportCld == true)
                {
                    Cld.GetPreview();
                    RhinoDoc.ActiveDoc.Views.Redraw();
                }
            }

            if (ImportCld == true)
            {
                if (Cld.PtCloud == null || Cld.PtCloud.Count == 0)
                {
                    Cld.GetPreview();
                    RhinoDoc.ActiveDoc.Views.Redraw();
                }
            }
        }

        //PREVIEW AND UI ==============================================================================================
        public void SetImport(bool import)
        {
            ImportCld = import;
        }
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.PreviewImport(this, SetImport, ZoomCloud);
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("09C9FADB-ACD3-40C8-8BDE-A64E2A9E3EB1");
    }


}