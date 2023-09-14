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
using siteReader.UI;

namespace SiteReader.UI
{
    public class RadioBoxes : GH_ComponentAttributes
    {

        // note to self: in C# use Action when you return void, and Func when you return value(s)
        /* documentation on base:
         https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base
         */

        public RadioBoxes(GH_Component owner, Action<int> selectField) : base(owner)
        {
            _selectField = selectField;
        }

        //FIELDS ------------------------------------------------------------------

        //return values
        private readonly Action<int> _selectField;

        //rectangles and pts for layouts
        private RectangleF _fieldLegendBounds;
        private Point _ptLeft;
        private Point _ptRight;

        private RectangleF _intensButBnds;
        private RectangleF _intensLgdBnds;
        private RectangleF _intensSelect;
        private RectangleF _rgbButBnds;
        private RectangleF _rgbLgdBnds;
        private RectangleF _rgbSelect;
        private RectangleF _classButBnds;
        private RectangleF _classLgdBnds;
        private RectangleF _classSelect;
        private RectangleF _returnsButBnds;
        private RectangleF _returnsLgdBnds;
        private RectangleF _returnsSelect;

        private RectangleF[] _radioButtons;
        private RectangleF[] _radioSelects;

        private int _chosenField = -1; // what field the user picks


    private string _fieldLegendTxt = "LIDAR field to display";


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
            const int extraHeight = 95;

            //here we can modify the bounds
            componentRec.Height += extraHeight; // for example

            //here we can assign the modified bounds to the component's bounds--------------------
            Bounds = componentRec;

            //here we can add extra stuff to the layout-------------------------------------------

            //points for divider lines

            _ptLeft = new Point(left + sideSpacer, bottom + horizSpacer / 2);
            _ptRight = new Point(right - sideSpacer, bottom + horizSpacer / 2);

            //the field legend
            _fieldLegendBounds = new RectangleF(left, bottom + horizSpacer, width, 10);
            _fieldLegendBounds.Inflate(-sideSpacer * 2, 0);

            // the buttons for the fields
            _intensButBnds = new RectangleF(left + 8, _fieldLegendBounds.Bottom + horizSpacer, 7, 7);
            _rgbButBnds = new RectangleF(left + 8, _intensButBnds.Bottom + horizSpacer, 7, 7);
            _classButBnds = new RectangleF(left + 8, _rgbButBnds.Bottom + horizSpacer, 7, 7);
            _returnsButBnds = new RectangleF(left + 8, _classButBnds.Bottom + horizSpacer, 7, 7);
            _radioButtons = new[] { _intensButBnds, _rgbButBnds, _classButBnds, _returnsButBnds };

            // the selections for the fields
            _intensSelect = _intensButBnds;
            _intensSelect.Inflate(-1, -1);
            _rgbSelect = _rgbButBnds;
            _rgbSelect.Inflate(-1, -1);
            _classSelect = _classButBnds;
            _classSelect.Inflate(-1, -1);
            _returnsSelect = _returnsButBnds;
            _returnsSelect.Inflate(-1, -1);

            _radioSelects = new[] { _intensSelect, _rgbSelect, _classSelect, _returnsSelect }; 

            // the legends for the fields
            _intensLgdBnds = new RectangleF(_intensButBnds.Right + 8, _intensButBnds.Top - 1, 100, _intensButBnds.Height + 2);
            _rgbLgdBnds = new RectangleF(_rgbButBnds.Right + 8, _rgbButBnds.Top - 1, 100, _rgbButBnds.Height + 2);
            _classLgdBnds = new RectangleF(_classButBnds.Right + 8, _classButBnds.Top - 1, 100, _classButBnds.Height + 2);
            _returnsLgdBnds = new RectangleF(_returnsButBnds.Right + 8, _returnsButBnds.Top - 1, 100, _returnsButBnds.Height + 2);



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

                //field legend 
                graphics.DrawString(_fieldLegendTxt, font, Brushes.Black, _fieldLegendBounds, GH_TextRenderingConstants.NearCenter);

                //drawings the radio buttons
                graphics.FillRectangles(CompStyles.RadioUnclicked, _radioButtons);
                graphics.DrawRectangles(outLine, _radioButtons);

                //drawing the clicked radion buttons if selected
                if (_chosenField >= 0)
                {
                    var clickRec = new[] { _radioSelects[_chosenField] };
                    graphics.FillRectangles(CompStyles.RadioClicked, clickRec);
                }

                

                //drawing the button legends
                graphics.DrawString("Intensity", font, Brushes.Black, _intensLgdBnds, GH_TextRenderingConstants.NearCenter);
                graphics.DrawString("RGB", font, Brushes.Black, _rgbLgdBnds, GH_TextRenderingConstants.NearCenter);
                graphics.DrawString("Classification", font, Brushes.Black, _classLgdBnds, GH_TextRenderingConstants.NearCenter);
                graphics.DrawString("Number of Returns", font, Brushes.Black, _returnsLgdBnds, GH_TextRenderingConstants.NearCenter);


            }

        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < _radioButtons.Length; i++)
                {
                    if (_radioButtons[i].Contains(e.CanvasLocation))
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
            }


            return base.RespondToMouseDown(sender, e);
        }

    }
}
