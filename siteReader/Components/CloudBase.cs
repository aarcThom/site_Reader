﻿using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Params;

namespace siteReader.Components
{
    /* Getting around the GUID:
     * https://discourse.mcneel.com/t/c-grasshopper-component-inheritance/60039/2
     * To make a GH_Component base class, declare it as abstract to force inheritors 
     * to overwrite methods that it does not overwrite, such as Guid, It’s just good practice I think.
     */

    public abstract class CloudBase : GH_Component
    {
        //FIELDS
        protected AsprCld _cld;
        protected AsprCld _prevCld; // used to check if the cloud input has changed


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
            AsprCld cld = new AsprCld();
            if (!DA.GetData(0, ref cld))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter ASPR Cloud failed to collect data");
                _cld = null;
                _prevCld = null;
                return;
            }
            else if (cld.ptCloud == null || cld.ptCloud.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no points");
                return;
            }
            else
            {
                if (_cld == null || _prevCld != _cld)
                {
                    _cld = new AsprCld(cld);
                    _prevCld = _cld;
                }

            }
        }

        
        //drawing the point cloud if preview is enabled
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cld != null && _cld.ptCloud != null)
            {
                args.Display.DrawPointCloud(_cld.ptCloud, 2);
            }
        }
        

        //Return a BoundingBox that contains all the geometry you are about to draw.
        public override BoundingBox ClippingBox
        {
            get
            {
                if (_cld != null && _cld.ptCloud != null)
                {
                    return _cld.ptCloud.GetBoundingBox(true);
                }

                return base.ClippingBox;

            }
        }

        //need to override this to be previewable despite having no geo output with preview method
        public override bool IsPreviewCapable => true;

    }
}