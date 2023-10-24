using System;
using System.Collections.Generic;
using System.Data;
using Grasshopper.Kernel;
using Rhino.Geometry;
using siteReader.Components.Clouds;
using siteReader.Methods;
using DotSpatial.Data;

namespace siteReader.Components
{
    public class ImportShapeFile : SiteReaderBase
    {
        //FIELDS ======================================================================================================

        //PROPERTIES ==================================================================================================

        //CONSTRUCTORS ================================================================================================
        public ImportShapeFile()
          : base("Import Shape File", "ImpShp",
              "Import a shape File", "GIS")
        {

            //IconPath = "siteReader.Resources...";
        }

        //IO ==========================================================================================================
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("file Path", "Path", "Path to shape (.shp) file.", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("test", "t", "tt", GH_ParamAccess.list);
            pManager.AddTextParameter("att", "a", "atttt", GH_ParamAccess.list);
            pManager.AddTextParameter("rows", "rows", "yo", GH_ParamAccess.tree);
        }

        //SOLVE =======================================================================================================
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string currentPath = string.Empty;
            if (!DA.GetData(0, ref currentPath)) return;

            var fTypes = new List<string>() { ".shp" };
            if (!Utility.TestFile(currentPath, fTypes, out string msg))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, msg);
                return;
            }

            var sFile = Shapefile.OpenFile(currentPath);
            var sTable = sFile.Attributes.Table;
            var sRange = sFile.ShapeIndices;
            var sFeatures = sFile.Features;

            List<Polyline> outLines = new List<Polyline>();
            List<string> colNames = new List<string>();
            List<List<string>> rows = new List<List<string>>();

            foreach (var col in sTable.Columns)
            {
                var column = col as DataColumn;
                colNames.Add(column.ColumnName);
            }

            foreach (var r in sTable.Rows)
            {
                List<string> rowValues = new List<string>();
                var row = r as DataRow;

                for (int i = 0; i < sTable.Columns.Count; i++)
                {
                    rowValues.Add(row[i].ToString());
                }
                rows.Add(rowValues);
            }

            var rowTree = Utility.CreateStringTree(rows);

            foreach (var feature in sFeatures)
            {
                 
                 if (feature.FeatureType == FeatureType.Line)
                 {
                     var verts = feature.Geometry.Coordinates;

                     List<Point3d> rVerts = new List<Point3d>();
                     foreach (var coord in verts)
                     {
                         Point3d pt = new Point3d(coord.X, coord.Y, 0);
                         rVerts.Add(pt);
                     }
                     outLines.Add(new Polyline(rVerts));
                 }
            }

            DA.SetDataList(0, outLines);
            DA.SetDataList(1, colNames);
            DA.SetDataTree(2, rowTree);

            /*
             * Point 1 Point  
                Line 2 Line  
                Polygon 3 Polygon  
                MultiPoint 4 MultiPoint  

             */

        }

        //PREVIEW AND UI ==============================================================================================

        //UTILITY METHODS =============================================================================================

        //GUID ========================================================================================================
        public override Guid ComponentGuid => new Guid("F0284E1D-C6B6-4C98-8CEC-200F07B2D234");
    }
}