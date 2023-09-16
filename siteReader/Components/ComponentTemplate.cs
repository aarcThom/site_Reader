﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace siteReader.Components
{
    public class ComponentTemplate : CloudBase
    {
        /// <summary>
        /// Initializes a new instance of the ComponentTemplate class.
        /// </summary>
        /// 

        public ComponentTemplate()
          : base("ComponentName", "Nickname",
              "Description", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
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
        /// MAKE SURE TO CHANGE THIS IF USING THE TEMPLATE!
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("36D4BC06-59B6-49D6-9F2B-8003F7050F10"); }
        }
    }
}