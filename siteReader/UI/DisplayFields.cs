﻿using System;
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
 

        private int _chosenField = -1; // what field the user picks

        //rectangles and pts for layouts
        private Point _ptLeft;
        private Point _ptRight;

        //the radiorectangles for buttons
        private RadioButtons _radRecs;

        //the rectangle for the gradient slider
        private RectangleF _gradientRect;

        //slider handles
        private int _numHandles = 2;
        private int _handleDiameter = 8;
        private HorizSliders _slider;
        private RectangleF[] _handleRecs;
        private int _currentHandle;
        private float _currentOffset;
        private bool _sliding = false;

        private string _fieldLegendTxt = "LIDAR field to display";

        // the selections
        private string[] _fields = new string[4] { "Intensity", "RGB", "Classification", "Number of Returns" };


        //CONSTRUCTOR -------------------------------------------------------------------------------------------
        // note to self: in C# use Action when you return void, and Func when you return value(s)
        /* documentation on base:
         https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/base
         */

        public DisplayFields(GH_Component owner, Action<int> selectField, Action<List<float>> handleValues) : base(owner)
        {
            _selectField = selectField;
            _slider = new HorizSliders(_numHandles, _handleDiameter);
            _handleValues = handleValues;
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
            const int extraHeight = 160;
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


            //the gradient graph
            var gradRectTop = _radRecs.Buttons.Last().Bottom + vertSpace;
            _gradientRect = new RectangleF(left, gradRectTop, width, 50);
            _gradientRect.Inflate(-sideSpacer * 2, 0);

            //the sliders 
            var sliderTop = gradRectTop + horizSpace;
            var sliderLeft = left + sideSpacer + _handleDiameter / 2;
            var sliderRight = right - sideSpacer - _handleDiameter / 2;
            var sliderWidth = sliderRight - sliderLeft;

            if (_sliding)
            {
                _slider.MoveSlider(_currentHandle, _currentOffset);
            }

            _handleRecs = _slider.LayoutSlider(sliderLeft, sliderTop, sliderWidth);
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

                //drawing the gradient graph outline
                var gradRect = new RectangleF[1] { _gradientRect };
                graphics.DrawRectangles(outLine, gradRect);

                //draw the slider
                _slider.DrawSlider(graphics, outLine);

            }

        }


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


    }
}
