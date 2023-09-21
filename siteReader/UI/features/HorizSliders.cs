using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace siteReader.UI.features
{
    public class HorizSliders
    {
        // FIELDS--------------------------------------------
        private List<float> _handPos;
        private List<float> _minPos;
        private List<float> _maxPos;

        private RectangleF[] _drawRecs;

        private int _numSliders;
        private int _dia;

        private bool _drawLine;
        private float _lineWidth;
        private float _lineLeft;
        private float _lineY;

        //PROPERTIES------------------------------------------
        public List<float> HandPos => _handPos;

        public HorizSliders(int numSliders, int handleDiameter, bool drawLine = true)
        {
            //the initial positions - evenly spaced between 0 and 1
            _handPos = Enumerable.Range(0, numSliders).Select(x => x / ((float)(numSliders - 1))).ToList();

            //the initial draw rectangles for the handles
            _drawRecs = new RectangleF[numSliders];

            //the initial max and min positions each slider can be slid to
            _minPos = new List<float> { 0 };
            _minPos.AddRange(_handPos.GetRange(0, numSliders - 1));

            _maxPos = _handPos.GetRange(1, numSliders - 1);
            _maxPos.Add(1);


            _numSliders = numSliders;

            _dia = handleDiameter;

            _drawLine = drawLine;
        }

        public RectangleF[] LayoutSlider(float leftSide, float top, float width)
        {
            RectangleF[] grabRecs = new RectangleF[_numSliders];
            for (int i = 0; i < _numSliders; i++)
            {
                float xPos;
                //if two handles are sharing a spot and the current handle is on the right side
                if (TestProximityLeft(i))
                {
                    xPos = leftSide + width * _handPos[i] - _dia / 2;
                    grabRecs[i] = new RectangleF(xPos + _dia / 2, top, _dia / 2, _dia);
                    
                }

                //if two handles are sharing a spot and the current handle is on the left side
                else if (TestProximityRight(i))
                {
                    xPos = leftSide + width * _handPos[i] - _dia / 2;
                    grabRecs[i] = new RectangleF(xPos, top, _dia / 2, _dia);
                }

                else
                {
                    xPos = leftSide + width * _handPos[i] - _dia / 2;
                    grabRecs[i] = new RectangleF(xPos, top, _dia, _dia);
                }

                _drawRecs[i] = new RectangleF(xPos, top, _dia, _dia);


            }

            _lineWidth = width;
            _lineLeft = leftSide;
            _lineY = top + _dia / 2;
      
            return grabRecs;
        }

        public void DrawSlider(Graphics graphics, Pen outline)
        {
            if (_drawLine)
            {
                graphics.DrawLine(outline, _lineLeft, _lineY, _lineLeft + _lineWidth, _lineY);
            }

            for (int i = 0; i < _numSliders; i++)
            {
                var handle = _drawRecs[i];
                var lineX = handle.Right - handle.Width / 2;
                System.Drawing.Drawing2D.FillMode fillMode = System.Drawing.Drawing2D.FillMode.Winding;

                if (TestProximityLeft(i))
                {
                    var fillPts = FillArc(handle, "right");
                    graphics.FillPolygon(CompStyles.HandleFill, fillPts, fillMode);
                    graphics.DrawArc(outline, handle, -90, 180);
                    graphics.DrawLine(outline, lineX, handle.Top, lineX, handle.Bottom);

                }
                else if (TestProximityRight(i))
                {
                    var fillPts = FillArc(handle, "left");
                    graphics.FillPolygon(CompStyles.HandleFill, fillPts, fillMode);
                    graphics.DrawArc(outline, handle, 90, 180);
                    graphics.DrawLine(outline, lineX, handle.Top, lineX, handle.Bottom);
                }
                else
                {
                    graphics.FillEllipse(CompStyles.HandleFill, handle);
                    graphics.DrawEllipse(outline, handle);
                }
            }
        }

        public void MoveSlider(int handleIX, float offset)
        {
            var offsetFactor = offset / _lineWidth;
            var posFactor = _handPos[handleIX] + offsetFactor;
            var maxX = _maxPos[handleIX];
            var minX = _minPos[handleIX];

            if (posFactor >= minX && posFactor <= maxX)
            {
                _handPos[handleIX] = posFactor;

                if (handleIX == 0 && _numSliders > 1)
                {
                    _minPos[1] = posFactor;
                }

                else if (handleIX == _numSliders - 1 && _numSliders > 1)
                {
                    _maxPos[handleIX - 1] = posFactor;
                }

                else if (_numSliders > 1)
                {
                    _minPos[handleIX + 1] = posFactor;
                    _maxPos[handleIX - 1] = posFactor;
                }

            }
            else
            {
                var bounds = new List<float>() { maxX, minX };
                _handPos[handleIX] = bounds.OrderBy(val => Math.Abs(posFactor - val)).First();
            }
        }

        private bool TestProximityLeft(int i)
        {
           if(i != 0 && Math.Abs(_handPos[i] - _handPos[i - 1]) < 0.02) return true;
           return false;
        }

        private bool TestProximityRight(int i)
        {
            if (i != _numSliders - 1 && Math.Abs(_handPos[i] - _handPos[i + 1]) < 0.02) return true;
            return false;
        }

        private PointF[] FillArc(RectangleF rec, string side)
        {
            List<float> angles;
            if (side == "left")
            {
                angles = Enumerable.Range(5, 11).Select(x => (float)((float)x / 2 * Math.PI / 5)).ToList();
            }
            else if (side == "right")
            {
                angles = Enumerable.Range(15, 6).Select(x => (float)((float)x / 2 * Math.PI / 5)).ToList();
                var angles2 = Enumerable.Range(1, 5).Select(x => (float)((float)x / 2 * Math.PI / 5)).ToList();
                angles.AddRange(angles2);
            }
            else return null;

            PointF[] points = new PointF[11];
            for (int i = 0; i < 11; i++)
            {
                var x = rec.Width / 2f * Math.Cos(angles[i]) + rec.Left + rec.Width / 2f;
                var y = rec.Width / 2f * Math.Sin(angles[i]) + rec.Top + rec.Height / 2f;

                points[i] = new PointF((float)x, (float)y);
            }

            return points;

        }
    }
}
