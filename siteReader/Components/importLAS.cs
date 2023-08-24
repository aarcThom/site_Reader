using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.IO;
using Rhino;
using siteReader.Params;
using Rhino.Geometry;
using Rhino.DocObjects;
using siteReader.Methods;

namespace siteReader.Components
{
    public class importLAS : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public importLAS()
          : base("import LAS", "impLAS",
            "Import a LAS file",
            "SiteReader", "Point Clouds")
        {
        }

        //FIELDS
        private string _prevPath = string.Empty;

        private List<Mesh> _cropShapes;
        private List<Mesh> _prevCropShapes;
        private bool _insideCrop = true;
        private bool _prevInside = true;


        private AsprCld _asprCld;
        private List<string> _headerOut;
        private List<string> _vlrOut;

        //view attributes
        private float _cloudDensity = 0f;
        private bool _importCloud = false;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file path", "path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
            pManager.AddMeshParameter("Crop Shape", "Crop", "Provide breps or meshes to crop your cloud upon import.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager.AddBooleanParameter("Inside Crop", "InCrp", "If set to true (default), pts will be kept inside the crop shape. " +
                "False will retain points outside the crop shape.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "header", "Header information.", GH_ParamAccess.list);
            pManager.AddTextParameter("VLR", "VLR", "Variable length records - if present in file.",
                GH_ParamAccess.list);
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //VARIABLES---------------------------------------
            // Input variables
            string currentPath = string.Empty;



            //TEST INPUTS-------------------------------------
            // Is input empty?
            if (!DA.GetData(0, ref currentPath))
            {

                // I'm not sure if there is a way to clear the cloud once file is disconnected
                // I think that the component will not run since DAparam 01 is not optional...
                if (_asprCld != null)
                {
                    _asprCld = null; //clear the cloud if need be (doesn't work)
                    _prevPath = String.Empty;
                } 
                return;
            } 

            // Test if file exists
            if (!File.Exists(currentPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot find file");
                return;
            }

            //is .las or .laz?
            if (!Utility.TestLasExt(currentPath))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You must provide a valid .las or .laz file.");
                return;
            }

            List<Mesh> cropShps = new List<Mesh>();
            //crop mesh input
            if(DA.GetDataList(1, cropShps))
            {
                _cropShapes = cropShps;
            }


            //inside crop
            DA.GetData(2, ref _insideCrop);



            //initial import
            if (_prevPath != currentPath)
            {
                _asprCld = new AsprCld(currentPath);
                _asprCld.displayDensity = _cloudDensity;

                _headerOut = Utility.FloatDictGhOut(_asprCld.header, this);
                _vlrOut = Utility.StringDictGhOut(_asprCld.vlr);

                _prevPath = currentPath;

                if (_importCloud) 
                { 
                    GetCloud(DA, overRide: true); 
                } 
                else
                {
                    _asprCld.displayDensity = 2; // setting the cloud density above 1 so that the getCloud method triggers on user button click
                }
            }

            //user updates cropshape or inside bool
            if ((_prevCropShapes != _cropShapes || _prevInside != _insideCrop) && _importCloud)
            {
                GetCloud(DA, overRide: true);
            }

            //user updates density
            GetCloud(DA);


            DA.SetDataList(0, _headerOut);
            DA.SetDataList(1, _vlrOut);
            
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("FF31124B-CEA9-474D-9C1A-FB5132D77D74");


        //PREVIEW OVERRIDES AND UI METHODS ---------------------------------------------------

        //methods for passing values from UI controller
        public void SetVal(float value)
        {
            _cloudDensity = value;
        }

        public void SetImport(bool import)
        {
            _importCloud = import;
        }

        public void ZoomCloud()
        {
            if (_importCloud && _asprCld.ptCloud != null)
            {
                var bBox = _asprCld.ptCloud.GetBoundingBox(true);
                RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.ZoomBoundingBox(bBox);
                RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
            }
        }

        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.DensityZoom(this, SetVal, SetImport, ZoomCloud);
        }

        //drawing the point cloud if preview is enabled
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_asprCld != null && _asprCld.ptCloud != null && _importCloud)
            {
                args.Display.DrawPointCloud(_asprCld.ptCloud, 2);
            }

        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get
            {
                if (_asprCld != null && _asprCld.ptCloud != null && _importCloud)
                {
                    return _asprCld.ptCloud.GetBoundingBox(true);
                }

                return base.ClippingBox;

            }
        }

        //need to override this to be previewable despite having no geo output with preview method
        public override bool IsPreviewCapable => true;


        //OTHER METHODS ------------------------------------------------------------
        private void GetCloud(IGH_DataAccess da, bool overRide = false)
        {
            // I added the override bool to initialize the pointcloud regardless of import status when a new file is referenced
            if (_asprCld != null && _asprCld.displayDensity != _cloudDensity && _importCloud || overRide)
            {
                _asprCld.displayDensity = _cloudDensity;
                _asprCld.GetPointCloud(_cropShapes, _insideCrop);
                da.SetData(2, new AsprCld(_asprCld));
                RhinoDoc.ActiveDoc.Views.Redraw();

                //update the crop shapes and bool check
                _prevCropShapes = _cropShapes;
                _prevInside = _insideCrop;
            }
        }
    }
}