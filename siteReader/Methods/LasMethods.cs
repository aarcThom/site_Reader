using Aardvark.Base;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Grasshopper.Kernel.Types.Transforms;
using System.IO;
using Rhino;
using System.Drawing.Text;

namespace siteReader.Methods
{
    public static class LasMethods
    {

        /// <summary>
        /// decodes and formats the .las VLRs and returns a dictionary of values if any
        /// </summary>
        /// <param name="vlr">the list of vlrs read by laszip</param>
        /// <returns>vlr key/value pairs</returns>
        public static Dictionary<string, string> VlrDict(AsprCld cld)
        {
            var lz = cld.laszip;
            var path = cld.path;


            Dictionary<string, string> vlrDict = new Dictionary<string, string>();
            bool isCompressed;

            lz.open_reader(path, out isCompressed);

            if (lz.header.vlrs.Count > 0)
            {
                var vlr = lz.header.vlrs;

                foreach (var v in vlr)
                {
                    var line = Encoding.ASCII.GetString(v.data);
                    var frags = line.Split(',').ToList();

                    if (frags.Count > 1)
                    {
                        for (int i = frags.Count - 1; i >= 0; i--)
                        {
                            frags[i] = frags[i].Replace("]", string.Empty);
                            frags[i] = frags[i].Replace("\"", string.Empty);

                            if (!frags[i].Contains("[") && i != 0)
                            {
                                frags[i - 1] += "," + frags[i];
                                frags.RemoveAt(i);
                            }
                        }
                        frags.Sort();

                        foreach (var f in frags)
                        {
                            f.Replace(',', ' ');
                            var keyVal = f.Split('[');
                            vlrDict.AddDup(keyVal[0], keyVal[1]);
                        }
                    }
                }
            }
            lz.close_reader();
            return vlrDict;

        }

        /// <summary>
        /// Retrieves pertinent information from the .las file's header
        /// </summary>
        /// <param name="ptCloud"></param>
        /// <param name="curPath"></param>
        /// <returns></returns>
        public static Dictionary<string, float> HeaderDict(AsprCld cld)
        {
            var lz = cld.laszip;
            var path = cld.path;


            Dictionary<string, float> headerDict = new Dictionary<string, float>();
            bool isCompressed;

            lz.open_reader(path, out isCompressed);

            long ptCount;
            lz.get_number_of_point(out ptCount);
            headerDict.Add("Number of Points", (float)ptCount);


            headerDict.Add("Min X", lz.header.min_x.DoubleToFloat());
            headerDict.Add("Min Y", lz.header.min_y.DoubleToFloat());
            headerDict.Add("Min Z", lz.header.min_z.DoubleToFloat());
            headerDict.Add("Max X", lz.header.max_x.DoubleToFloat());
            headerDict.Add("Max Y", lz.header.max_y.DoubleToFloat());
            headerDict.Add("Max Z", lz.header.max_z.DoubleToFloat());
            headerDict.Add("Point Format", (float)lz.header.point_data_format);

            lz.close_reader();
            return headerDict;
        }
        /// <summary>
        /// Given a point density between 0.1 and 1, return a pattern of indices used to pick points from a .las cloud
        /// </summary>
        /// <param name="density"></param>
        /// <returns></returns>
        public static List<int> GetMaskingPattern(float density)
        {
            List<int> indices = new List<int>();

            switch (density)
            {
                case 0.1f:
                    indices = new List<int>() { 5 };
                    break;
                case 0.2f:
                    indices = new List<int>() { 3, 7 };
                    break;
                case 0.3f:
                    indices = new List<int>() { 2, 6, 8 };
                    break;
                case 0.4f:
                    indices = new List<int>() { 0, 3, 6, 9 };
                    break;
                case 0.5f:
                    indices = new List<int>() { 1, 3, 5, 7, 9 };
                    break;
                case 0.6f:
                    indices = new List<int>() { 0, 2, 3, 5, 6, 8 };
                    break;
                case 0.7f:
                    indices = new List<int>() { 0, 1, 3, 4, 6, 7, 8 };
                    break;
                case 0.8f:
                    indices = new List<int>() { 0, 1, 3, 4, 5, 6, 8, 9 };
                    break;
                case 0.9f:
                    indices = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                    break;
                case 1f:
                    indices = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                    break;
            }


            return indices;
        }
        /// <summary>
        /// Get the .las cloud point format that determines what fields are available for the cloud
        /// </summary>
        /// <param name="ptCloud"></param>
        /// <param name="curPath"></param>
        /// <returns></returns>
        public static byte GetPointFormat(AsprCld cld)
        {
            var lz = cld.laszip;
            var path = cld.path;

            byte format = 0;

            bool isCompressed;
            lz.open_reader(path, out isCompressed);

            format = lz.header.point_data_format;

            lz.close_reader();

            return format;
        }

        /// <summary>
        /// Tests if a given .las pointFormat contains an RGB field
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool ContainsRGB(byte format)
        {
            byte[] RGBformats = new byte[] { 2, 3, 5, 7, 8, 10 };

            if (RGBformats.Contains(format))
            {
                return true;
            }
            return false;
        }

        public static (PointCloud, List<bool>) GetInitialPts(AsprCld cld)
        {
            var lz = cld.laszip;
            var path = cld.path;
            var header = cld.header;
            var density = cld.displayDensity;
            var format = cld.pointFormat;

            List<bool> ptMask = new List<bool>();

            var ptCloud = new PointCloud();
            bool isCompressed;
            lz.open_reader(path, out isCompressed);

            int pointCount = header["Number of Points"].ToInt();
            List<int> densityMask = GetMaskingPattern(density);
            int maskIx = 0;

            

            for (int i = 0; i < pointCount; i++)
            {
                lz.read_point();

                if (densityMask.Contains(maskIx))
                {
                    ptMask.Add(true);

                    double[] coords = new double[3];
                    lz.get_coordinates(coords);
                    var rPoint = new Point3d(coords[0], coords[1], coords[2]);


                    if (ContainsRGB(format))
                    {
                        ushort[] rgb = lz.point.rgb;
                        Color rgbColor = Utility.ConvertRGB(rgb);
                        ptCloud.Add(rPoint, rgbColor);
                    }
                    else
                    {
                        ptCloud.Add(rPoint);
                    }
                }
                else
                {
                    ptMask.Add(false);
                }

                maskIx++;
                if (maskIx == 10) maskIx = 0;
            }
            lz.close_reader();


            return (ptCloud, ptMask);
        }

        public static List<int> GetIntensity(AsprCld cld)
        {
            var lz = cld.laszip;
            var path = cld.path;
            var ptMask = cld.ptMask;
            var header = cld.header;
            var ptCld = cld.ptCloud;

            float ratio = 255 / 65535;


            var intenseList = new List<int>();
            bool isCompressed;
            lz.open_reader(path, out isCompressed);

            int pointCount = header["Number of Points"].ToInt();


            for (int i = 0; i < pointCount; i++)
            {
                lz.read_point();

                if (ptMask[i])
                {
                    var intensity = lz.point.intensity;
                    intenseList.Add(Convert.ToInt32(intensity * ratio));
                }
                
            }
            lz.close_reader();
            return intenseList;

        }
    }
}
