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
using siteReader.UI;

namespace SiteReader.UI
{
    public class DensityZoom : GH_ComponentAttributes
    {

        // note to self: in C# use Action when you return void, and Func when you return value(s)
        /* documentation on base:
         https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base
         */

        public DensityZoom(GH_Component owner, Action<float> sliderValue, Action<bool> previewCld, Action zoomCloud) : base(owner)
        {
            _returnSliderVal = sliderValue;
            _importAction = previewCld;
            _zoomCloud = zoomCloud;
        }

        //FIELDS ------------------------------------------------------------------

        //return values
        private readonly Action<float> _returnSliderVal;
        private readonly Action<bool> _importAction;
        private readonly Action _zoomCloud;

        //rectangles for layouts
        private RectangleF _importLegendBounds;
        private RectangleF _importBtnBounds;
        private RectangleF _zoomLegendBounds;
        private RectangleF _zoomButtonBounds;
        private RectangleF _sliderLegendBounds;
        private RectangleF _sliderBounds;
        private RectangleF _handleShape;
        private PointF _handleNum;
        private RectangleF _secondCapsuleBounds;

        //preview the Cloud?
        private bool _importCloud = false;
        private string _importBtnTxt = "false";
        private string _importLegendTxt = "Import the Cloud?";

        //zoom in on cloud button & Legend text
        private string _zoomLegendTxt = "Zoom in on Cloud";
        private string _zoomBttnText = "Zoom";

        //string legend for selecting cloud density
        private string _densityLegendTxt = "Cloud Density Factor";

        //field for slider handle position and preview number
        private bool _slid = false;
        private bool _currentlySliding = false;
        private float _handlePosX;
        private float _handlePosY;
        private float _handleWidth = 8;


        private float _curHandleOffset = 0;
        private List<float> _handleOffsets;
        private float _cloudDensity = 0;


        protected override void Layout()
        {
            base.Layout(); //handles the basic layout, computes the bounds, etc.
            Rectangle componentRec = GH_Convert.ToRectangle(Bounds); //getting component base bounds

            //saving the original bounds to refer to in custom layout
            var left = componentRec.Left;
            var top = componentRec.Top;
            var right = componentRec.Right;
            var bottom = componentRec.Bottom;
            var width = componentRec.Width;
            var height = componentRec.Height;

            //useful layout variables like spacers, etc.
            /* docs on constants
             https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/constants
             */

            const int horizSpacer = 10;
            const int sideSpacer = 2;
            const int extraHeight = 140;

            //here we can modify the bounds
            componentRec.Height += extraHeight; // for example

            //here we can assign the modified bounds to the component's bounds--------------------
            Bounds = componentRec;

            //here we can add extra stuff to the layout-------------------------------------------
            //_secondCapsuleBounds = new RectangleF(left, bottom, width, extraHeight);

            _sliderLegendBounds = new RectangleF(left, bottom + horizSpacer, width, 10);
            _sliderLegendBounds.Inflate(-sideSpacer * 2, 0);

            _sliderBounds = new RectangleF(left, _sliderLegendBounds.Bottom + horizSpacer * 0.25f, width, 20);
            _sliderBounds.Inflate(-sideSpacer * 4, 0);


            //slider handle and code to move it properly
            _handleWidth = 8;
            _handlePosY = _sliderBounds.Height / 2 - _handleWidth / 2 + _sliderBounds.Top;


            //getting the handle snap locations
            _handleOffsets = new List<float>();
            for (int i = 0; i < 11; i++)
            {
                var iFl = (float)i;
                var offsetX = iFl * (_sliderBounds.Width / 10) - _handleWidth / 2;
                _handleOffsets.Add(offsetX);
            }

            //getting current position
            if (!_slid)
            {
                _handlePosX = _sliderBounds.Left - _handleWidth / 2;
            }

            else
            {
                _handlePosX = _sliderBounds.Left + _curHandleOffset;
            }


            _handleShape = new RectangleF(_handlePosX, _handlePosY, _handleWidth, _handleWidth);
            _handleNum = new PointF(_handlePosX + _handleWidth / 2, _handlePosY + _handleWidth + 5);


            //the import legend
            _importLegendBounds = new RectangleF(left, _sliderBounds.Bottom + horizSpacer, width, 10);
            _importLegendBounds.Inflate(-sideSpacer * 2, 0);

            //the import button
            _importBtnBounds = new RectangleF(left, _importLegendBounds.Bottom + horizSpacer * 0.25f, width, 20);
            _importBtnBounds.Inflate(-sideSpacer, 0);


            //the zoom legend
            _zoomLegendBounds = new RectangleF(left, _importBtnBounds.Bottom + horizSpacer, width, 10);
            _zoomLegendBounds.Inflate(-sideSpacer * 2, 0);

            //the zoom importBtn
            _zoomButtonBounds = new RectangleF(left, _zoomLegendBounds.Bottom + horizSpacer * 0.25f, width, 20);
            _zoomButtonBounds.Inflate(-sideSpacer, 0);


        }

        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            base.Render(canvas, graphics, channel); // handle the wires, draw nickname, name, etc.

