using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Eto.Forms;
using Rhino.UI;
using MouseButtons = System.Windows.Forms.MouseButtons;
using siteReader.Methods;
using siteReader.UI.features;
using Grasshopper;

namespace SiteReader.UI
{
    public class DisplayFields : GH_ComponentAttributes
    {

        //FIELDS -----------------------------------------------------------------------------------------------

        //return values
        private readonly Action<int> _selectField;
        private readonly Action<List<float>> _handleValues;
        private readonly Action _filterFields;

        //get values from owner
        private readonly Func<List<Color>> _fColors;
        private readonly Func<List<int>> _fValCounts;
        private readonly Func<List<int>> _fValues;
 

        private int _chosenField = -1; // what field the user picks

        //rectangles and pts for layouts
        private Point _ptLeft;
        private Point _ptRight;

        //the radio rectangles for buttons
        private RadioButtons _radRecs;

        //the rectangle for the gradient slider
        private RectangleF _gradientBox;
        //the rectangle for the values and sliders themselves
        private RectangleF _sliderValBox;

        //slider handles
        private readonly int _numHandles = 2;
        private readonly int _handleDiameter = 8;
        private HorizSliders _slider;
        private RectangleF[] _handleRecs;
        private int _currentHandle;
        private float _currentOffset;
        private bool _sliding = false;

        private string _fieldLegendTxt = "LIDAR field to display";

        private RectangleF _filterButton;

        // the selections
        private string[] _fields = new string[6] { "Intensity", "Red Channel", "Green Channel", "Blue Channel", "Classification", "Number of Returns" };


        //CONSTRUCTOR -------------------------------------------------------------------------------------------
        // note to self: in C# use Action when you return void, and Func when you return value(s)
        /* documentation on base:
         https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base
         */

        public DisplayFields(GH_Component owner, Action<int> selectField, Action<List<float>> handleValues, 
            Action filterFields, Func<List<Color>> fColors, Func<List<int>> fValCounts, Func<List<int>> fValues) : base(owner)
        {
            _selectField = selectField;
            _slider = new HorizSliders(_numHandles, _handleDiameter);
            _handleValues = handleValues;
            _filterFields = filterFields;
            _fColors = fColors;
            _fValCounts = fValCounts;
            _fValues = fValues;
        }


        protected override void Layout()
        {
            base.Layout(); //handles the basic layout, computes the bounds, etc.
            Rectangle componentRec = GH_Convert.ToRectangle(Bounds); //getting component base bounds

            //saving the original bounds to refer to in custom layout
            var left = componentRec.Left;
            var right = componentRec.Right;
            var bottom = componentRec.Bottom;
            var width = componentRec.Width;

            //useful layout variables like spacers, etc.
            /* docs on constants
             https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/constants
             */

            const int vertSpace = 10;
            const int sideSpacer = 4;
            const int extraHeight = 230;
            const int horizSpace = 8;

            //here we can modify the bounds
            componentRec.Height += extraHeight;

            //here we can assign the modified bounds to the component's bounds--------------------
            Bounds = componentRec;

            //here we can add extra stuff to the layout-------------------------------------------

            //points for divider lines
            _ptLeft = new Point(left + sideSpacer / 2, bottom + vertSpace / 2);
            _ptRight = new Point(right - sideSpacer / 2, bottom + vertSpace / 2);

            //the radio buttons
            _radRecs = new RadioButtons(componentRec, _fields, _fieldLegendTxt, horizSpace, vertSpace,
                sideSpacer, _ptLeft.Y + vertSpace);


            //the gradient graph rectangle
            var gradRectTop = _radRecs.Buttons.Last().Bottom + vertSpace;
            _gradientBox = new RectangleF(left, gradRectTop, width, 50);
            _gradientBox.Inflate(-sideSpacer * 2, 0);

            _sliderValBox = _gradientBox;
            _sliderValBox.Inflate(_handleDiameter / -2f , _handleDiameter / -2f);

            //the sliders 
            var sliderTop = gradRectTop + horizSpace;
            var sliderLeft = _sliderValBox.Left;
            var sliderRight = _sliderValBox.Right;
            var sliderWidth = sliderRight - sliderLeft;

            if (_sliding)
            {
                _slider.MoveSlider(_currentHandle, _currentOffset);
            }

            _handleRecs = _slider.LayoutSlider(sliderLeft, sliderTop, sliderWidth);

            //the filter button
            var filterButTop = _gradientBox.Bottom + vertSpace;
            _filterButton = new RectangleF(left, filterButTop, width, 20);
            _filterButton.Inflate(-sideSpacer * 2, 0);
        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel); // handle the wires, draw nickname, name, etc.

