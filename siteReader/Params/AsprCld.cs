using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Methods;
using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.DocObjects;
using Rhino;
using Aardvark.Base;

namespace siteReader.Params
{
    public class AsprCld : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject

    {
        //CONSTRUCTORS--------------------------------------------------------
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

            _vlr = cld.vlr.Copy();
            _header = cld.header.Copy();
            _format = cld.pointFormat;

            _intensity = cld.intensity.Copy();
            _rgb = cld.rgb.Copy();
            _classification = cld.classification.Copy();
            _numReturns = cld.numReturns.Copy();

            _ptCloud = new PointCloud(cld.ptCloud);
            this.m_value = _ptCloud;
        }

        public AsprCld(PointCloud transformedCloud, AsprCld cld)
        {
            _path = cld.path;
            _laszip = cld.laszip;

            _vlr = cld.vlr.Copy();
            _header = cld.header.Copy();
            _format = cld.pointFormat;

            _ptCloud = transformedCloud;
            this.m_value = _ptCloud;
        }

        public AsprCld()
        {
            //needed for Rhino ref outs
        }


        //FIELDS--------------------------------------------------------------

        //aspr / .las properties
        private readonly string _path;
        private readonly Dictionary<string, float> _header;
        private readonly Dictionary<string, string> _vlr;
        private byte _format;

        private List<ushort> _intensity;
        private List<Color> _rgb;
        private List<byte> _classification;
        private List<byte> _numReturns;

        //laszip
        private LASzip.Net.laszip _laszip;

        //geometry
        private PointCloud _ptCloud;

        //PROPERTIES---------------------------------------------------------

        //aspr / .las properties
        public string path => _path;
        public Dictionary<string, float> header => _header;
        public Dictionary<string, string> vlr => _vlr;
        public byte pointFormat => _format;

        public List<ushort> intensity => _intensity;
        public List<Color> rgb => _rgb;
        public List<byte> classification => _classification;
        public List<byte> numReturns => _numReturns;

        //laszip
        public LASzip.Net.laszip laszip => _laszip;

        //geometry
        public PointCloud ptCloud => _ptCloud;

        //needed by components
        public float displayDensity { get; set; } 


        //INTERFACE METHODS---------------------------------------------------------------------------------------

        //IGH_PreviewData METHODS
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            // No meshes
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            //removed because it's faster to render in a given component
            // args.Pipeline.DrawPointCloud(this.m_value, args.Thickness); 
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

        //CLOUD METHODS----------------------------------------------------
        public void GetPointCloud(List<Mesh> cropShapes = null, bool inside = false)
        {
            (_ptCloud, _intensity, _rgb, _classification, _numReturns) = LasMethods.GetPtCloud(this, displayDensity, cropShapes, inside);
        }

        public void GetPreview()
        {
            _ptCloud = LasMethods.GetPreviewCld(this);
        }
    }
}
