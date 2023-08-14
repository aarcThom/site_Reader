using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using Rhino.Geometry;

namespace siteReader.Methods
{
    public static class Utility
    {
        /// <summary>
        /// tests if file path is .las or .laz
        /// </summary>
        /// <param name="path">the file path to test</param>
        /// <returns>true if .las or .laz</returns>
        public static bool TestLasExt(string path)
        {
            string fileExt = Path.GetExtension(path);

            if (fileExt == ".las" || fileExt == ".laz")
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// formats VLR dictionary for GH textual output
        /// </summary>
        /// <param name="stringDict"></param>
        /// <returns>list<string> for GH output</returns>
        public static List<string> StringDictGhOut(Dictionary<string, string> stringDict)
        {
            List<string> ghOut = new List<string>();

            if (stringDict.Count == 0)
            {
                return new List<string> { "No VLRs found." };
            }


            foreach (string key in stringDict.Keys)
            {
                ghOut.Add($"{key} : {stringDict[key]}");
            }

            return ghOut;
        }

        /// <summary>
        /// Formats header dictionary for GH Textural output
        /// </summary>
        /// <param name="floatDict"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static List<string> FloatDictGhOut(Dictionary<string, float> floatDict, GH_Component owner)
        {
            List<string> ghOut = new List<string>();

            if (floatDict.Count == 0)
            {
                owner.AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "LAS header not found. The LAS spec. needs a header. " +
                    "Double check your data source as this will probably cause errors down the road.");
                return ghOut;
            }


            foreach (string key in floatDict.Keys)
            {
                ghOut.Add($"{key} : {floatDict[key]}");
            }

            return ghOut;
        }

        public static Color ConvertRGB(ushort[] arrIN)
        {
            int r = Convert.ToInt32(arrIN[0]) / 256;
            int b = Convert.ToInt32(arrIN[1]) / 256;
            int g = Convert.ToInt32(arrIN[2]) / 256;

            return Color.FromArgb(r, b, g);
        }


        public static Mesh TriangulateMesh(Mesh inMesh)
        {
            foreach (var face in inMesh.Faces)
            {
                if (face.IsQuad)
                {
                    double dist1 = inMesh.Vertices[face.A].DistanceTo
                        (inMesh.Vertices[face.C]);
                    double dist2 = inMesh.Vertices[face.B].DistanceTo
                        (inMesh.Vertices[face.D]);

                    if (dist1 > dist2)
                    {
                        inMesh.Faces.AddFace(face.A, face.B, face.D);
                        inMesh.Faces.AddFace(face.B, face.C, face.D);
                    }
                    else
                    {
                        inMesh.Faces.AddFace(face.A, face.B, face.C);
                        inMesh.Faces.AddFace(face.A, face.C, face.D);
                    }
                }
            }

            var newFaces = new List<MeshFace>();
            foreach (var fc in inMesh.Faces)
            {
                if (fc.IsTriangle) newFaces.Add(fc);
            }

            inMesh.Faces.Clear();
            inMesh.Faces.AddFaces(newFaces);

            return inMesh;
        }

        public static List<g3.Index3i> GetFaces(Mesh mesh)
        {
            var triList = new List<g3.Index3i>();

            foreach (var face in mesh.Faces)
            {
                var tri = new g3.Index3i(face.A, face.B, face.C);
                triList.Add(tri);
            }

            return triList;
        }

        public static List<g3.Vector3f> GetVertices(Mesh mesh)
        {
            var vertices = new List<g3.Vector3f>();

            foreach (var vert in mesh.Vertices)
            {
                var coords = new g3.Vector3f(vert.X, vert.Y, vert.Z);
                vertices.Add(coords);
            }

            return vertices;
        }

        public static List<g3.Vector3f> GetNormals(Mesh mesh)
        {
            var normals = new List<g3.Vector3f>();

            foreach (var norm in mesh.Normals)
            {
                var normal = new g3.Vector3f(norm.X, norm.Y, norm.Z);
                normals.Add(normal);
            }

            return normals;
        }


        public static DMesh3 MeshtoDMesh(Mesh rMesh)
        {
            Mesh triMesh = TriangulateMesh(rMesh);

            var faces = GetFaces(triMesh);
            var vertices = GetVertices(triMesh);
            var normals = GetNormals(triMesh);

            DMesh3 dMesh = new DMesh3(MeshComponents.VertexNormals);
            for (int i = 0; i < vertices.Count; i++)
            {
                dMesh.AppendVertex(new NewVertexInfo(vertices[i], normals[i]));
            }
            foreach(var tri in faces)
            {
                dMesh.AppendTriangle(tri);
            }



            return dMesh;
        }
    }
}
