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
    internal class FullPointCloud
    {
        //constructor
        public FullPointCloud(string path, Dictionary<string, string> vlr, Dictionary<string,float> header, Vector3d prevTranslation)
        {
            _path = path;
            _vlr = vlr;
            _header = header;

            _prevVect = prevTranslation;
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
        public void TranslateCloud(bool translate, GH_Component owner)
        {
            if (_prevVect != Vector3d.Zero && translate)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, 
                    "You can not re-translate a cloud with a provided translation vector.");
                return;
            }

            if (translate && _prevVect == Vector3d.Zero)
            {
                translationVect = new Vector3d(99, 99, 99);
                return;
            }

            if (!translate && _prevVect == Vector3d.Zero)
            {
                translationVect = new Vector3d(0, 0, 0);
                return;
            }

            if (_prevVect != Vector3d.Zero && !translate)
            {
                //clear messages
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Blank, "");
            }
        }

    }
}
