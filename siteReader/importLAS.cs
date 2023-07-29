using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace siteReader
{
    public class importLAS : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public importLAS()
          : base("import LAS", "impLAS",
            "Import a LAS file",
            "AARC", "siteReader")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file path", "path", "Path to LAS or LAZ file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Output", "out", "Component messages. Use to check for errors.",
                GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input variables
            string currentPath = String.Empty;

            //output variables
            string outMsg = "";

            // Is input empty?
            if (!DA.GetData(0, ref currentPath)) return;

            // Test if file exists
            if (!File.Exists(currentPath))
            {
                outMsg = "Cannot find file.";
                DA.SetData(0, outMsg);
                return;
            }

            bool isCompressed;
            var impLas = new LASzip.Net.laszip();
            impLas.open_reader(currentPath, out isCompressed);
            long numPts;
            impLas.get_number_of_point(out numPts);

            var vlr = impLas.header.vlrs;


            Dictionary<string, string> vlrDict = new Dictionary<string, string>();
            using (StreamWriter writer = new StreamWriter(@"C:\Users\rober\Desktop\output.txt"))
            {


                foreach (var v in vlr)
                {
                    var line = System.Text.Encoding.ASCII.GetString(v.data);
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
                                vlrDict.Add(keyVal[0] + "_" +  count, keyVal[1].Replace(',', ' '));
                            }

                        }

                        foreach (var pair in vlrDict)
                        {
                            writer.WriteLine(pair.Key + " : "+pair.Value);
                        }
                    }


                }
            }

            var encode = impLas.header.global_encoding;

            DA.SetData(0, encode.ToString());

        }

        public static List<string> SplitWkt(string wkt)
        {
            List<string> result = new List<string>();
            return result;
        }




        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("FF31124B-CEA9-474D-9C1A-FB5132D77D74");


    }
}