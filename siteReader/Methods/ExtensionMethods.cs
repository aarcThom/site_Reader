using Aardvark.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader.Methods
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Appeands '_#' to a duplicate key in a dictionary where # is the existing number of keys that contain the input key
        /// </summary>
        /// <param name="baseDictionary"></param>
        /// <param name="dKey"></param>
        /// <param name="dVal"></param>
        public static void AddDup(this Dictionary<string, string> baseDictionary, string dKey, string dVal)
        {
            if (baseDictionary.ContainsKey(dKey))
            {
                // get the count of dKey substring
                int subStringCount = baseDictionary.Keys.Count(kys => kys.Contains(dKey));

                string newKey = $"{dKey}_{subStringCount}";
                baseDictionary.Add(newKey, dVal);

            }
            else
            {
                baseDictionary.Add(dKey, dVal);
            }
        }
    }
}
