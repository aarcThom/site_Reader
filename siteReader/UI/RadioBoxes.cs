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

namespace SiteReader.UI
{
    public class RadioBoxes : GH_ComponentAttributes
    {

        //FIELDS -----------------------------------------------------------------------------------------------

        //return values
        private readonly Action<int> _selectField;

        //rectangles and pts for layouts
        private RectangleF _fieldLegendBounds;
        private Point _ptLeft;
        private Point _ptRight;

        private int _chosenField = -1; // what field the user picks

        private string _fieldLegendTxt = "LIDAR field to display";

        // the selections
        private string[] _fields =new string[4]{ "Intensity", "RGB", "Classification", "Number of Returns" };

        //the radiorectangles for buttons
        private RadioRectangles _radRecs;

        //CONSTRUCTOR -------------------------------------------------------------------------------------------
        // note to self: in C# use Action when you return void, and Func when you return value(s)
        /* documentation on base:
         https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base
         */

        public RadioBoxes(GH_Component owner, Action<int> selectField) : base(owner)
        {
            _selectField = selectField;
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
            const int sideSpacer = 2;
            const int extraHeight = 95;

            //here we can modify the bounds
            componentRec.Height += extraHeight; // for example

            //here we can assign the modified bounds to the component's bounds--------------------
            Bounds = componentRec;

            //here we can add extra stuff to the layout-------------------------------------------

            //points for divider lines

            _ptLeft = new Point(left + sideSpacer, bottom + vertSpace / 2);
            _ptRight = new Point(right - sideSpacer, bottom + vertSpace / 2);

            //the field legend
            _fieldLegendBounds = new RectangleF(left, bottom + vertSpace, width, 10);
            _fieldLegendBounds.Inflate(-sideSpacer * 2, 0);

            //the radio buttons
            _radRecs = new RadioRectangles(componentRec, _fields, 8, vertSpace, _fieldLegendBounds.Bottom);

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
                graphics.FillRectangles(CompStyles.RadioUnclicked, _radRecs.Buttons);
                graphics.DrawRectangles(outLine, _radRecs.Buttons);

                //drawing the clicked radion buttons if selected
                if (_chosenField >= 0)
                {
                    var clickRec = new[] { _radRecs.Selectors[_chosenField] };
                    graphics.FillRectangles(CompStyles.RadioClicked, clickRec);
                }

                //drawing the button legends
                for (int i = 0; i < _fields.Length; i++)
                {
                    var text = _fields[i];
                    var rec = _radRecs.Legends[i];

                    graphics.DrawString(text, font, Brushes.Black, rec, GH_TextRenderingConstants.NearCenter);
                }
            }

        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (e.Button == MouseButtons.Left && Owner.RuntimeMessageLevel == GH_RuntimeMessageLevel.Blank)
            {
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
            }


            return base.RespondToMouseDown(sender, e);
        }

    }
}
