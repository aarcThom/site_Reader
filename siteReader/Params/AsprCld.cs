﻿using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Methods;
using System;
using System.Collections.Generic;
using System.Drawing;
using Rhino.DocObjects;
using Rhino;
using Aardvark.Base;
using Rhino.Commands;
using System.Linq;
using Aardvark.Base.Sorting;

namespace siteReader.Params
{
    public class AsprCld : GH_GeometricGoo<PointCloud>, IGH_PreviewData, IGH_BakeAwareObject

    {
        //FIELDS--------------------------------------------------------------

        //aspr / .las properties
        private readonly string _path;
        private readonly Dictionary<string, float> _header;
        private readonly Dictionary<string, string> _vlr;
        private byte _format;

        private List<ushort> _intensity;
        private List<Color> _rgb;
        private List<ushort> _r;
        private List<ushort> _g;
        private List<ushort> _b;
        private List<byte> _classification;
        private List<byte> _numReturns;

        private List<float> _currentField; //used for cropping by value - floats between 0 and 1

        //Laszip
        private LASzip.Net.laszip _laszip;

        //geometry
        private PointCloud _ptCloud;

        //PROPERTIES---------------------------------------------------------

        //aspr / .las properties
        public string Path => _path;
        public Dictionary<string, float> Header => _header;
        public Dictionary<string, string> Vlr => _vlr;
        public byte PointFormat => _format;

        public List<ushort> Intensity => _intensity;
        public List<Color> Rgb => _rgb;
        public List<ushort> R => _r;
        public List<ushort> G => _g;
        public List<ushort> B => _b;

        public List<byte> Classification => _classification;
        public List<byte> NumReturns => _numReturns;

        public List<float> CurrentField => _currentField;

        //Laszip
        public LASzip.Net.laszip Laszip => _laszip;

        //geometry
        public PointCloud PtCloud => _ptCloud;

        //needed by components
        public float DisplayDensity { get; set; }


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
            _path = cld.Path;
            _laszip = cld.Laszip;

            _vlr = cld.Vlr.Copy();
            _header = cld.Header.Copy();
            _format = cld.PointFormat;


            _intensity = cld.Intensity.Copy();
            _rgb = cld.Rgb.Copy();
            _r = cld.R.Copy();
            _g = cld.G.Copy();
            _b = cld.B.Copy();
            _classification = cld.Classification.Copy();
            _numReturns = cld.NumReturns.Copy();

            _ptCloud = new PointCloud(cld.PtCloud);
            this.m_value = _ptCloud;
        }

        public AsprCld(PointCloud transformedCloud, AsprCld cld)
        {
            _path = cld.Path;
            _laszip = cld.Laszip;

            _vlr = cld.Vlr.Copy();
            _header = cld.Header.Copy();
            _format = cld.PointFormat;

            _intensity = cld.Intensity.Copy();
            _rgb = cld.Rgb.Copy();
            _r = cld.R.Copy();
            _g = cld.G.Copy();
            _b = cld.B.Copy();
            _classification = cld.Classification.Copy();
            _numReturns = cld.NumReturns.Copy();

            _ptCloud = transformedCloud;
            this.m_value = _ptCloud;
        }

        public AsprCld()
        {
            //needed for Rhino ref outs
        }


        //INTERFACE METHODS---------------------------------------------------------------------------------------

        //IGH_PreviewData METHODS
        public void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            // No meshes
        }

        public void DrawViewportWires(GH_PreviewWireArgs args)
        {
            args.Pipeline.DrawPointCloud(this.m_value, args.Thickness); 
        }

        public BoundingBox ClippingBox => this.m_value.GetBoundingBox(true);

        //IGH_BakeAwareObject METHODS
        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, new ObjectAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            obj_ids.Add(doc.Objects.AddPointCloud(m_value, att));
        }

        public bool IsBakeCapable => PtCloud != null;


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
        public void GetPointCloud(List<Mesh> cropShapes, bool inside)
        {
            (_ptCloud, _intensity, _rgb, _r, _g, _b, _classification, _numReturns) = 
                LasMethods.GetPtCloud(this, DisplayDensity, cropShapes, inside);
        }

        public void GetPreview()
        {
            _ptCloud = LasMethods.GetPreviewCld(this);
        }

        public void ApplyColors(List<Color> colors)
        {
            int colorCount = 0;
            foreach(var pt in _ptCloud)
            {
                pt.Color = colors[colorCount];
                colorCount++;
            }
        }

        public void SetFieldToClassOrReturns(List<byte> field)
        {
            List<float> result = new List<float>();

            byte maxVal = field.Max();

            foreach (var val in field)
            {
                float mapped = maxVal == 0 ? 0 : (float)val / (float)maxVal;
                result.Add(mapped);
            }

            _currentField = result;
        }

        public void SetFieldToIntensOrClrChannel()
        {
            List<float> result = new List<float>();

            ushort maxVal = _intensity.Max();

            foreach (var val in _intensity)
            {
                float mapped = maxVal == 0 ? 0 : (float)val / (float)maxVal;
                result.Add(mapped);
            }

            _currentField = result;
        }

        public void SetFieldToRGB()
        {
            //NOTE: I NEED TO FIGURE THIS OUT
            // I'm thinking it will be based on color distance from a preset rainbow slider

            _currentField = Enumerable.Range(0, _rgb.Count).Select(val => (float)val / (float)_rgb.Count).ToList();
        }
    }
}
