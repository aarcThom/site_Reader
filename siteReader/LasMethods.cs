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
                            var keyVal = f.Split('[');

                            if (!vlrDict.ContainsKey(keyVal[0]))
                            {
                                vlrDict.Add(keyVal[0], keyVal[1].Replace(',', ' '));
                                count = 1;
                            }
                            else
                            {
                                count++;
                                vlrDict.Add(keyVal[0] + "_" + count, keyVal[1].Replace(',', ' '));
                            }

                        }
                    }
                }
            }
            ptCloud.close_reader();
            return vlrDict;

        }

    }
}
