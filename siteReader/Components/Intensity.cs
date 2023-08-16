using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
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
                "Note: if you edit the inputs of the gradient component inputs, you may get slower compute times and strange results.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
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
                var lowerLimit = gradComp.Params.Input[0] as Param_Number;
                lowerLimit.AddVolatileData(new Grasshopper.Kernel.Data.GH_Path(0), 0, new GH_Number(0));

                var upperLimit = gradComp.Params.Input[1] as Param_Number;
                upperLimit.AddVolatileData(new Grasshopper.Kernel.Data.GH_Path(0), 0, new GH_Number(255));

                var valRange = Enumerable.Range(0, 255);

                var steps = gradComp.Params.Input[2] as Param_Number;
                steps.AddVolatileDataList(new Grasshopper.Kernel.Data.GH_Path(0), valRange);


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

            // GET INPUTS ---------------------------------------------------------------------------

            AsprCld cld = new AsprCld();
            if (!DA.GetData(0, ref cld)) return;

            if (cld.ptCloud == null || cld.ptCloud.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This cloud has no points to color");
                return;
            }
            




            //AsprCld colorCloud = new AsprCld(LasMethods.ColorByField(cld), cld);

            //DA.SetData(0, colorCloud);


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