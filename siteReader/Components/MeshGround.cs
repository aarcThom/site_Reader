using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Methods;

namespace siteReader.Components
{
    public class MeshGround : SiteReaderBase
    {
        //CONSTRUCTORS ================================================================================================
        public MeshGround()
          : base("Mesh Ground", "Mesh G",
              "Tessellate a point cloud using the XY plane to get a mesh. Works best for ground surfaces.", 
              "Point Clouds")
        {

            //IconPath = "siteReader.Resources...";
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
            pManager.AddNumberParameter("Max. Edge Length", "Max Edge",
                "The maximum allowed mesh edge length. Leave blank if you want no holes or splits in your mesh.",
                GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Ground Mesh", "mesh", "A 2.5D meshing of the supplied point cloud",
                GH_ParamAccess.item);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            if (CldInput == false)
            {
                return;
            }

            Mesh mesh;
            double maxLen = 0;
            if (!DA.GetData(1, ref maxLen))
            {
                mesh = Meshing.TesselatePoints(Cld);
            }
            else
            {
                mesh = Meshing.TesselatePoints(Cld, maxLen);
            }

            DA.SetData(0, mesh);
        }

        //GUID ========================================================================================================
        // make sure to change this if using template
        public override Guid ComponentGuid => new Guid("1D5B1500-AF00-47EA-B01F-87910CD2362C");
    }
}