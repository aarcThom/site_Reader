using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using System.Drawing;
using siteReader.Params;
using System.Linq;
using siteReader.Methods;
using System.Windows.Forms;
using siteReader.UI.features;
using System.IO;
using System.Reflection;
using Rhino.DocObjects;
using Rhino;

namespace siteReader.Components
{
    public class FilterByField : CloudBase
    {
        // NOTE: SEE https://james-ramsden.com/grasshopperdocument-component-grasshopper-visual-studio/
        // for referencing component and grasshopper document in VS
        GH_Document GrasshopperDocument;
        IGH_Component Component;

        //grabbing embedded resources
        private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

        // FIELDS -----------------------------------------------------------------------------------------

        private int _selectedField = -1;
        private List<Color> _colors;

        private int _gradientSelection;

        private List<float> _handleValues = new List<float> { 0f, 1f};

        private AsprCld _previewCloud;

        //the lists for displaying the bar graph
        private List<int> _uniqueFieldVals;
        private List<int> _fieldValCounts;


        /// <summary>
        /// Initializes a new instance of the AssignField class.
        /// </summary>
        /// 
        public FilterByField()
          : base("Filter by Field", "Filter",
              "Filter a point cloud based on its LAS fields", "Point Clouds")
        {
            _gradientSelection = 0;
            _colors = CloudColors.GetColorList(_gradientSelection);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new AsprParam(), "ASPR Cloud", "cld", "A point cloud linked with ASPRS data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            base.SolveInstance(DA);

            
            // clear the UI if cloud input disconnected
            if (_cldInput == false)
            {
                _fieldValCounts = new List<int>();
                _uniqueFieldVals = new List<int>();
                _selectedField = -1;
                Grasshopper.Instances.RedrawCanvas();
                return;
            }
            
            if (_previewCloud != null)
            {
                DA.SetData(0, _previewCloud);
            }
            else
            {
                DA.SetData(0, _cld);
            }
        }

        // PREVIEW OVERRIDES AND UI METHODS --------------------------------------------------------------------

        /// <summary>
        /// Called primarily by the IGH_attributes. Converts selected field values to the selected gradient and applies to pt cloud.
        /// </summary>
        /// <param name="selection"></param>
        public void SelectField(int selection)
        {

            List<Color> newVColors;
            _selectedField = selection;
            var ptCount = _cld.PtCloud.Count;

            switch (selection)
            {
                case 0:
                    newVColors = LasMethods.UShortToColor(_cld.Intensity, _colors, ptCount);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToIntensOrClrChannel(_cld.Intensity);
                    break;

                case 1:
                    newVColors = LasMethods.UShortToColor(_cld.R, _colors, ptCount);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToIntensOrClrChannel(_cld.R);
                    break;

                case 2:
                    newVColors = LasMethods.UShortToColor(_cld.G, _colors, ptCount);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToIntensOrClrChannel(_cld.G);
                    break;

                case 3:
                    newVColors = LasMethods.UShortToColor(_cld.B, _colors, ptCount);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToIntensOrClrChannel(_cld.B);
                    break;

                case 4:
                    newVColors = LasMethods.ByteToColor(_cld.Classification, _colors, ptCount);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToClassOrReturns(_cld.Classification);
                    break;

                case 5:
                    newVColors = LasMethods.ByteToColor(_cld.NumReturns, _colors, ptCount);
                    _cld.ApplyColors(newVColors);
                    _cld.SetFieldToClassOrReturns(_cld.NumReturns);
                    break;
            }

            CountFieldVals();

            //update the preview cloud if necessary
            if (_previewCloud != null)
            {
                FilterFields();
            }
            else
            {
                ExpirePreview(true);
            }
        }

