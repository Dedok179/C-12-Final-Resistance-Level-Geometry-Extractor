using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace C12_Obj
{
    class Obj
    {
        public Obj()
        {
            name = "";
            matFile = "";
            vertices = new List<string>();
            normals = new List<string>();
            uvs = new List<string>();
            faces = new List<string>();
        }

        public List<string> GetObj()
        {
            result = new List<string>();

            result.Add("C12 Tool");
            result.Add(matFile);
            result.Add($"o {name}");
            result.AddRange(vertices);
            result.AddRange(uvs);
            result.AddRange(faces);

            return result;
        }

        public string name;
        public string matFile;
        public List<string> vertices;
        public List<string> normals;
        public List<string> uvs;
        public List<string> faces;
        public List<string> result;
    }

    class Mat
    {

    }
}

