using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader.Methods
{
    public static class ASPR
    {
        public static Color GetIntensityCol(ushort iVal)
        {
            float remapped = RemapIntensity(iVal);
            return CColors.InterpolateColor(CColors.Rainbow, remapped);
        }

        public static float RemapIntensity(ushort iVal)
        {
            float maxVal = 65535;
            float val = Convert.ToSingle(iVal);
            return val / maxVal;
        }
    }
}