            //the main component rendering channel
            if (channel == GH_CanvasChannel.Objects)
            {
                //declare the pens / brushes / pallets we will need to draw the custom objects - defaults for blank / message levels
                Pen outLine = CompStyles.BlankOutline;

                //use a switch statement to retrieve the proper pens / brushes from our CompColors class
                switch (Owner.RuntimeMessageLevel)
                {
                    case GH_RuntimeMessageLevel.Warning:
                        // assign warning values
                        outLine = CompStyles.WarnOutline;
                        break;

                    case GH_RuntimeMessageLevel.Error:
                        // assign warning values
                        outLine = CompStyles.ErrorOutline;
                        break;
                }


                //render custom elements----------------------------------------------------------

                //font for legends
                Font font = GH_FontServer.Small;
                // adjust fontsize to high resolution displays
                font = new Font(font.FontFamily, font.Size / GH_GraphicsUtil.UiScale, FontStyle.Regular);

                //spacer line
                graphics.DrawLine(outLine, _ptLeft, _ptRight);

                //radio buttons
                _radRecs.Draw(outLine, font, font, graphics, _chosenField);

                //drawing the gradient graph outline and background
                var gradRect = new RectangleF[1] { _gradientBox };
                graphics.FillRectangles(CompStyles.GraphBackground, gradRect);
                graphics.DrawRectangles(outLine, gradRect);

                //draw the slider bars for the graph
                foreach (var cntr in _slider.HandCenters)
                {
                    var ptBase = new PointF(cntr.X, _sliderValBox.Bottom);
                    Pen handleLine = new Pen(Color.AliceBlue, 0.25f);
                    graphics.DrawLine(handleLine, cntr, ptBase);
                }

                //drawing the gradient graphs
                if (_chosenField >= 0)
                {
                    var vals = _fValues();
                    var valCnts = _fValCounts();
                    var colors = _fColors();

                    var maxHeight = _sliderValBox.Height * 0.6;
                    var maxCount = valCnts.Max();
                    var maxVal = vals.Max();
                    var baseY = _sliderValBox.Bottom;

                    for (int i = 0; i < 257; i++) //need to look deeper into why classifications coming in at 256
                    {
                        if (vals.Contains(i) && maxVal > 0) 
                        {
                            var ix = vals.IndexOf(i);

                            var lineX = _sliderValBox.Left + _sliderValBox.Width * i / 256;
                            var lineY = (float)(baseY - 3 - maxHeight * valCnts[ix] / maxCount);

                            var mapped = 255 * i / maxVal;

                            Pen lineCol = new Pen(colors[mapped], _sliderValBox.Width / 256);

                            graphics.DrawLine(lineCol, lineX, lineY, lineX, baseY);
                        }
                        
                    }
                }
                

                //draw the slider
                _slider.DrawSlider(graphics, outLine);



                //the draw the filter button
                GH_Capsule filterButton = GH_Capsule.CreateTextCapsule(_filterButton, _filterButton, GH_Palette.Black, "Filter Field Values");
                filterButton.Render(graphics, Selected, Owner.Locked, false);
                filterButton.Dispose();

            }

        }


        //EVENT HANDLERS--------------------------------------------------------------------------------------------------
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Blank)
            {
                //radio buttons
                for (int i = 0; i < _radRecs.Buttons.Length; i++)
                {
                    if (_radRecs.Buttons[i].Contains(e.CanvasLocation))
                    {
                        Owner.RecordUndoEvent("Site reader button clicked");
                        _chosenField = i;
                        _selectField(_chosenField);

                        /* note sure why I can't access Owner.ExpireLayout() but the below works to refresh the display while NOT expiring the solution
                        https://discourse.mcneel.com/t/grasshopper-importBtn-should-i-expire-solution/117368
                        */
                        base.ExpireLayout();
                        sender.Refresh();

                        return GH_ObjectResponse.Handled;
                    }
                }

                //slider handles
                for (int i = 0; i < _handleRecs.Length; i++)
                {
                    if (_handleRecs[i].Contains(e.CanvasLocation))
                    {
                        _currentHandle = i;

                        //use the drag cursor
                        Grasshopper.Instances.CursorServer.AttachCursor(sender, "GH_NumericSlider");
                        _sliding = true;
                        return GH_ObjectResponse.Capture;
                    }
                }

            }
            return base.RespondToMouseDown(sender, e);
        }

        //slider handles moved
        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _sliding)
            {
                var recCenter = _handleRecs[_currentHandle].Left + _handleRecs[_currentHandle].Width / 2;
                _currentOffset = e.CanvasX - recCenter;
                _handleValues(_slider.HandPos);

                base.ExpireLayout();
                sender.Refresh();
                Owner.ExpirePreview(true);

                return GH_ObjectResponse.Ignore;
            }

            return base.RespondToMouseMove(sender, e);
        }

        //slider handle released
        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _sliding)
            {
                base.ExpireLayout();
                sender.Refresh();
                Owner.ExpirePreview(true);
                _handleValues(_slider.HandPos);

                _sliding = false;
                return GH_ObjectResponse.Release;
            }

            return base.RespondToMouseUp(sender, e);
        }

        //filter field button clicked
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _filterButton.Contains(e.CanvasLocation))
            {
                Owner.RecordUndoEvent("SiteReader field filtered");
                _filterFields();
                Owner.ExpireSolution(true);
                return GH_ObjectResponse.Handled;

            }

            return base.RespondToMouseDoubleClick(sender, e);
        }




    }
}
