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

            IconPath = "siteReader.Resources.filterField.png";
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
            if (CldInput == false)
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
                DA.SetData(0, Cld);
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
            var ptCount = Cld.PtCloud.Count;

            switch (selection)
            {
                case 0:
                    newVColors = LasMethods.UShortToColor(Cld.Intensity, _colors, ptCount);
                    Cld.ApplyColors(newVColors);
                    Cld.SetFieldToIntensOrClrChannel(Cld.Intensity);
                    break;

                case 1:
                    newVColors = LasMethods.UShortToColor(Cld.R, _colors, ptCount);
                    Cld.ApplyColors(newVColors);
                    Cld.SetFieldToIntensOrClrChannel(Cld.R);
                    break;

                case 2:
                    newVColors = LasMethods.UShortToColor(Cld.G, _colors, ptCount);
                    Cld.ApplyColors(newVColors);
                    Cld.SetFieldToIntensOrClrChannel(Cld.G);
                    break;

                case 3:
                    newVColors = LasMethods.UShortToColor(Cld.B, _colors, ptCount);
                    Cld.ApplyColors(newVColors);
                    Cld.SetFieldToIntensOrClrChannel(Cld.B);
                    break;

                case 4:
                    newVColors = LasMethods.ByteToColor(Cld.Classification, _colors, ptCount);
                    Cld.ApplyColors(newVColors);
                    Cld.SetFieldToClassOrReturns(Cld.Classification);
                    break;

                case 5:
                    newVColors = LasMethods.ByteToColor(Cld.NumReturns, _colors, ptCount);
                    Cld.ApplyColors(newVColors);
                    Cld.SetFieldToClassOrReturns(Cld.NumReturns);
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
            if (Cld != null && Cld.PtCloud != null)
            {

                if (Cld.CurrentField != null && _previewCloud != null && _previewCloud.PtCloud != null)
                {
                    args.Display.DrawPointCloud(_previewCloud.PtCloud, 2);
                }

                else
                {
                    args.Display.DrawPointCloud(Cld.PtCloud, 2);
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
                    Stream stream = GHAssembly.GetManifestResourceStream(
                    "siteReader.Resources.menus." + gradName + "_yes.png");
                    if (stream != null) img = Image.FromStream(stream);
                }
                else
                {
                    Stream stream = GHAssembly.GetManifestResourceStream(
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
                if (Cld != null) SelectField(_selectedField);
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
                obj_ids.Add(doc.Objects.AddPointCloud(Cld.PtCloud, att));
            }

        }

        public override bool IsBakeCapable => _previewCloud.PtCloud != null || Cld.PtCloud != null;

        //UTILITY METHODS-------------------------------------------------------------------------------------------------

        /// <summary>
        /// Filters the point cloud based on handle position's relation to selected field.
        /// </summary>
        public void FilterFields()
        {
            if (Cld.CurrentField == null) return;

            var cldPts = Cld.PtCloud.GetPoints();

            bool[] filterArr = new bool[cldPts.Length];

            for (int i = 0; i < cldPts.Length; i++)
            {
                var inBounds = Cld.CurrentField[i] >= _handleValues[0] && Cld.CurrentField[i] <= _handleValues[1];
                filterArr[i] = inBounds;
            }

            _previewCloud = new AsprCld(Cld, filterArr);
            ExpirePreview(true);
        }

        /// <summary>
        /// Sets the selected field's stats
        /// </summary>
        private void CountFieldVals()
        {
            var formattedVals = Cld.CurrentField.Select(val => (int)(val * 256)).ToList();
            formattedVals.Sort();
            _uniqueFieldVals = new HashSet<int>(formattedVals).ToList();

            _fieldValCounts = new List<int>();

            foreach(var val in _uniqueFieldVals)
            {
                _fieldValCounts.Add(formattedVals.Count(x => x == val));
            }

        }



        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// MAKE SURE TO CHANGE THIS IF USING THE TEMPLATE!
        /// </summary>
        public override Guid ComponentGuid => new Guid("31D0F86A-21AA-4AB1-A071-EB77551C4B70");
    }
}