            //the main component rendering channel
            if (channel == GH_CanvasChannel.Objects)
            {
                //declare the pens / brushes / pallets we will need to draw the custom objects - defaults for blank / message levels
                Pen outLine = CompStyles.BlankOutline;
                GH_Palette palette = GH_Palette.Normal;

                //use a switch statement to retrieve the proper pens / brushes from our CompColors class
                switch (Owner.RuntimeMessageLevel)
                {
                    case GH_RuntimeMessageLevel.Warning:
                        // assign warning values
                        outLine = CompStyles.WarnOutline;
                        palette = GH_Palette.Warning;
                        break;

                    case GH_RuntimeMessageLevel.Error:
                        // assign warning values
                        outLine = CompStyles.ErrorOutline;
                        palette = GH_Palette.Error;
                        break;
                }


                //render custom elements----------------------------------------------------------

                //secondary capsule
                //GH_Capsule secondCap = GH_Capsule.CreateCapsule(_secondCapsuleBounds, palette);
                //secondCap.Render(graphics, Selected, Owner.Locked, false);
                //secondCap.Dispose();

                //font for legends and slider number value
                Font font = GH_FontServer.Small;
                // adjust fontsize to high resolution displays
                font = new Font(font.FontFamily, font.Size / GH_GraphicsUtil.UiScale, FontStyle.Regular);


                //slider legend
                graphics.DrawString(_densityLegendTxt, font, Brushes.Black, _sliderLegendBounds, GH_TextRenderingConstants.NearCenter);

                //slider line
                var sliderY = _sliderBounds.Top + _sliderBounds.Height / 2;
                graphics.DrawLine(outLine, _sliderBounds.Left, sliderY, _sliderBounds.Right, sliderY);

                //slider line vertical ticks
                int count = 0;
                foreach (var offset in _handleOffsets)
                {
                    var tickX = offset + _sliderBounds.Left + _handleWidth / 2;

                    float top;
                    if (count % 5 == 0)
                    {
                        top = _sliderBounds.Top + 2;
                    }
                    else
                    {
                        top = _sliderBounds.Top + 5;
                    }

                    graphics.DrawLine(outLine, tickX, sliderY, tickX, top);

                    count++;
                }

                //slider handle
                graphics.FillEllipse(CompStyles.HandleFill, _handleShape);
                graphics.DrawEllipse(outLine, _handleShape);

                //density number text
                graphics.DrawString(_cloudDensity.ToString(), font, Brushes.Black, _handleNum, GH_TextRenderingConstants.CenterCenter);

                //import legend 
                graphics.DrawString(_importLegendTxt, font, Brushes.Black, _importLegendBounds, GH_TextRenderingConstants.NearCenter);

                //import cloud Button
                GH_Capsule importBtn = GH_Capsule.CreateTextCapsule(_importBtnBounds, _importBtnBounds, GH_Palette.Black, _importBtnTxt);
                importBtn.Render(graphics, Selected, Owner.Locked, false);
                importBtn.Dispose();

                //zoom legend 
                graphics.DrawString(_zoomLegendTxt, font, Brushes.Black, _zoomLegendBounds, GH_TextRenderingConstants.NearCenter);

                //zoom Button
                GH_Capsule zoomButton = GH_Capsule.CreateTextCapsule(_zoomButtonBounds, _zoomButtonBounds, GH_Palette.Black, _zoomBttnText);
                zoomButton.Render(graphics, Selected, Owner.Locked, false);
                zoomButton.Dispose();

            }

        }

