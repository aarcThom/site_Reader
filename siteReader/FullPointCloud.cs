using Aardvark.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace siteReader
{
    public class FullPointCloud
    {
        //constructor
        public FullPointCloud(string path)
        {
            _path = path;
            _laszip = new LASzip.Net.laszip();

            _vlr = LasMethods.VlrDict(_laszip, _path);
            _header = LasMethods.HeaderDict(_laszip, _path);
        }

        //fields
        private LASzip.Net.laszip _laszip;
        private readonly string _path;

        private readonly Dictionary<string, string> _vlr;
        private readonly Dictionary<string, float> _header;

        //properties
        public Vector3d translationVect { get; set; }
        public Dictionary<string, float> header => _header;
        public Dictionary<string, string> vlr => _vlr;
        public string path => _path;

        public PointCloud rhinoPtCloud { get; set; }


        //methods
        

        public Vector3d GetTranslation()
        {
            float x = _header["Min X"];
            float y = _header["Min Y"];

            return new Vector3d(-x, -y, 0);
        }

        public void GetPointCloud()
        {
            PointCloud rPtCloud = new PointCloud();
            bool isCompressed;
            _laszip.open_reader(_path, out isCompressed);

            for (int i  = 0; i < _header["Number of Points"]; i++)
            {
                double[] coords = new double[3];
                _laszip.read_point();
                _laszip.get_coordinates(coords);
                var rPoint = new Point3d(coords[0], coords[1], coords[2]);
                
                /*
                 * need to fix this
                var rgb = _laszip.point.rgb;

                rPtCloud.Add(rPoint, Color.FromArgb(rgb[0], rgb[1], rgb[2]));
                */
            }


            _laszip.close_reader();
            rhinoPtCloud = rPtCloud;
        }

    }
}
