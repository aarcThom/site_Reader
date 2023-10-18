using Aardvark.Base;
using Rhino.Geometry;
using siteReader.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using g3;


namespace siteReader.Methods
{
    public static class LasMethods
    {
        //LAS METHODS=============================================================================================
        //The methods contained within this class are referenced only by the AsprCld class and the AsprParam class.
        //========================================================================================================

        /// <summary>
        /// decodes and formats the .las VLRs and returns a dictionary of values if any
        /// </summary>
        /// <param name="vlr">the list of vlrs read by Laszip</param>
        /// <returns>Vlr key/value pairs</returns>
        public static Dictionary<string, string> VlrDict(AsprCld cld)
        {
            var lz = cld.Laszip;
            var path = cld.Path;


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
        /// Retrieves pertinent information from the .las file's Header
        /// </summary>
        /// <param name="ptCloud"></param>
        /// <param name="curPath"></param>
        /// <returns></returns>
        public static Dictionary<string, float> HeaderDict(AsprCld cld)
        {
            var lz = cld.Laszip;
            var path = cld.Path;


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
            var lz = cld.Laszip;
            var path = cld.Path;

            byte format = 0;

            bool isCompressed;
            lz.open_reader(path, out isCompressed);

            format = lz.header.point_data_format;

            lz.close_reader();

            return format;
        }

        /// <summary>
        /// Tests if a given .las PointFormat contains an Rgb field
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

        /// <summary>
        /// The main Rhino Pointcloud and fields retrieval method.
        /// </summary>
        /// <param name="cld">The AsprCloud</param>
        /// <param name="density">The density factor between 0 and 1 to convert to a Rhino cloud.</param>
        /// <param name="crops">Any 3d meshes or Breps to crop the resultant Rhino cloud.</param>
        /// <param name="inside">If true, points inside the crop will be kept. If false, points outside the crop will be kept.</param>
        /// <returns>A rhino pointcloud along with field values for each point.</returns>
        public static (PointCloud, List<ushort>, List<Color>, List<ushort>, List<ushort>, List<ushort>, List<byte>, List<byte>) 
            GetPtCloud(AsprCld cld, float density, List<Mesh> crops, bool inside)
        {
            var lz = cld.Laszip;
            var path = cld.Path;
            var header = cld.Header;
            var format = cld.PointFormat;

            var intensity = new List<ushort>();
            var rgb = new List<Color>();
            var r = new List<ushort>();
            var g = new List<ushort>();
            var b = new List<ushort>();
            var classification = new List<byte>();
            var numReturns = new List<byte>();

            var hasRGB = ContainsRGB(format);

            var ptCloud = new PointCloud();
            bool isCompressed;
            lz.open_reader(path, out isCompressed);

            int pointCount = header["Number of Points"].ToInt();
            List<int> densityMask = GetMaskingPattern(density);

            //cropping stuff if needed
            DMeshAABBTree3 spatial = BuildSpatialTree(crops);

            var dir = new g3.Vector3d(0, 0, 1); //intersection vector if needed

            int maskIx = 0;
            for (int i = 0; i < pointCount; i++)
            {
                lz.read_point();
                var lsPt = lz.point;

                if (densityMask.Contains(maskIx))
                {
                    double[] coords = new double[3];
                    lz.get_coordinates(coords);
                    var rPoint = new Point3d(coords[0], coords[1], coords[2]);


                    //testing if point is inside
                    bool hit = spatial != null ? PtInsideCrop(spatial, rPoint, inside) : true;
                    
                    if (hit == true)
                    {
                        ptCloud.Add(rPoint);
                        intensity.Add(lsPt.intensity);
                        if (hasRGB)
                        {
                            rgb.Add(Utility.ConvertRGB(lsPt.rgb));
                            r.Add(Convert.ToUInt16(lsPt.rgb[0] / 256));
                            g.Add(Convert.ToUInt16(lsPt.rgb[1] / 256));
                            b.Add(Convert.ToUInt16(lsPt.rgb[2] / 256));
                        }
                        classification.Add(lsPt.classification);
                        numReturns.Add(lsPt.number_of_returns);
                    }
                }
                maskIx++;
                if (maskIx == 10) maskIx = 0;
            }
            lz.close_reader();


            return (ptCloud, intensity, rgb, r, g, b, classification, numReturns);
        }

        // SO IT TURNS OUT Rgb IS REALLY THE ONLY FORMAT SPECIFIC FIELD
        // IF YOU NEED IT FOR SOME OTHER FIELDS, RESURRECT THIS SWITCH METHOD
        /*
        private static bool ContainsField(string field, byte format)
        {
            var contains = false;

            switch (field)
            {
                case "Intensity":
                    var iList = new List<byte> {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
                    if (iList.Contains(format)) contains = true;
                    break;

                case "Rgb":
                    var rList = new List<byte> { 2, 3, 5, 7, 8, };
                    if (rList.Contains(format)) contains = true;
                    break;

                case "Classification":
                    var cList = new List<byte> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
                    if (cList.Contains(format)) contains = true;
                    break;

                case "NumReturns":
                    var nList = new List<byte> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
                    if (nList.Contains(format)) contains = true;
                    break;

            }
                return contains;
            }
        */

        /// <summary>
        /// Spatial tree from Rhino Geometry. For point inclusion testing.
        /// </summary>
        /// <param name="crops">Rhino meshes or Breps in.</param>
        /// <returns>A DMesh spatial tree.</returns>
        private static DMeshAABBTree3 BuildSpatialTree(List<Mesh> crops)
        {
            if (crops == null || crops.Count == 0) return null;

            var cropMesh = new Mesh();
            foreach (Mesh mesh in crops)
            {
                cropMesh.Append(mesh);
            }

            DMesh3 dMesh = Utility.MeshtoDMesh(cropMesh);
            DMeshAABBTree3 spatial = new DMeshAABBTree3(dMesh);
            spatial.Build();

            return spatial;
        }

        /// <summary>
        /// Test if a given point is inside or outside a crop.
        /// </summary>
        /// <param name="spatial">A DMesh spatial tree.</param>
        /// <param name="rPt">The point3D to be tested.</param>
        /// <param name="inside">If true, test if point is inside. If false, test if point is outside.</param>
        /// <returns>True if point meets provided inclusion.</returns>
        private static bool PtInsideCrop(DMeshAABBTree3 spatial, Point3d rPt, bool inside)
        {
            var dir = new g3.Vector3d(0, 0, 1);
            g3.Vector3d pt = new g3.Vector3d(rPt.X, rPt.Y, rPt.Z);
            g3.Ray3d ray = new g3.Ray3d(pt, dir);

            int hitCnt = spatial.FindAllHitTriangles(ray);

            if ((hitCnt % 2 != 0 && inside) || (hitCnt % 2 == 0 && !inside))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Converts .Las / .laz pointcloud to Rhino geometry for initial visualiztion.
        /// </summary>
        /// <param name="cld">An AsprCld class object.</param>
        /// <returns>A rhino geometry point cloud.</returns>
        public static PointCloud GetPreviewCld(AsprCld cld)
        {
            var lz = cld.Laszip;
            var path = cld.Path;
            var header = cld.Header;
            var format = cld.PointFormat;

            var ptCloud = new PointCloud();
            bool isCompressed;
            lz.open_reader(path, out isCompressed);

            int pointCount = header["Number of Points"].ToInt();
            int maskIx = 0;

            for (int i = 0; i < pointCount; i++)
            {
                lz.read_point();

                if (maskIx == 10)
                {

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

                maskIx++;
                if (maskIx == 20) maskIx = 0;
            }
            lz.close_reader();
            return ptCloud;
        }

        /// <summary>
        /// Remaps a byte fields (_classification, _numReturns) to the range 0-->1
        /// </summary>
        /// <param name="field">las field to process.</param>
        /// <returns>Normalized list of values.</returns>
        public static List<float> SetFieldToClassOrReturns(List<byte> field)
        {
            List<float> result = new List<float>();

            byte maxVal = field.Max();

            foreach (var val in field)
            {
                float mapped = maxVal == 0 ? 0 : (float)val / (float)maxVal;
                result.Add(mapped);
            }

            return result;
        }

        /// <summary>
        /// Remaps a byte fields (r,g,b, instensity) to the range 0-->1
        /// </summary>
        /// <param name="field">las field to process.</param>
        /// <returns>Normalized list of values.</returns>
        public static List<float> SetFieldToIntensOrClrChannel(List<ushort> field)
        {
            List<float> result = new List<float>();

            if (field.Count > 0)
            {
                ushort maxVal = field.Max();

                foreach (var val in field)
                {
                    float mapped = maxVal == 0 ? 0 : (float)val / (float)maxVal;
                    result.Add(mapped);
                }
            }
            return result;
        }

        /// <summary>
        /// Given a color range, convert a range of ushorts to corresponding color for visualization.
        /// </summary>
        /// <param name="itns">Cloud field values.</param>
        /// <param name="clrs">Color range.</param>
        /// <param name="ptCount">Number of points in range.</param>
        /// <returns>A list of colors corresponding to each point field value.</returns>
        public static List<Color> UShortToColor(List<ushort> itns, List<Color> clrs, int ptCount)
        {
            List<Color> result = new List<Color>();

            ushort maxVal = itns.Count > 0 ? itns.Max(): (ushort)0;

            if (itns.Count > 0 && maxVal > 0)
            {
                foreach (var val in itns)
                {
                    int mapped = 255 * val / maxVal;
                    result.Add(clrs[mapped]);
                }
            }
            else
            {
                for (int i = 0; i < ptCount; i++)
                {
                    result.Add(Color.Black);
                }
            }

            return result;
        }

        /// <summary>
        /// Given a color range, convert a range of bytes to corresponding color for visualization.
        /// </summary>
        /// <param name="itns">Cloud field values.</param>
        /// <param name="clrs">Color range.</param>
        /// <param name="ptCount">Number of points in range.</param>
        /// <returns>A list of colors corresponding to each point field value.</returns>
        public static List<Color> ByteToColor(List<byte> itns, List<Color> clrs, int ptCount)
        {
            List<Color> result = new List<Color>();

            byte maxVal = itns.Count > 0 ? itns.Max() : (byte)0;

            if (itns.Count > 0 && maxVal > 0)
            {
                foreach (var val in itns)
                {
                    int mapped = 255 * val / maxVal;
                    result.Add(clrs[mapped]);
                }
            }
            else 
            {
                for (int i = 0; i < ptCount; i++)
                {
                    result.Add(Color.Black);
                }
            }
            
            return result;
        }
    }
}
