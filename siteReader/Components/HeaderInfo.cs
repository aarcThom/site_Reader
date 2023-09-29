using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Methods;
using siteReader.Params;
using System.Drawing;


namespace siteReader.Components
{
    public class HeaderInfo : CloudBase
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public HeaderInfo()
          : base("LAS Header info", "Header",
            "Return a .las cloud's Header fields as individual parameters",
            "Point Clouds")
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Point Count", "PtCnt", "The number of points in the .las file", GH_ParamAccess.item);
            pManager.AddPointParameter("Minimum XYZ Point", "MinPt", "The minimum XYZ values in the cloud as a point3d", GH_ParamAccess.item);
            pManager.AddPointParameter("Maximum XYZ Point", "MaxPt", "The maximum XYZ values in the cloud as a point3d", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Point Format", "PtFrmt", "The .las 1.4 point format. See standards for included fields", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (CldInput == false)
            {
                //CLEAR UI DATA HERE
                //Grasshopper.Instances.RedrawCanvas();
                return;
            }

            if (Cld.Header == null) 
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no Header. Most likely a rhino referenced point cloud.");
                return;
            }

            int ptCount = (int)Cld.Header["Number of Points"];

            Point3d minPt = new Point3d();
            minPt.X = Cld.Header["Min X"];
            minPt.Y = Cld.Header["Min Y"];
            minPt.Z = Cld.Header["Min Z"];

            Point3d maxPt = new Point3d();
            maxPt.X = Cld.Header["Max X"];
            maxPt.Y = Cld.Header["Max Y"];
            maxPt.Z = Cld.Header["Max Z"];

            int ptFrmt = Convert.ToInt32(Cld.Header["Point Format"]);


            DA.SetData(0, ptCount);
            DA.SetData(1, minPt);
            DA.SetData(2, maxPt);
            DA.SetData(3, ptFrmt);
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("2244C6D1-85AC-4E0C-A4CB-55549DB009BD"); }
        }
    }
}