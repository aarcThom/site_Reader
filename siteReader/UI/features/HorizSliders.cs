using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Grasshopper.Kernel.Geometry;
using Rhino.Input.Custom;
using Rhino.UI.Controls;
using Eto.Forms;
using Newtonsoft.Json;

namespace siteReader.UI.features
{
    public class HorizSliders
    {
        //FIELDS--------------------------------------------------
        private float _xLeft;
        private float _xRight;
        private float _y;

        private bool _drawBar;

        private int _sliderDiameter;

        private HorizSliderHandle[] _handles;
        private float[] _offsets;

        //PROPERTIES----------------------------------------------

        public HorizSliderHandle[] Handles => _handles;

        public HorizSliders(float sliderTop, Rectangle comp, int numSliders, int sideSpace, float[] offsets, bool drawBar = true, int diameter = 8)
        {
            _drawBar = drawBar;
            _sliderDiameter = diameter;
            _offsets = offsets;

            _y = sliderTop + _sliderDiameter / 2;
            _xLeft = comp.Left + sideSpace + _sliderDiameter / 2;
            _xRight = comp.Right - sideSpace - _sliderDiameter / 2;
            var sliderWidth = _xRight - _xLeft;

            float handleSpace;
            if (numSliders > 1)
            {
                handleSpace = sliderWidth / (numSliders - 1);
            }
            else
            {
                handleSpace = 0;
            }

            //initial handle rectangles
            _handles = new HorizSliderHandle[numSliders];

            for (int i = 0; i < numSliders; i++)
            {
                var sliderX = handleSpace * i + _xLeft + _offsets[i];
                HorizSliderHandle handle = new HorizSliderHandle(sliderX, _y, _sliderDiameter, i, _handles);
                _handles[i] = handle;
            }

            //set initial handle min / max values
            if (numSliders == 1)
            {
                _handles[0].MinX = _xLeft;
                _handles[0].MaxX = _xRight;
            }
            else
            {
                //set the left most and right most handles 
                _handles[0].MinX = _xLeft;
                _handles[0].MaxX = _handles[1].Xpos;

                _handles.Last().MinX = _handles[numSliders - 2].Xpos;
                _handles.Last().MaxX = _xRight;

                for (int i = 1; i < numSliders - 1; i++)
                {
                    _handles[i].MinX = _handles[i - 1].Xpos;
                    _handles[i].MaxX = _handles[i + 1].Xpos;
                }
            }
        }

        public void Draw(Graphics graphics, Pen outline)
        {
            //slider bar (if drawn)
            if (_drawBar)
            {
                graphics.DrawLine(outline, _xLeft, _y, _xRight, _y);
            }
            
            //slider handle(s)
            foreach(var handle in _handles)
            {
                handle.Draw(graphics, outline);
            }
        }
    }

    public class HorizSliderHandle
    {
        //FIELDS ---------------------------------
        private float _yPos; // center position of the handle
        private float _xPos;

        private float _left; // the top left point for drawing a rectangle
        private float _top;

        private float _handleDia; //handle diameter

        private float _maxX; //the extents that the slider can slider
        private float _minX;

        private int _index; //the position of the handle in the list

        private float _offset; //the offset caused by user moving

        private RectangleF _rec;
        private HorizSliderHandle[] _nbrhd;

        //PROPERTIES -----------------------------
        public float Xpos
        {
            get => _xPos;
            set => _xPos = value;
        }

        public float MaxX
        {
            get => _maxX;
            set => _maxX = value;
        }

        public float MinX
        {
            get => _minX;
            set => _minX = value;
        }

        public float HandleDia
        {
            get => _handleDia;
            set => _handleDia = value;
        }

        public RectangleF Rect => _rec;

        public float Offset => _offset;

        public HorizSliderHandle(float x, float y, float dia, int index, HorizSliderHandle[] neighbourhood)
        {
            _left = x - dia / 2;
            _top = y - dia / 2;

            _xPos = x;
            _yPos = y;

            _handleDia = dia;

            _rec = new RectangleF(_left, _top, _handleDia, _handleDia);
            _index = index;
            _nbrhd = neighbourhood;

            _offset = 0;
        }

        public void Draw(Graphics graphics, Pen outline)
        {
            graphics.FillEllipse(CompStyles.HandleFill, _rec);
            graphics.DrawEllipse(outline, _rec);
        }


    }
}
