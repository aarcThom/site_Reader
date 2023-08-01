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
        
        private bool _prevTransBool = false;

        private Vector3d _translationVector;
        private Vector3d _prevTransVector = Vector3d.Zero;

        private FullPointCloud fullPtCloud;
        private List<string> _headerOut;
        private List<string> _vlrOut;
        

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file path", "path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("move to origin?", "translate",
                "translate the point cloud so that its minimum X,Y,Z values land at the origin", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager.AddVectorParameter("Translation Vector", "Translation", 
                "Optional translation vector used to keep your data aligned", GH_ParamAccess.item, Vector3d.Zero);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Header", "header", "Header information.", GH_ParamAccess.list);
            pManager.AddTextParameter("VLR", "VLR", "Variable length records - if present in file.",
                GH_ParamAccess.list);
            pManager.AddVectorParameter("Translation Vector", "Translation", "The vector used to translate the pt cloud to origin", GH_ParamAccess.item);
            
            
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
            bool translateBool = false;

            DA.GetData(1, ref translateBool);
            DA.GetData(2, ref _translationVector);

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


            //import the cloud
            if (_prevPath != currentPath)
            {

                var impLas = new LASzip.Net.laszip();

                var header = LasMethods.HeaderDict(impLas, currentPath);
                _headerOut = Utility.FloatDictGhOut(header, this);
                

                var vlrDict = LasMethods.VlrDict(impLas, currentPath);
                _vlrOut = Utility.StringDictGhOut(stringDict: vlrDict);
                

                fullPtCloud = new FullPointCloud(currentPath, vlrDict, header);

                _prevPath = currentPath;
            }

            //translate the cloud
            if(_prevTransBool != translateBool || _translationVector != _prevTransVector || _prevPath != currentPath)
            {
                if (fullPtCloud != null)
                {
                    _translationVector = LasMethods.TranslateCloud(translateBool, fullPtCloud, this, _translationVector);
                }
                _prevTransBool = translateBool;
                _prevTransVector = _translationVector;
            }


            DA.SetDataList(0, _headerOut);
            DA.SetDataList(1, _vlrOut);
            DA.SetData(2, _translationVector);







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


    }
}