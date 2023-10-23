using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino;
using siteReader.Params;
using Rhino.Geometry;
using siteReader.Methods;

namespace siteReader.Components.Clouds
{
    public class ImportLas : CloudDisplay
    {
        //FIELDS ======================================================================================================
        private string _prevPath = string.Empty;

        private List<Mesh> _cropShapes;
        private List<Mesh> _prevCropShapes;
        private bool _insideCrop = true;
        private bool _prevInside = true;

        private List<string> _headerOut;
        private List<string> _vlrOut;

        private float _cloudDensity = 0f;

        //CONSTRUCTORS ================================================================================================
        public ImportLas()
            : base("import LAS", "impLAS",
                "Import a LAS file",
                "Point Clouds")
        {
            // IconPath = "siteReader.Resources...";
            ImportCld = false;
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file Path", "Path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
            pManager.AddMeshParameter("Crop Shape", "Crop", "Provide breps or meshes to crop your cloud upon import.",
                GH_ParamAccess.list);

            pManager[1].Optional = true;
            pManager.AddBooleanParameter("Inside Crop", "InCrp",
                "If set to true (default), pts will be kept inside the crop shape. " +
                "False will retain points outside the crop shape.", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "Header", "Header information.", GH_ParamAccess.list);
            pManager.AddTextParameter("VLR", "VLR", "Variable length records - if present in file.",
                GH_ParamAccess.list);

            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld",
                "A point cloud linked with ASPRS data", GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string currentPath = string.Empty;
            if (!DA.GetData(0, ref currentPath))
            {
                // I'm not sure if there is a way to clear the cloud once file is disconnected
                // I think that the component will not run since DAparam 01 is not optional...
                if (Cld != null)
                {
                    Cld = null; //clear the cloud if need be (doesn't work)
                    _prevPath = string.Empty;
                }
                return;
            }

            if (!File.Exists(currentPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot find file");
                return;
            }

            if (!Utility.TestLasExt(currentPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You must provide a valid .las or .laz file.");
                return;
            }

            var cropShapes = new List<Mesh>();
            if (DA.GetDataList(1, cropShapes))
            {
                _cropShapes = cropShapes;
            }

            DA.GetData(2, ref _insideCrop);

            //initial import
            if (_prevPath != currentPath)
            {
                Cld = new AsprCld(currentPath)
                {
                    DisplayDensity = _cloudDensity
                };

                _headerOut = FloatDictGhOut(Cld.Header, this);
                _vlrOut = StringDictGhOut(Cld.Vlr);

                _prevPath = currentPath;

                if (ImportCld == true)
                {
                    GetCloud(DA, overRide: true);
                }
                else
                {
                    // setting the cloud density above 1 so that the getCloud method triggers on user button click
                    Cld.DisplayDensity = 2;
                }
            }

            //user updates crop shape or inside bool
            if ((_prevCropShapes != _cropShapes || _prevInside != _insideCrop) && ImportCld == true)
            {
                GetCloud(DA, overRide: true);
            }

            //user updates density
            GetCloud(DA);

            DA.SetDataList(0, _headerOut);
            DA.SetDataList(1, _vlrOut);
        }


        //PREVIEW AND UI ==============================================================================================
        public void SetVal(float value)
        {
            _cloudDensity = value;
        }

        public void SetImport(bool import)
        {
            ImportCld = import;
        }

        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.DensityZoom(this, SetVal, SetImport, ZoomCloud);
        }

        //UTILITY METHODS =============================================================================================

        /// <summary>
        /// Set's the ptcloud in the asprCld object to density and sets component output.
        /// </summary>
        /// <param name="da"></param>
        /// <param name="overRide"></param>
        private void GetCloud(IGH_DataAccess da, bool overRide = false)
        {
            // override bool to initialize the pointcloud regardless of import status when a new file is referenced
            if (Cld != null && Cld.DisplayDensity != _cloudDensity && ImportCld == true || overRide && Cld != null)
            {
                Cld.DisplayDensity = _cloudDensity;
                Cld.GetPointCloud(_cropShapes, _insideCrop);
                da.SetData(2, new AsprCld(Cld));
                RhinoDoc.ActiveDoc.Views.Redraw();

                //update the crop shapes and bool check
                _prevCropShapes = _cropShapes;
                _prevInside = _insideCrop;
            }
        }

        /// <summary>
        /// Formats Header dictionary for GH Textural output
        /// </summary>
        /// <param name="floatDict"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        private List<string> FloatDictGhOut(Dictionary<string, float> floatDict, GH_Component owner)
        {
            List<string> ghOut = new List<string>();

            if (floatDict.Count == 0)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "LAS Header not found. The LAS spec. needs a Header. " +
                    "Double check your data source as this will probably cause errors down the road.");
                return ghOut;
            }

            foreach (string key in floatDict.Keys)
            {
                ghOut.Add($"{key} : {floatDict[key]}");
            }
            return ghOut;
        }

        /// <summary>
        /// formats VLR dictionary for GH textual output
        /// </summary>
        /// <param name="stringDict"></param>
        /// <returns>string list for GH output</returns>
        private List<string> StringDictGhOut(Dictionary<string, string> stringDict)
        {
            List<string> ghOut = new List<string>();

            if (stringDict.Count == 0)
            {
                return new List<string> { "No VLRs found." };
            }

            foreach (string key in stringDict.Keys)
            {
                ghOut.Add($"{key} : {stringDict[key]}");
            }
            return ghOut;
        }

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("FF31124B-CEA9-474D-9C1A-FB5132D77D74");
    }
}