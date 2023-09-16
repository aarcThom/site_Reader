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
using siteReader.UI.features;

namespace SiteReader.UI
{
    public class PreviewImport : GH_ComponentAttributes
    {

        // note to self: in C# use Action when you return void, and Func when you return value(s)
        /* documentation on base:
         https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base
         */

        public PreviewImport(GH_Component owner, Action<bool> previewCld, Action zoomCloud) : base(owner)
        {
            _importAction = previewCld;
            _zoomCloud = zoomCloud;
        }

        //FIELDS ------------------------------------------------------------------

        //return values
        private readonly Action<bool> _importAction;
        private readonly Action _zoomCloud;

        //rectangles and pts for layouts
        private RectangleF _importLegendBounds;
        private RectangleF _importBtnBounds;
        private RectangleF _zoomLegendBounds;
        private RectangleF _zoomButtonBounds;
        private Point _ptLeft;
        private Point _ptRight;

        //preview the Cloud?
        private bool _importCloud = false;
        private string _importBtnTxt = "false";
        private string _importLegendTxt = "Import the Cloud?";

        //zoom in on cloud button & Legend text
        private string _zoomLegendTxt = "Zoom in on Cloud";
        private string _zoomBttnText = "Zoom";

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
            const int extraHeight = 83;

            //here we can modify the bounds
            componentRec.Height += extraHeight; // for example

            //here we can assign the modified bounds to the component's bounds--------------------
            Bounds = componentRec;

            //here we can add extra stuff to the layout-------------------------------------------

            //points for divider lines

            _ptLeft = new Point(left + sideSpacer, bottom + horizSpacer / 2);
            _ptRight = new Point(right - sideSpacer, bottom + horizSpacer / 2);

            //the import legend
            _importLegendBounds = new RectangleF(left, bottom + horizSpacer, width, 10);
            _importLegendBounds.Inflate(-sideSpacer * 2, 0);

            //the import button
            _importBtnBounds = new RectangleF(left, _importLegendBounds.Bottom + horizSpacer * 0.25f, width, 20);
            _importBtnBounds.Inflate(-sideSpacer, 0);


            //the zoom legend
            _zoomLegendBounds = new RectangleF(left, _importBtnBounds.Bottom + horizSpacer * 0.5f, width, 10);
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


        //handling zoom button
        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
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
    }
}
