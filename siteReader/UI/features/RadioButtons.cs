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
        /// <summary>
        /// Creates a list of radio buttons plus a legend.
        /// </summary>
        /// <param name="component">The parent component's bounding rectangle</param>
        /// <param name="btnList">The names of all the radio buttons.</param>
        /// <param name="legend">The Radio button section name.</param>
        /// <param name="horizSpace">Space between buttons and button labels.</param>
        /// <param name="vertSpace">Vertical space between radio buttons.</param>
        /// <param name="sideSpace">Space from edge of component to buttons.</param>
        /// <param name="startPt">The top point of the legend. All elements will be drawn below this.</param>
        public RadioButtons(Rectangle component, string[] btnList, string legend, int horizSpace, int vertSpace, int sideSpace, float startPt)
        {
            var left = component.Left;
            var width = component.Width;

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
        /// <summary>
        /// Draws the radio boxes. Calling during in the Render method of a GH_ComponentsAtttributes class.
        /// </summary>
        /// <param name="outline"> The outline of the radio buttons.</param>
        /// <param name="buttonFont">The font for the buttons' labels.</param>
        /// <param name="legendFont">The font for the radio buttons' legend</param>
        /// <param name="graphics">Graphics param from Render method.</param>
        /// <param name="selection">The selected radio button. Anything >=0 will select a radio button.</param>

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
