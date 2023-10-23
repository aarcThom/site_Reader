using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Rhino.Geometry;
using Rhino;
using Grasshopper.Kernel;

namespace siteReader.Methods
{
    public static class Utility
    {
        //UTILITY METHODS==============================================================================================
        //The methods contained within this class are methods that don't fit in the other method classes
        //=============================================================================================================

        /// <summary>
        /// converts LAS ushort array to RGB
        /// </summary>
        /// <param name="arrIN">las RGB field per point</param>
        /// <returns>RGB color</returns>
        public static Color ConvertRGB(ushort[] arrIN)
        {
            int r = Convert.ToInt32(arrIN[0]) / 256;
            int b = Convert.ToInt32(arrIN[1]) / 256;
            int g = Convert.ToInt32(arrIN[2]) / 256;

            return Color.FromArgb(r, b, g);
        }

        /// <summary>
        /// Zooms in on given bounding box
        /// </summary>
        /// <param name="box">bounding box surrounding geo to zoom in on</param>
        public static void ZoomGeo(BoundingBox box)
        {
            var views = RhinoDoc.ActiveDoc.Views.GetViewList(true, false);

            foreach (var view in views)
            {
                view.ActiveViewport.ZoomBoundingBox(box);
                view.Redraw();
            }
        }


        public static bool TestFile(string path, List<string> fileTypes, out string message)
        {
            message = null;

            if (!File.Exists(path))
            {
                message = "Cannot find file";
                return false;
            }

            if (!TestFileExt(path, fileTypes))
            {
                message = formatExtMsg(fileTypes);
                return false;
            }

            return true;

        }

        private static string formatExtMsg(List<string> exts)
        {
            string msg = "You must provide a valid ";

            if (exts.Count == 1)
            {
                return msg + exts[0] + " file.";
            }

            for (int i = 0; i < exts.Count; i++)
            {
                if (i < exts.Count - 1)
                {
                    msg += exts[i] + ", ";
                }
                else
                {
                    msg += "or " + exts[i] + " file.";
                }
            }
            return msg;
        }

        /// <summary>
        /// tests if file Path is .las or .laz
        /// </summary>
        /// <param name="path">the file Path to test</param>
        /// <returns>true if .las or .laz</returns>
        private static bool TestFileExt(string path, List<string>types)
        {
            string fileExt = Path.GetExtension(path);

            foreach (var type in types)
            {
                if (fileExt == type)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