        /* See David Rutten's explanation of GH_ObjectResponse
         * https://discourse.mcneel.com/t/why-i-cannot-move-a-custom-component/72959/4
         */

        //handling double clicks for import
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_importBtnBounds.Contains(e.CanvasLocation))
                {
                    Owner.RecordUndoEvent("SiteReader button clicked");
                    _importCloud = _importCloud == false;
                    _importAction(_importCloud); //return the value to the component
                    _importBtnTxt = _importCloud.ToString();
                    Owner.ExpireSolution(true);
                    return GH_ObjectResponse.Handled;
                }
            }

            return base.RespondToMouseDoubleClick(sender, e);
        }


        //handling slider and zoom button
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && !_importCloud)
            {
                if (_handleShape.Contains(e.CanvasLocation))
                {
                    //use the drag cursor
                    Grasshopper.Instances.CursorServer.AttachCursor(sender, "GH_NumericSlider");
                    _currentlySliding = true;
                    return GH_ObjectResponse.Capture;
                }
            }

            //if the user clicks the zoom button
            if (e.Button == MouseButtons.Left)
            {
                if (_zoomButtonBounds.Contains(e.CanvasLocation))
                {
                    Owner.RecordUndoEvent("SiteReader zoom on cloud");
                    _zoomCloud(); //trigger the zoom on the owner component

                    return GH_ObjectResponse.Handled;
                }
            }


            return base.RespondToMouseDown(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && _currentlySliding)
            {
                _slid = true;

                var currentX = e.CanvasX;

                /* NOT NEEDED FOR SNAPPING SLIDER - USE FOR SLIDING SLIDER

                //slide the handle around within limits
                if (currentX < _sliderBounds.Left)
                {
                    _curHandleOffset = -_handleWidth / 2;
                } 
                else if (currentX > _sliderBounds.Right)
                {
                    _curHandleOffset = _sliderBounds.Width - _handleWidth / 2;
                }
                else
                {
                    _curHandleOffset = currentX - _sliderBounds.Left - _handleWidth / 2;
                }
                */

                //update the number below the handle to snap to the nearest whole value + snap the slider
                var currentVal = currentX - _sliderBounds.Left;
                _curHandleOffset =
                    _handleOffsets.Aggregate((x, y) => Math.Abs(x - currentVal) < Math.Abs(y - currentVal) ? x : y);
                _cloudDensity = (_curHandleOffset + _handleWidth / 2) / _sliderBounds.Width;



                /* note sure why I can't access Owner.ExpireLayout() but the below works to refresh the display while NOT expiring the solution
                 https://discourse.mcneel.com/t/grasshopper-importBtn-should-i-expire-solution/117368
                 */
                base.ExpireLayout();
                sender.Refresh();

                return GH_ObjectResponse.Ignore;
            }

            return base.RespondToMouseMove(sender, e);
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {

            if (e.Button == MouseButtons.Left && _currentlySliding)
{
                _returnSliderVal(_cloudDensity); // return the final value

                // again, we don't want to refresh the solution until the display importBtn is clicked
                base.ExpireLayout();
                sender.Refresh();

                _currentlySliding = false;
                return GH_ObjectResponse.Release;

            }
            return base.RespondToMouseUp(sender, e);
        }
    }


}
