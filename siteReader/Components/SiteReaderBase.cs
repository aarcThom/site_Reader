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

    public abstract class SiteReaderBase : GH_Component
    {
        //FIELDS ======================================================================================================

        // NOTE: See james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/
        // for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        //grabbing embedded resources
        protected readonly Assembly GHAssembly = Assembly.GetExecutingAssembly();

        protected string IconPath;

        //CONSTRUCTORS ================================================================================================

        // See the below link for a good example of an abstract base class for custom component inheritance:
        // github.com/mcneel/rhino-developer-samples/blob/5/grasshopper/cs/SamplePlatonics/GrasshopperPlatonics
        protected SiteReaderBase(string name, string nickname, string description, string subCategory)
          : base(name, nickname, description, "SiteReader", subCategory)
        {
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