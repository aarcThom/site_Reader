using System.Collections.Generic;
using System.Linq;

namespace siteReader.Methods
{
    public static class Extension
    {
        //EXTENSION METHODS============================================================================================
        //Extending built in types.
        //=============================================================================================================

        /// <summary>
        /// Appends '_#' to a duplicate key in a dictionary where # is the existing # of keys that contain input key
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
