using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader
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
        /// <param name="dictIn"></param>
        /// <returns>list<string> for GH output</returns>
        public static List<string> DictToGhOut(Dictionary<string, string> dictIn)
        {
            List<string> ghOut = new List<string>();

            if (dictIn.Count == 0)
            {
                return new List<string>{"No VLRs found."};
            }


            foreach (string key in dictIn.Keys)
            {
                ghOut.Add($"{key} : {dictIn[key]}");
            }

            return ghOut;
        }
    }
}
