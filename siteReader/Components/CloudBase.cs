using System.Reflection;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Params;
using System.Drawing;
using siteReader.Methods;
using System.Diagnostics;

namespace siteReader.Components
{
    /* Getting around the GUID:
     * https://discourse.mcneel.com/t/c-grasshopper-component-inheritance/60039/2
     * To make a GH_Component base class, declare it as abstract to force inheritors 
     * to overwrite methods that it does not overwrite, such as Guid, It’s just good practice I think.
     */

    public abstract class CloudBase : SiteReaderBase
    {
        //CONSTRUCTORS ================================================================================================
        protected CloudBase(string name, string nickname, string description, string subCategory)
          : base(name, nickname, description, subCategory)
        {
        }

        //PREVIEW AND UI ==============================================================================================
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if ((Cld != null && Cld.PtCloud != null) && (ImportCld == true || !ImportCld.HasValue) && !Locked)
            {
                args.Display.DrawPointCloud(Cld.PtCloud, 2);
            }
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                if ((Cld != null && Cld.PtCloud != null) && (ImportCld == true || !ImportCld.HasValue))
                {
                    return Cld.PtCloud.GetBoundingBox(true);
                }

                return base.ClippingBox;

            }
        }

        //need to override this to be previewable despite having no geo output with preview method
        public override bool IsPreviewCapable => true;

        /// <summary>
        /// Zoom in on the cloud in all viewports
        /// </summary>
        public void ZoomCloud()
        {
            if (ImportCld == true && Cld.PtCloud != null)
            {
                var bBox = Cld.PtCloud.GetBoundingBox(true);
                Utility.ZoomGeo(bBox);
            }
        }
    }
}