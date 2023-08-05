using Aardvark.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader
{
    public class FullPointCloud
    {
        //constructor
        public FullPointCloud(string path, float viewDensity)
        {
            _path = path;
            _laszip = new LASzip.Net.laszip();

            _vlr = LasMethods.VlrDict(_laszip, _path);
            _header = LasMethods.HeaderDict(_laszip, _path);

            maxDisplayDensity = viewDensity;
            translationVect = GetTranslation();

            format = LasMethods.GetPointFormat(_laszip, _path);
        }

        //fields
        private LASzip.Net.laszip _laszip;
        private readonly string _path;

        private readonly Dictionary<string, string> _vlr;
        private readonly Dictionary<string, float> _header;

        public bool isTranslated;
        public byte format;

        //properties
        public Vector3d translationVect { get; set; }
        public Dictionary<string, float> header => _header;
        public Dictionary<string, string> vlr => _vlr;
        public string path => _path;

        public PointCloud rhinoPtCloud { get; set; }

        public float maxDisplayDensity { get; set; }



        //methods
        

        public Vector3d GetTranslation()
        {
            float x = _header["Min X"];
            float y = _header["Min Y"];

            return new Vector3d(-x, -y, 0);
        }

        public void GetPointCloud()
        {

            rhinoPtCloud = new PointCloud();
            bool isCompressed;
            _laszip.open_reader(_path, out isCompressed);

            int pointCount = (_header["Number of Points"]).ToInt();
            List<int> ptIndices = LasMethods.GetPointIndices(maxDisplayDensity);

            int ptIndex = 0;

            for (int i  = 0; i < pointCount; i++)
            {
                _laszip.read_point();

                if (ptIndices.Contains(ptIndex))
                {
                    double[] coords = new double[3];
                    _laszip.get_coordinates(coords);
                    var rPoint = new Point3d(coords[0], coords[1], coords[2]);


                    if (LasMethods.ContainsRGB(format))
                    {
                        ushort[] rgb = _laszip.point.rgb;
                        Color rgbColor = Utility.ConvertRGB(rgb);
                        rhinoPtCloud.Add(rPoint, rgbColor);
                    } 
                    else
                    {
                        rhinoPtCloud.Add(rPoint, Color.Blue);
                    }
                    

                }

                ptIndex++;
                if (ptIndex == 10) ptIndex = 0;
            }
            _laszip.close_reader();
        }

        public void MovePointCloud()
        {
            Transform cloudTransform = Transform.Translation(translationVect);
            rhinoPtCloud.Transform(cloudTransform);
            translationVect *= -1;
        }

    }
}
