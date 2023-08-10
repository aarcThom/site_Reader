using Aardvark.Base;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using siteReader.Methods;

namespace siteReader.Params
{
    public class AsprCld : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject
    {
        //constructors
        public AsprCld(string path)
        {
            _path = path;
            _laszip = new LASzip.Net.laszip();

            _vlr = LasMethods.VlrDict(this);
            _header = LasMethods.HeaderDict(this);
            _format = LasMethods.GetPointFormat(this);

            _ptCloud = new PointCloud();
            this.m_value = _ptCloud; // the geometry for the grasshopper geometricGoo
        }

        public AsprCld(AsprCld cld)
        {
            _path = cld.path;
            _laszip = cld.laszip;

            _vlr = cld.vlr;
            _header = cld.header;
            _format = cld.pointFormat;

            _ptCloud = cld.ptCloud;
            this.m_value = _ptCloud;
        }

        public AsprCld(PointCloud transformedCloud, AsprCld cld)
        {
            _path = cld.path;
            _laszip = cld.laszip;

            _vlr = cld.vlr;
            _header = cld.header;
            _format = cld.pointFormat;

            _ptCloud = transformedCloud;
            this.m_value = _ptCloud;
        }

        public AsprCld(PointCloud ptCld)
        {
            _userRefCld = true;
            _format = 77; //reserve this format # for user ref'd clouds

            _ptCloud = ptCld;
            this.m_value = _ptCloud;
        }

        public AsprCld()
        {
            _userRefCld = true;
            _format = 77; //reserve this format # for user ref'd clouds

            _ptCloud = new PointCloud();
            this.m_value = _ptCloud;
        }

        //fields
        private bool _userRefCld = false; //maybe delete this it might cause issues down the road to have user ref'd clouds without ASPR data
        private readonly string _path;
        private readonly Dictionary<string, float> _header;
        private readonly Dictionary<string, string> _vlr;
        private byte _format;

        private LASzip.Net.laszip _laszip;

        private PointCloud _ptCloud;

        //PROPERTIES---------------------------------------------------------

        //ASPR / .las properties

        public bool userRefCld => _userRefCld; // is the cloud a point cloud referenced from Rhino?
        public string path => _path;
        public Dictionary<string, float> header => _header;
        public Dictionary<string, string> vlr => _vlr;
        public byte pointFormat => _format;
        
        
        
        //laszip
        public LASzip.Net.laszip laszip => _laszip;

        //geometry properties
        public PointCloud ptCloud => _ptCloud;

        //grasshopper options
        public float displayDensity { get; set; }




        //INTERFACE METHODS--------------------------------------------------

        //IGH_PreviewData METHODS
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            // No meshes
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawPointCloud(this.m_value, args.Thickness);
        }

        public BoundingBox ClippingBox
        {
            get
            {
                return this.m_value.GetBoundingBox(true);
            }
        }

        //IGH_BakeAwareObject METHODS
        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, new ObjectAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            obj_ids.Add(doc.Objects.AddPointCloud(m_value, att));
        }

        public bool IsBakeCapable => ptCloud != null;


        //GH_GOO METHODS

        public override string ToString()
        {
            return $"Point Cloud with {m_value.Count} points.";
        }

        public override BoundingBox GetBoundingBox(Transform xform)
        {
            return this.m_value.GetBoundingBox(xform);
        }

        public override IGH_GeometricGoo Morph(SpaceMorph xmorph)
        {
            var duplicate = this.m_value.Duplicate();
            xmorph.Morph(duplicate);
            return new AsprCld((PointCloud)duplicate, this);
        }

        public override BoundingBox Boundingbox => this.m_value.GetBoundingBox(true);

        public override string TypeDescription => "A point cloud linked with ASPR info";

        public override IGH_GeometricGoo DuplicateGeometry()
        {
            var duplicate = this.m_value.Duplicate();
            return new AsprCld((PointCloud)duplicate, this);

        }

        public override string TypeName => "ASPR PointCloud";

        public override IGH_GeometricGoo Transform(Transform xform)
        {
            var duplicate = this.m_value.Duplicate();
            duplicate.Transform(xform);
            return new AsprCld((PointCloud)duplicate, this);
        }

        //CLOUD METHODS
        public void GetPointCloud()
        {
            _ptCloud = LasMethods.GetCoordinates(this);
        }

    }
}
