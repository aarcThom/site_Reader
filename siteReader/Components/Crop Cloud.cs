using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Methods;
using siteReader.Params;
using g3;
using Rhino.Render.ChangeQueue;
using Mesh = Rhino.Geometry.Mesh;

namespace siteReader.Components
{
    public class CropCloud : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CropCloud class.
        /// </summary>
        public CropCloud()
          : base("Crop Cloud", "CropCld",
              "Remove any points from a point cloud that are not in (or outside) a given brep",
              "SiteReader", "Point Clouds")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
            pManager.AddMeshParameter("Crop Shape(s)", "Brep or Mesh", "The shape(s) to crop the point cloud", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Inside", "In", "If set to true, points inside the cloud will be kept. If set to false, points outside the cloud will be kept.", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // GET INPUTS ---------------------------------------------------------------------------
            AsprCld cld = new AsprCld();
            if (!DA.GetData(0, ref cld)) return;

            if (cld.ptCloud == null || cld.ptCloud.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no points to crop!");
                return;
            }

            var cropMeshes = new List<Mesh>();
            if (!DA.GetDataList(1, cropMeshes)) return;

            bool inside = true;
            if (!DA.GetData(2, ref inside)) return;

            //THE WORK -----------------------------------------------------------------------------

            var ptCloud = new PointCloud();

            List<int> newIndices = new List<int>();
            
            var cropMesh = new Mesh();
            foreach (Mesh mesh in cropMeshes) 
            { 
                cropMesh.Append(mesh);
            }

            DMesh3 dMesh = Utility.MeshtoDMesh(cropMesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(dMesh);
            spatial.Build();

            var dir = new g3.Vector3d(0, 0, 1);

            int count = 0;

            foreach (var item in cld.ptCloud)
            {
                var rPt = item.Location;
                g3.Vector3d pt = new g3.Vector3d(rPt.X, rPt.Y, rPt.Z);
                g3.Ray3d ray = new g3.Ray3d(pt, dir);

                int hitCnt = spatial.FindAllHitTriangles(ray);

                if (hitCnt % 2 != 0 && inside)
                {
                    ptCloud.Add(rPt, item.Color);
                    newIndices.Add(cld.ptIndices[count]);
                } 
                else if (hitCnt % 2 == 0 && !inside)
                {
                    ptCloud.Add(rPt, item.Color);
                    newIndices.Add(cld.ptIndices[count]);
                }

                count++;

            }

            AsprCld cropCloud = new AsprCld(ptCloud, cld);


            DA.SetData(0, cropCloud);

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
            get { return new Guid("07A43544-2CDA-48EF-890C-0BB29FE40889"); }
        }
    }
}