﻿using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Reflection;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Params;
using System.Drawing;
using System.Windows.Forms;

namespace siteReader.Components
{
    /* Getting around the GUID:
     * https://discourse.mcneel.com/t/c-grasshopper-component-inheritance/60039/2
     * To make a GH_Component base class, declare it as abstract to force inheritors 
     * to overwrite methods that it does not overwrite, such as Guid, It’s just good practice I think.
     */

    public abstract class CloudBase : GH_Component
    {
        // NOTE: SEE https://james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/
        // for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        //grabbing embedded resources
        protected readonly Assembly GHAssembly = Assembly.GetExecutingAssembly();

        //FIELDS
        protected AsprCld Cld;
        protected bool CldInput; //used to check if their is input in the inheriting components

        protected string IconPath;


        /// <summary>
        /// Initializes a new instance of the CloudBase class.
        /// See the below link for a good example of an abstract base class for custom component inheritance:
        /// https://github.com/mcneel/rhino-developer-samples/blob/5/grasshopper/cs/SamplePlatonics/GrasshopperPlatonics/PlatonicComponentBase.cs
        /// </summary>


        protected CloudBase(string name, string nickname, string description, string subCategory)
          : base(name, nickname, description, "SiteReader", subCategory)
        {
        }


        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Retrive the input data from the Aspr Cloud input
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

        
        //drawing the point cloud if preview is enabled
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Cld != null && Cld.PtCloud != null)
            {
                args.Display.DrawPointCloud(Cld.PtCloud, 2);
            }
        }
        

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get
            {
                if (Cld != null && Cld.PtCloud != null)
                {
                    return Cld.PtCloud.GetBoundingBox(true);
                }

                return base.ClippingBox;

            }
        }

        /// <summary>
        /// Provides an Icon for the component.
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

        //need to override this to be previewable despite having no geo output with preview method
        public override bool IsPreviewCapable => true;

    }
}