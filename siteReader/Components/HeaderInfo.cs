using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Methods;
using siteReader.Params;

namespace siteReader.Components
{
    public class HeaderInfo : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public HeaderInfo()
          : base("LAS Header info", "Header",
            "Return a .las cloud's Header fields as individual parameters",
            "SiteReader", "Point Clouds")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
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
            AsprCld cld = new AsprCld();
            if (!DA.GetData(0, ref cld)) return;

            if (cld.Header == null) 
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no Header. Most likely a rhino referenced point cloud.");
                return;
            }

            int ptCount = (int)cld.Header["Number of Points"];

            Point3d minPt = new Point3d();
            minPt.X = cld.Header["Min X"];
            minPt.Y = cld.Header["Min Y"];
            minPt.Z = cld.Header["Min Z"];

            Point3d maxPt = new Point3d();
            maxPt.X = cld.Header["Max X"];
            maxPt.Y = cld.Header["Max Y"];
            maxPt.Z = cld.Header["Max Z"];

            int ptFrmt = Convert.ToInt32(cld.Header["Point Format"]);


            DA.SetData(0, ptCount);
            DA.SetData(1, minPt);
            DA.SetData(2, maxPt);
            DA.SetData(3, ptFrmt);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
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