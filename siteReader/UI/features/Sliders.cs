using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Grasshopper.Kernel.Geometry;
using Eto.Forms;

namespace siteReader.UI.features
{
    public class Sliders
    {
        //FIELDS--------------------------------------------------
        private float _left;
        private float _right;
        private float _top;

        private bool _drawBar;


        private RectangleF[] _sliderRecs;
        private float[] _sliderMaxPos;
        private float[] _sliderMinPos;
        private int _sliderDiameter = 8;
        //PROPERTIES----------------------------------------------

        public Sliders(float sliderTop, Rectangle comp, int numSliders, int sideSpace, bool drawBar = true)
        {
            _drawBar = drawBar;

            _top = sliderTop;
            _left = comp.Left + sideSpace;
            _right = comp.Right - sideSpace - _sliderDiameter;
            var sliderWidth = _right - _left;

            float handleSpace;
            if (numSliders > 1)
            {
                handleSpace = sliderWidth / (numSliders - 1);
            }
            else
            {
                handleSpace = 0;
            }

            _sliderRecs = new RectangleF[numSliders];

            for (int i = 0; i < numSliders; i++)
            {
                var sliderX = handleSpace * i + _left;
                RectangleF rec = new RectangleF(sliderX, _top, _sliderDiameter, _sliderDiameter);
                _sliderRecs[i] = rec;

                //setting up initial min and max positions of slider handles
                if (i == 0)
                {

                }
            }
        }

        public void Draw(Graphics graphics, Pen outline)
        {
            //slider bar (if drawn)
            if (_drawBar)
            {
                var barY = _top + _sliderDiameter / 2;
                graphics.DrawLine(outline, _left, barY, _right, barY);
            }
            
            //slider handle(s)
            foreach(var slider in _sliderRecs)
            {
                graphics.FillEllipse(CompStyles.HandleFill, slider);
                graphics.DrawEllipse(outline, slider);
            }
        }
    }
}