        public int SendSelectedField()
        {
            return _selectedField;
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

        /// <summary>
        /// This region overrides the typical component layout
        /// </summary>
        public override void CreateAttributes()
        {
            m_attributes = new SiteReader.UI.DisplayFields(this, SelectField, SendSelectedField,
                SliderValues, FilterFields, SendColors, SendValCounts, SendValues);
        }

        /// <summary>
        /// Draws the input cloud if not filtered or filtered cloud if filtered
        /// </summary>
        /// <param name="args"></param>
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (_cld != null && _cld.PtCloud != null)
            {

                if (_cld.CurrentField != null && _previewCloud != null && _previewCloud.PtCloud != null)
                {
                    args.Display.DrawPointCloud(_previewCloud.PtCloud, 2);
                }

                else
                {
                    args.Display.DrawPointCloud(_cld.PtCloud, 2);
                }
            }
        }

        // DROPDOWN MENU --------------------------------------------------------------------------------------

        /// <summary>
        /// Appends the gradient selector to the dropdown menu
        /// </summary>
        /// <param name="menu"></param>
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            var gradients = CloudColors.GradNames;


            for (int i = 0; i < gradients.Count; i++)
            {
                var gradName = gradients[i];
                Image img = null;

                if (i == _gradientSelection)
                {
                    Stream stream = _assembly.GetManifestResourceStream(
                    "siteReader.Resources.menus." + gradName + "_yes.png");
                    if (stream != null) img = Image.FromStream(stream);
                }
                else
                {
                    Stream stream = _assembly.GetManifestResourceStream(
                        "siteReader.Resources.menus." + gradName + "_no.png");
                    if (stream != null) img = Image.FromStream(stream);
                }

                if (img != null)
                {
                    GH_Component.Menu_AppendItem(menu, gradName, Menu_GradientSelect, img);
                }
                else
                {
                    GH_Component.Menu_AppendItem(menu, gradName, Menu_GradientSelect);
                }
                
            }
        }

        /// <summary>
        /// Gradient selection event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Menu_GradientSelect(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && CloudColors.GradNames.Contains(item.Text))
            {
                _gradientSelection = CloudColors.GradNames.IndexOf(item.Text);
                _colors = CloudColors.GetColorList(_gradientSelection);
                if (_cld != null) SelectField(_selectedField);
                Grasshopper.Instances.RedrawCanvas();
                

                if (_previewCloud != null)
                {
                    FilterFields();
                }
                else
                {
                    ExpirePreview(true);
                }
            }
        }

        // OVERRIDING BAKING - need to do this to bake the preview cloud ------------------------------------
        public override void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, new ObjectAttributes(), obj_ids);
        }

        public override void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            if (_previewCloud != null && _previewCloud.PtCloud != null)
            {
                obj_ids.Add(doc.Objects.AddPointCloud(_previewCloud.PtCloud, att));
            }
            else
            {
                obj_ids.Add(doc.Objects.AddPointCloud(_cld.PtCloud, att));
            }

        }

        public override bool IsBakeCapable => _previewCloud.PtCloud != null || _cld.PtCloud != null;

        //UTILITY METHODS-------------------------------------------------------------------------------------------------

        /// <summary>
        /// Filters the point cloud based on handle position's relation to selected field.
        /// </summary>
        public void FilterFields()
        {
            if (_cld.CurrentField == null) return;

            var cldPts = _cld.PtCloud.GetPoints();

            bool[] filterArr = new bool[cldPts.Length];

            for (int i = 0; i < cldPts.Length; i++)
            {
                var inBounds = _cld.CurrentField[i] >= _handleValues[0] && _cld.CurrentField[i] <= _handleValues[1];
                filterArr[i] = inBounds;
            }

            _previewCloud = new AsprCld(_cld, filterArr);
            ExpirePreview(true);
        }

        /// <summary>
        /// Sets the selected field's stats
        /// </summary>
        private void CountFieldVals()
        {
            var formattedVals = _cld.CurrentField.Select(val => (int)(val * 256)).ToList();
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
        protected override Bitmap Icon
        {
            get
            {
                var stream = _assembly.GetManifestResourceStream("siteReader.Resources.menus.heatmap_yes.png");
                return new Bitmap(stream);
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// MAKE SURE TO CHANGE THIS IF USING THE TEMPLATE!
        /// </summary>
        public override Guid ComponentGuid => new Guid("31D0F86A-21AA-4AB1-A071-EB77551C4B70");
    }
}