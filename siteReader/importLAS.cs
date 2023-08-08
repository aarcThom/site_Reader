using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using Aardvark.Base;
using Rhino.Render;
using Rhino;

namespace siteReader
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
            "AARC", "siteReader")
        {
        }

        //FIELDS
        private string _prevPath = String.Empty;


        private LasPtCloud _fullPtCloud;
        private List<string> _headerOut;
        private List<string> _vlrOut;

        //view attributes
        private float _cloudDensity = 0f;
        private bool _previewCloud = false;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file path", "path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "header", "Header information.", GH_ParamAccess.list);
            pManager.AddTextParameter("VLR", "VLR", "Variable length records - if present in file.",
                GH_ParamAccess.list);

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
            string currentPath = String.Empty;


            //TEST INPUTS-------------------------------------
            // Is input empty?
            if (!DA.GetData(0, ref currentPath)) return;

            // Test if file exists
            if (!File.Exists(currentPath))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot find file");
                return;
            }

            //is .las or .laz?
            if (!Utility.TestLasExt(currentPath))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You must provide a valid .las or .laz file.");
                return;
            }


            //initial import
            if (_prevPath != currentPath)
            {
                _fullPtCloud = new LasPtCloud(currentPath);
                _fullPtCloud.maxDisplayDensity = _cloudDensity;

                _headerOut = Utility.FloatDictGhOut(_fullPtCloud.header, this);
                _vlrOut = Utility.StringDictGhOut(_fullPtCloud.vlr);

                _prevPath = currentPath;

                if (_previewCloud) GetCloud(overRide: true);
            }

            //user updates density
            GetCloud();


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

        public void SetPreview(bool preview)
        {
            _previewCloud = preview;
        }

        public void ZoomCloud()
        {
            if (_previewCloud && _fullPtCloud.rhinoPtCloud != null)
            {
                var bBox = _fullPtCloud.rhinoPtCloud.GetBoundingBox(true);
                RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.ZoomBoundingBox(bBox);
                RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
            }
        }

        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.BaseAttributes(this, SetVal, SetPreview, ZoomCloud);
        }



        //drawing the point cloud if preview is enabled
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_fullPtCloud.rhinoPtCloud != null && _previewCloud)
            {
                args.Display.DrawPointCloud(_fullPtCloud.rhinoPtCloud, 2);
            }

        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get
            {
                if (_fullPtCloud != null && _fullPtCloud.rhinoPtCloud != null && _previewCloud)
                {
                    return _fullPtCloud.rhinoPtCloud.GetBoundingBox(true);
                }

                return base.ClippingBox;

            }
        }

        //need to override this to be previewable despite having no geo output
        public override bool IsPreviewCapable => true;


        //OTHER METHODS ------------------------------------------------------------
        private void GetCloud(bool overRide = false)
        {
            // I added the override bool to initialize the pointcloud regardless of preview status when a new file is referenced
            if ((_fullPtCloud != null && _fullPtCloud.maxDisplayDensity != _cloudDensity && _previewCloud) || overRide)
                {
                _fullPtCloud.maxDisplayDensity = _cloudDensity;
                _fullPtCloud.GetPointCloud();
                RhinoDoc.ActiveDoc.Views.Redraw();
            }
        }
    }
}