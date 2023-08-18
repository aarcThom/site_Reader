﻿using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Grasshopper;
using Grasshopper.GUI.Gradient;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using siteReader.Methods;
using siteReader.Params;

namespace siteReader.Components
{
    public class Intensity : GH_Component
    {
        /// NOTE: SEE https://james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/ for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        Grasshopper.Kernel.Special.GH_GradientControl gradComp;

        /// <summary>
        /// Initializes a new instance of the Intensity class.
        /// </summary>
        public Intensity()
          : base("Intensity", "Nickname",
              "Description",
              "SiteReader", "Point Clouds")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
           pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
           pManager.AddColourParameter("Gradient", "Grad", "The color gradient that will be used to visualize point cloud fields. " +
                "Note: if you edit the the gradient component inputs, or replace the auto generated gradient component you may get slower compute times and strange results." +
                "Absolutely feel free to set your own color scheme though!", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
            pManager.AddIntegerParameter("test", "t", "yo", GH_ParamAccess.list);
        }

        //adding this so we can add a gradient control without any inputs
        protected override void BeforeSolveInstance()
        {
            base.BeforeSolveInstance();

            if (gradComp == null)
            {
                //assign values to the component and doc variables
                Component = this;
                GrasshopperDocument = this.OnPingDocument();

                // create a color gradient component
                gradComp = new Grasshopper.Kernel.Special.GH_GradientControl();
                gradComp.CreateAttributes();

                // add default values to the grad component parameters
                var upperLimit = gradComp.Params.Input[1] as Param_Number;
                upperLimit.PersistentData.ClearData();
                upperLimit.PersistentData.Append(new GH_Number(255));

                var ghRange = new List<GH_Number>();

                for (int i = 0; i < 256; i++)
                {
                    ghRange.Add(new GH_Number(i));
                }

                var steps = gradComp.Params.Input[2] as Param_Number;
                steps.PersistentData.AppendRange(ghRange);


                //get postion for gradient component
                float xPos = this.Attributes.Pivot.X - gradComp.Attributes.Bounds.Width - 160;
                float yPos = this.Attributes.Pivot.Y + gradComp.Attributes.Bounds.Height / 2;

                gradComp.Attributes.Pivot = new System.Drawing.PointF(xPos, yPos);

                //add grad component to document and add to input
                GrasshopperDocument.AddObject(gradComp, false);
                this.Params.Input[1].AddSource(gradComp.Params.Output[0]);
            }
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            gradComp.ComputeData();

            // GET INPUTS ---------------------------------------------------------------------------

            AsprCld cld = new AsprCld();
            if (!DA.GetData(0, ref cld)) return;

            if (cld.ptCloud == null || cld.ptCloud.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no points to color");
                return;
            }

            List<Color> colors = new List<Color>();
            if (!DA.SetDataList(1, colors)) return;
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
            get { return new Guid("D5F14187-98EA-4D39-9CBD-232429CE6B5B"); }
        }
    }
}