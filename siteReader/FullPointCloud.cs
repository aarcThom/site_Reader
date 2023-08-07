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
    public class LasPtCloud
    {
        //constructor
        public LasPtCloud(string path)
        {
            _path = path;
            _laszip = new LASzip.Net.laszip();

            _vlr = LasMethods.VlrDict(_laszip, _path);
            _header = LasMethods.HeaderDict(_laszip, _path);
            _translationVect = GetTranslation();

            _format = LasMethods.GetPointFormat(_laszip, _path);
        }

        //fields
        private LASzip.Net.laszip _laszip;
        private readonly string _path;

        private readonly Dictionary<string, string> _vlr;
        private readonly Dictionary<string, float> _header;

        private bool _isTranslated = false;
        private Vector3d _translationVect;
        private Vector3d _userProvidedVect = Vector3d.Zero;

        private byte _format;

        //properties
        public Vector3d translationVect => GetTranslation();
        public Vector3d userProvidedVect 
        {
            get { return _userProvidedVect; }
            set { _userProvidedVect = SetUserMove(value); }
        }
        public bool isTranslated => _isTranslated;

        public Dictionary<string, float> header => _header;
        public Dictionary<string, string> vlr => _vlr;
        public string path => _path;

        public PointCloud rhinoPtCloud { get; set; }

        public float maxDisplayDensity { get; set; }

        public byte pointFormat => _format;



        //methods
        

        private Vector3d GetTranslation()
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


                    if (LasMethods.ContainsRGB(_format))
                    {
                        ushort[] rgb = _laszip.point.rgb;
                        Color rgbColor = Utility.ConvertRGB(rgb);
                        rhinoPtCloud.Add(rPoint, rgbColor);
                    } 
                    else
                    {
                        rhinoPtCloud.Add(rPoint);
                    }
                    

                }

                ptIndex++;
                if (ptIndex == 10) ptIndex = 0;
            }
            _laszip.close_reader();
        }

        public void MovePointCloud()
        {
            Transform cloudTransform = Transform.Translation(_translationVect);
            rhinoPtCloud.Transform(cloudTransform);
            _translationVect *= -1;
            _isTranslated = !_isTranslated;
        }

        private Vector3d SetUserMove(Vector3d vectIn)
        {
            if (rhinoPtCloud != null)
            {

                if (vectIn != _userProvidedVect)
                {
                    //move the cloud back to its original position
                    Transform cloudTransform = Transform.Translation(_userProvidedVect * -1);
                    rhinoPtCloud.Transform(cloudTransform);
                }

                Transform cloudTransform2 = Transform.Translation(vectIn);
                rhinoPtCloud.Transform(cloudTransform2);
                return vectIn;
            }

            return _userProvidedVect;

        }


    }
}
