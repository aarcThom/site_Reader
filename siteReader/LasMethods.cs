using Aardvark.Base;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader
{
    public static class LasMethods
    {

        /// <summary>
        /// decodes and formats the .las VLRs and returns a dictionary of values if any
        /// </summary>
        /// <param name="vlr">the list of vlrs read by laszip</param>
        /// <returns>vlr key/value pairs</returns>
        public static Dictionary<string, string> VlrDict(LASzip.Net.laszip ptCloud, string curPath)
        {
            Dictionary<string, string> vlrDict = new Dictionary<string, string>();
            bool isCompressed;

            ptCloud.open_reader(curPath, out isCompressed);

            if (ptCloud.header.vlrs.Count > 0)
            {
                var vlr = ptCloud.header.vlrs;

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

                        int count = 1;
                        foreach (var f in frags)
                        {
                            f.Replace(',', ' ');
                            var keyVal = f.Split('[');
                            vlrDict.AddDup(keyVal[0], keyVal[1]);
                        }
                    }
                }
            }
            ptCloud.close_reader();
            return vlrDict;

        }


        public static Dictionary<string, float> HeaderDict(LASzip.Net.laszip ptCloud, string curPath)
        {
            Dictionary<string, float> headerDict = new Dictionary<string, float>();
            bool isCompressed;

            ptCloud.open_reader(curPath, out isCompressed);

            long ptCount;
            ptCloud.get_number_of_point(out ptCount);
            headerDict.Add("Number of Points", ptCount);

            headerDict.Add("Min X", ptCloud.header.min_x.DoubleToFloat());
            headerDict.Add("Min Y", ptCloud.header.min_y.DoubleToFloat());
            headerDict.Add("Min Z", ptCloud.header.min_z.DoubleToFloat());
            headerDict.Add("Max X", ptCloud.header.max_x.DoubleToFloat());
            headerDict.Add("Max Y", ptCloud.header.max_y.DoubleToFloat());
            headerDict.Add("Max Z", ptCloud.header.max_z.DoubleToFloat());

            ptCloud.close_reader();
            return headerDict;
        }

        public static List<int> GetPointIndices(float density)
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
                    indices = new List<int>() { 2, 6, 8};
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
                    indices = new List<int>() { 0, 1, 3, 4, 6, 7, 8};
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

    }
}
