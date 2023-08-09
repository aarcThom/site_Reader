using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader.Methods
{
    public static class Utility
    {
        /// <summary>
        /// tests if file path is .las or .laz
        /// </summary>
        /// <param name="path">the file path to test</param>
        /// <returns>true if .las or .laz</returns>
        public static bool TestLasExt(string path)
        {
            string fileExt = Path.GetExtension(path);

            if (fileExt == ".las" || fileExt == ".laz")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// formats dictionary for GH textual output
        /// </summary>
        /// <param name="stringDict"></param>
        /// <returns>list<string> for GH output</returns>
        public static List<string> StringDictGhOut(Dictionary<string, string> stringDict)
        {
            List<string> ghOut = new List<string>();

            if (stringDict.Count == 0)
            {
                return new List<string> { "No VLRs found." };
            }


            foreach (string key in stringDict.Keys)
            {
                ghOut.Add($"{key} : {stringDict[key]}");
            }

            return ghOut;
        }


        public static List<string> FloatDictGhOut(Dictionary<string, float> floatDict, GH_Component owner)
        {
            List<string> ghOut = new List<string>();

            if (floatDict.Count == 0)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "LAS header not found. The LAS spec. needs a header. " +
                    "Double check your data source as this will probably cause errors down the road.");
                return ghOut;
            }


            foreach (string key in floatDict.Keys)
            {
                ghOut.Add($"{key} : {floatDict[key]}");
            }

            return ghOut;
        }

        public static Color ConvertRGB(ushort[] arrIN)
        {
            int r = Convert.ToInt32(arrIN[0]) / 256;
            int b = Convert.ToInt32(arrIN[1]) / 256;
            int g = Convert.ToInt32(arrIN[2]) / 256;

            return Color.FromArgb(r, b, g);
        }
    }
}
