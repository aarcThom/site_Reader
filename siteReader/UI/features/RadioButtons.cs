using Grasshopper.GUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader.UI.features
{
    public class RadioButtons
    {
        // FIELDS-----------------------------------------
        private RectangleF[] _textRecs;
        private RectangleF[] _selectors;
        private RectangleF[] _buttons;
        private RectangleF _legendRec;

        private string[] _fieldNames;
        private string _legend;

        // PROPERTIES-------------------------------------
        public RectangleF[] Buttons => _buttons;

        // CONSTRUCTORS-----------------------------------
        public RadioButtons(Rectangle component, string[] btnList, string legend, int horizSpace, int vertSpace, int sideSpace, float startPt)
        {
            var left = component.Left;
            var top = component.Top;
            var right = component.Right;
            var bottom = component.Bottom;
            var width = component.Width;
            var height = component.Height;

            _textRecs = new RectangleF[btnList.Length];
            _selectors = new RectangleF[btnList.Length];
            _buttons = new RectangleF[btnList.Length];

            _fieldNames = btnList;
            _legend = legend;

  
            _legendRec = new RectangleF(left, startPt, width, 10);
            _legendRec.Inflate(-sideSpace, 0);


            float butTop;
            for (int i = 0; i < btnList.Length; i++)
            {
                if (i == 0)
                {
                    butTop = _legendRec.Bottom + vertSpace;
                }
                else
                {
                    butTop = _buttons[i - 1].Bottom + vertSpace;
                }
                var butRec = new RectangleF(left + horizSpace, butTop, 7, 7);
                var txtRec = new RectangleF(butRec.Right + horizSpace, butRec.Top - 1, 
                    width - butRec.Width - horizSpace * 2, butRec.Height + 2);
                var selectRec = butRec;
                selectRec.Inflate(-1, -1);

                _textRecs[i] = txtRec;
                _selectors[i] = selectRec;
                _buttons[i] = butRec;
            }

        }

        //DRAWING THE RECTANGLES
        public void Draw(Pen outline, Font buttonFont, Font legendFont, Graphics graphics, int selection)
        {
            //draw the legend
            graphics.DrawString(_legend, legendFont, Brushes.Black, _legendRec, GH_TextRenderingConstants.NearCenter);


            //drawing the radio buttons
            graphics.FillRectangles(CompStyles.RadioUnclicked, _buttons);
            graphics.DrawRectangles(outline, _buttons);

            //drawing the clicked button
            if (selection >= 0)
            {
                var clickRec = new[] { _selectors[selection] };
                graphics.FillRectangles(CompStyles.RadioClicked, clickRec);
            }

            //drawing the button legends
            for (int i = 0; i < _fieldNames.Length; i++)
            {
                var text = _fieldNames[i];
                var rec = _textRecs[i];
                graphics.DrawString(text, buttonFont, Brushes.Black, rec, GH_TextRenderingConstants.NearCenter);
            }

        }
    }
}
