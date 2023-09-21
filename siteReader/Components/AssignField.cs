using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Drawing;
using siteReader.Params;
using System.Linq;
using siteReader.Methods;

namespace siteReader.Components
{
    public class AssignField : CloudBase
    {
        /// NOTE: SEE https://james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/ for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;
        Grasshopper.Kernel.Special.GH_GradientControl gradComp;

        private int _selectedField = -1;
        private List<Color> _colors;
        private List<Color> _prevColors;

        private List<float> _handleValues = new List<float> { 0f, 1f};

        private PointCloud _previewCloud;



        /// <summary>
        /// Initializes a new instance of the AssignField class.
        /// </summary>
        /// 

        public AssignField()
          : base("Assign Field", "Field",
              "Assign the values contained in a LAS field to the point cloud", "Point Clouds")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            base.RegisterInputParams(pManager);
            pManager.AddColourParameter("Gradient", "Grad", "The color gradient that will be used to visualize point cloud fields. " +
            "Note: if you edit the the gradient component inputs, or replace the auto generated gradient component you may get slower compute times and strange results." +
            "Absolutely feel free to set your own color scheme though!", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
            pManager.AddNumberParameter("val", "val", "val", GH_ParamAccess.list);
        }

        /*
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
        */

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            List<Color> colors = new List<Color>();
            if (!DA.GetDataList(1, colors))
            {
                return;
            }
            else
            {
                _colors = colors;
            }

            // checking if the colors have been updated
            if (_prevColors != null && _selectedField != -1 && !_colors.SequenceEqual(_prevColors))
            {
                SelectField(_selectedField);
            }

            DA.SetDataList(1, _handleValues);
        }

        //PREVIEW OVERRIDES AND UI METHODS ---------------------------------------------------

        //methods for passing values from UI controller
        public void SelectField(int selection)
        {

            List<Color> newVColors;
            _selectedField = selection;

            switch (selection)
            {
                case 0:
                    newVColors = LasMethods.uShortToColor(_cld.intensity, _colors);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToIntensity();
                    break;

                case 1:
                    newVColors = _cld.rgb;
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToRGB();
                    break;

                case 2:
                    newVColors = LasMethods.byteToColor(_cld.classification, _colors);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToClassOrReturns(_cld.classification);
                    break;

                case 3:
                    newVColors = LasMethods.byteToColor(_cld.numReturns, _colors);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToClassOrReturns(_cld.numReturns);
                    break;
            }

            _prevColors = _colors;
            ExpirePreview(true);

        }

        public void SliderValues(List<float> handlePositions)
        {
            _handleValues = handlePositions;
        }

        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.DisplayFields(this, SelectField, SliderValues, FilterFields);
        }

        //need to override this to display the value cropped cloud
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cld != null && _cld.ptCloud != null)
            {

                if (_cld.currentField != null && _previewCloud != null)
                {
                    args.Display.DrawPointCloud(_previewCloud, 2);
                }

                else
                {
                    args.Display.DrawPointCloud(_cld.ptCloud, 2);
                }
            }
        }

        //Other methods
        public void FilterFields()
        {
            if (_cld.currentField == null) return;

            _previewCloud = new PointCloud();
            var cldPts = _cld.ptCloud.GetPoints();
            var ptColors = _cld.ptCloud.GetColors();

            for (int i = 0; i < cldPts.Length; i++)
            {
                if (_cld.currentField[i] >= _handleValues[0] && _cld.currentField[i] <= _handleValues[1])
                    _previewCloud.Add(cldPts[i], ptColors[i]);
            }
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
            get { return new Guid("31D0F86A-21AA-4AB1-A071-EB77551C4B70"); }
        }
    }
}