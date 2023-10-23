using System.Reflection;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Params;
using System.Drawing;
using siteReader.Methods;

namespace siteReader.Components.Clouds
{
    /* Getting around the GUID:
     * https://discourse.mcneel.com/t/c-grasshopper-component-inheritance/60039/2
     * To make a GH_Component base class, declare it as abstract to force inheritors 
     * to overwrite methods that it does not overwrite, such as Guid, It’s just good practice I think.
     */

    public abstract class CloudBase : GH_Component
    {
        //FIELDS ======================================================================================================

        // NOTE: See james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/
        // for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        //grabbing embedded resources
        protected readonly Assembly GHAssembly = Assembly.GetExecutingAssembly();

        protected AsprCld Cld;
        protected bool CldInput; //used to check if their is input in the inheriting components
        protected bool? ImportCld; //used if a component has an import cld button. bool? = nullable bool.

        protected string IconPath;

        //CONSTRUCTORS ================================================================================================

        // See the below link for a good example of an abstract base class for custom component inheritance:
        // github.com/mcneel/rhino-developer-samples/blob/5/grasshopper/cs/SamplePlatonics/GrasshopperPlatonics
        protected CloudBase(string name, string nickname, string description, string subCategory)
          : base(name, nickname, description, "SiteReader", subCategory)
        {
        }


        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data",
                GH_ParamAccess.item);

            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Retrieve the input data from the Aspr Cloud input
            //NOTE: The inheriting component needs to return if CldInput == false
            AsprCld cld = new AsprCld();
            if (!DA.GetData(0, ref cld))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter ASPR Cloud failed to collect data");
                Cld = null;
                CldInput = false;
            }
            else if (cld.PtCloud == null || cld.PtCloud.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no points");
                CldInput = false;
            }
            else
            {
                if (Cld == null || cld != Cld)
                {
                    Cld = new AsprCld(cld);
                    CldInput = true;
                }

            }
        }

        /// <summary>
        /// Provides an Icon for the component. Defaults to generic icon if none provided.
        /// </summary>
        protected override Bitmap Icon
        {
            get
            {
                if (IconPath == null)
                {
                    IconPath = "siteReader.Resources.generic.png";
                }

                var stream = GHAssembly.GetManifestResourceStream(IconPath);
                return new Bitmap(stream);
            }
        }
    }
}