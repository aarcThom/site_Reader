using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader.Methods
{
    public static class CColors
    {
        public static ColorBlend Rainbow => GetRainbow();

        //color blend methods
        public static ColorBlend GetRainbow()
        {
            ColorBlend colorBlend = new ColorBlend();
            colorBlend.Colors = new Color[] { Color.Red, Color.Yellow, Color.Green };
            colorBlend.Positions = new float[] { 0.0f, 0.5f, 1.0f };
            
            return colorBlend;
        }


        //interpolation methods

        public static Color InterpolateColor(ColorBlend colorBlend, float position)
        {
            int colorCount = colorBlend.Colors.Length;
            float[] positions = colorBlend.Positions;

            // Find the index of the color stop before the given position
            int startIndex = 0;
            for (int i = 1; i < colorCount; i++)
            {
                if (position <= positions[i])
                {
                    startIndex = i - 1;
                    break;
                }
            }

            // Calculate the fraction between the two color stops
            float fraction = (position - positions[startIndex]) / (positions[startIndex + 1] - positions[startIndex]);

            // Linearly interpolate between the colors
            Color startColor = colorBlend.Colors[startIndex];
            Color endColor = colorBlend.Colors[startIndex + 1];

            int red = (int)(startColor.R + fraction * (endColor.R - startColor.R));
            int green = (int)(startColor.G + fraction * (endColor.G - startColor.G));
            int blue = (int)(startColor.B + fraction * (endColor.B - startColor.B));

            return Color.FromArgb(red, green, blue);
        }
    }
}
