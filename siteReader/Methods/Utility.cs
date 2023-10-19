using System;
using System.Drawing;
using System.IO;
using Rhino.Geometry;
using Rhino;

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

        /// <summary>
        /// tests if file Path is .las or .laz
        /// </summary>
        /// <param name="path">the file Path to test</param>
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
    }
}
