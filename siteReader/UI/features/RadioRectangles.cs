using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader.UI.features
{
    public class RadioRectangles
    {
        // FIELDS-----------------------------------------
        private RectangleF[] _legends;
        private RectangleF[] _selectors;
        private RectangleF[] _buttons;

        // PROPERTIES-------------------------------------

        public RectangleF[] Legends => _legends;
        public RectangleF[] Buttons => _buttons;
        public RectangleF[] Selectors => _selectors;

        // CONSTRUCTORS-----------------------------------
        public RadioRectangles(Rectangle component, string[] btnList, int horizSpace, int vertSpace, float startPt)
        {
            var left = component.Left;
            var top = component.Top;
            var right = component.Right;
            var bottom = component.Bottom;
            var width = component.Width;
            var height = component.Height;

            _legends = new RectangleF[btnList.Length];
            _selectors = new RectangleF[btnList.Length];
            _buttons = new RectangleF[btnList.Length];

            float butTop;
            for (int i = 0; i < btnList.Length; i++)
            {
                if (i == 0)
                {
                    butTop = startPt + vertSpace;
                }
                else
                {
                    butTop = _buttons[i - 1].Bottom + vertSpace;
                }
                var butRec = new RectangleF(left + horizSpace, butTop, 7, 7);
                var legRec = new RectangleF(butRec.Right + horizSpace, butRec.Top - 1, 100, butRec.Height + 2);
                var selectRec = butRec;
                selectRec.Inflate(-1, -1);

                _legends[i] = legRec;
                _selectors[i] = selectRec;
                _buttons[i] = butRec;
            }

        }
    }
}
