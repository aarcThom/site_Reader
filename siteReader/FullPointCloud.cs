using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
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

        }

    }
}
