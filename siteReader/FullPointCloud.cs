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
        public FullPointCloud(string path, Dictionary<string, string> vlr, Dictionary<string,float> header)
        {
            _path = path;
            _vlr = vlr;
            _header = header;
            translationVect = _prevVect;
        }

        //fields
        private readonly string _path;
        private readonly Dictionary<string, string> _vlr;
        private readonly Dictionary<string, float> _header;
        private readonly Vector3d _prevVect; 

        //properties
        public Vector3d translationVect { get; set; }

        //methods
        

        public Vector3d GetTranslation()
        {
            float x = _header["Min X"];
            float y = _header["Min Y"];

            return new Vector3d(-x, -y, 0);
        }

    }
}
