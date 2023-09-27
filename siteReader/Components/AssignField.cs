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
using System.Windows.Forms;
using siteReader.UI.features;
using Rhino.UI.Interfaces;
using System.IO;
using System.Runtime.InteropServices;
using System.Reflection;
using Grasshopper.GUI.Canvas;

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

        private int _gradientSelection;

        private List<float> _handleValues = new List<float> { 0f, 1f};

        private PointCloud _previewCloud;

        //the lists for displaying the bar graph
        private List<int> _uniqueFieldVals;
        private List<int> _fieldValCounts;

        //FOR GRABBING EMBEDDED RESOURCES
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();



        /// <summary>
        /// Initializes a new instance of the AssignField class.
        /// </summary>
        /// 

        public AssignField()
          : base("Assign Field", "Field",
              "Assign the values contained in a LAS field to the point cloud", "Point Clouds")
        {
            _gradientSelection = 0;
            _colors = CloudColors.GetColorList(_gradientSelection);
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
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
            pManager.AddNumberParameter("val", "val", "val", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);
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

            CountFieldVals();
            ExpirePreview(true);

        }

        public void SliderValues(List<float> handlePositions)
        {
            _handleValues = handlePositions;
        }

        public List<Color> SendColors()
        {
            return _colors;
        }

        public List<int> SendValCounts()
        {
            return _fieldValCounts;
        }

        public List<int> SendValues()
        {
            return _uniqueFieldVals;
        }

        //This region overrides the typical component layout
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.DisplayFields(this, SelectField, SliderValues, FilterFields, SendColors, SendValCounts, SendValues);
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

        // adding the gradient selector to the right click menu
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            var gradients = CloudColors.GradNames;


            for (int i = 0; i < gradients.Count; i++)
            {
                var gradName = gradients[i];
                Image img;

                if (i == _gradientSelection)
                {
                    Stream stream = _assembly.GetManifestResourceStream(
                    "siteReader.Resources.menus.selected.png");
                    img = Image.FromStream(stream);
                }
                else
                {
                    Stream stream = _assembly.GetManifestResourceStream(
                    "siteReader.Resources.menus.deselected.png");
                    img = Image.FromStream(stream);
                }
                GH_Component.Menu_AppendItem(menu, gradName, Menu_GradientSelect, img);
            }
        }

        //the gradient selection event handler
        public void Menu_GradientSelect(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && CloudColors.GradNames.Contains(item.Text))
            {
                _gradientSelection = CloudColors.GradNames.IndexOf(item.Text);
                _colors = CloudColors.GetColorList(_gradientSelection);
                SelectField(_selectedField);
                Attributes.ExpireLayout();
                ExpirePreview(true);
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

        private void CountFieldVals()
        {
            var formattedVals = _cld.currentField.Select(val => (int)(val * 256)).ToList();
            formattedVals.Sort();
            _uniqueFieldVals = new HashSet<int>(formattedVals).ToList();

            _fieldValCounts = new List<int>();

            foreach(var val in _uniqueFieldVals)
            {
                _fieldValCounts.Add(formattedVals.Count(x => x == val));
            }

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        /// //You can add image files to your project resources and access them like this:
        // return Resources.IconForThisComponent;
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// MAKE SURE TO CHANGE THIS IF USING THE TEMPLATE!
        /// </summary>
        public override Guid ComponentGuid => new Guid("31D0F86A-21AA-4AB1-A071-EB77551C4B70");
    }
}