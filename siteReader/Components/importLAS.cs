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
            "AARC", "siteReader")
        {
        }

        //FIELDS
        private string _prevPath = string.Empty;


        private AsprCld _asprCld;
        private List<string> _headerOut;
        private List<string> _vlrOut;

        //view attributes
        private float _cloudDensity = 0f;
        private bool _previewCloud = false;


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file path", "path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
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
            if (!DA.GetData(0, ref currentPath)) return;

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


            //initial import
            if (_prevPath != currentPath)
            {
                _asprCld = new AsprCld(currentPath);
                _asprCld.displayDensity = _cloudDensity;

                _headerOut = Utility.FloatDictGhOut(_asprCld.header, this);
                _vlrOut = Utility.StringDictGhOut(_asprCld.vlr);

                _prevPath = currentPath;

                if (_previewCloud) 
                { 
                    GetCloud(DA, overRide: true); 
                } 
                else
                {
                    _asprCld.displayDensity = 2; // setting the cloud density above 1 so that the getCloud method triggers on user button click
                }
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

        public void SetPreview(bool preview)
        {
            _previewCloud = preview;
        }

        public void ZoomCloud()
        {
            if (_previewCloud && _asprCld.ptCloud != null)
            {
                var bBox = _asprCld.ptCloud.GetBoundingBox(true);
                RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.ZoomBoundingBox(bBox);
                RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
            }
        }

        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.BaseAttributes(this, SetVal, SetPreview, ZoomCloud);
        }


        /*
        //drawing the point cloud if preview is enabled
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_asprCld.ptCloud != null && _previewCloud)
            {
                args.Display.DrawPointCloud(_asprCld.ptCloud, 2);
            }

        }

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get
            {
                if (_asprCld != null && _asprCld.ptCloud != null && _previewCloud)
                {
                    return _asprCld.ptCloud.GetBoundingBox(true);
                }

                return base.ClippingBox;

            }
        }

        //need to override this to be previewable despite having no geo output
        public override bool IsPreviewCapable => true;


        //BAKE METHODS
        public override bool IsBakeCapable
        {
            get
            {
                return _asprCld.ptCloud != null && _previewCloud;
            }
        }

        public override void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            if (IsBakeCapable)
            {
                doc.Objects.AddPointCloud(_asprCld.ptCloud);
            }
        }
        */



        //OTHER METHODS ------------------------------------------------------------
        private void GetCloud(IGH_DataAccess da, bool overRide = false)
        {
            // I added the override bool to initialize the pointcloud regardless of preview status when a new file is referenced
            if (_asprCld != null && _asprCld.displayDensity != _cloudDensity && _previewCloud || overRide)
            {
                _asprCld.displayDensity = _cloudDensity;
                _asprCld.GetPointCloud();
                da.SetData(2, new AsprCld(_asprCld));
                RhinoDoc.ActiveDoc.Views.Redraw();
            }
        }
    }
}