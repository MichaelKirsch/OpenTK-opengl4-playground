﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace YEET
{
    public class OBJLoader
    {
        private int VAO, VBO;
        private string current_material;

        public Matrix4 ModelMatrix=new Matrix4();
        
        private List<Vector3> Vertices,Normals,ColorPerIndex,FinalVertexArray;
        private List<int> Indices, NormalIndices;
        
        private Dictionary<string, Vector3> Materials;
        private Dictionary<string,int> MaterialsIndices;
        public Vector3 Position;
        public ShaderLoader Loader;
        
        
        /// <summary>
        /// </summary>
        /// <param name="path">Path from the Models Directory</param>
        public OBJLoader(string path,ShaderLoader loader)
        {
            Util.StopWatchMilliseconds watch = new Util.StopWatchMilliseconds();
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            ColorPerIndex = new List<Vector3>();
            NormalIndices = new List<int>();
            Indices = new List<int>();
            MaterialsIndices = new Dictionary<string, int>();
            Materials = new Dictionary<string, Vector3>();
            Loader = loader;
            Matrix4.CreateTranslation(0, 0, 0,out ModelMatrix);
            ReadMaterials(path);
            ReadOBJ(path);
            GenerateFinalVertexArray();
            
            Console.WriteLine("Model "+path+ " constructed. Vertices:" + FinalVertexArray.Count+" Time:"+watch.Result()+" ms");
        }

        public void Draw()
        {
            
            Loader.UseShader();
            Matrix4.CreateTranslation(Position,out ModelMatrix);
            Loader.SetUniformMatrix4F("projection",ref Camera.Projection);
            Loader.SetUniformMatrix4F("view",ref Camera.View);
            Loader.SetUniformMatrix4F("model",ref ModelMatrix);
            GL.BindVertexArray(VAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, FinalVertexArray.Count / 3);
            GL.BindVertexArray(0);
            
        }

        private void SplitFLine(string[] line)
        {
            for (int i = 1; i < line.Length; i++)
            {
                string[] vertex = line[i].Split("/");
                Indices.Add(Convert.ToInt32(vertex[0]) - 1);
                NormalIndices.Add(Convert.ToInt32(vertex[2]) - 1);
                ColorPerIndex.Add(Materials[current_material]);
            }
        }

        public void SetPosition(float x=0f, float y=0f, float z=0f)
        {
            Matrix4.CreateTranslation(x, y, z,out ModelMatrix);
            Position = new Vector3(x, y, z);
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
        }
        
        public void SetRotation(Vector3 rot, float z=0f)
        {
            Matrix4.CreateRotationX(rot.X, out ModelMatrix);
            Matrix4.CreateRotationY(rot.Y, out ModelMatrix);
            Matrix4.CreateRotationZ(rot.Z, out ModelMatrix);
        }
        
        public void SetRotation(float x=0f, float y=0f, float z=0f)
        {
            Matrix4.CreateRotationX(x, out ModelMatrix);
            Matrix4.CreateRotationY(y, out ModelMatrix);
            Matrix4.CreateRotationZ(z, out ModelMatrix);
        }
        
        private void GenerateFinalVertexArray()
        {
            FinalVertexArray = new List<Vector3>();
            for (int x = 0; x < Indices.Count; x++)
            {
                //vec3 position,vec3 normals, vec3 color
                FinalVertexArray.Add(Vertices[Indices[x]]);
                FinalVertexArray.Add(Normals[NormalIndices[x]]);
                FinalVertexArray.Add(ColorPerIndex[x]);
            }
            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer,VBO);
            GL.BufferData(BufferTarget.ArrayBuffer,FinalVertexArray.Count*sizeof(float)*3,FinalVertexArray.ToArray(),BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3*sizeof(float));
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6*sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.BindVertexArray(0);
        }

        private void ReadMaterials(string path)
        {
            StreamReader materialreader = new StreamReader("Models/" + path + ".mtl", Encoding.UTF8);
            string current_material_name = "", material_line;
            while ((material_line = materialreader.ReadLine()) != null)
            {
                string[] substrings = material_line.Split(" ");
                switch (substrings[0])
                {
                    case "newmtl":
                        current_material_name = substrings[1];
                        break;
                    case "Kd":
                        var color = new Vector3(Convert.ToSingle(substrings[1], CultureInfo.InvariantCulture),
                            Convert.ToSingle(substrings[2], CultureInfo.InvariantCulture),
                            Convert.ToSingle(substrings[3], CultureInfo.InvariantCulture));
                        Materials.Add(current_material_name, color);
                        MaterialsIndices.Add(current_material_name,Materials.Count-1);
                        break;
                }
            }
        }

        private void ReadOBJ(string path)
        {
            string line;

            StreamReader reader = new StreamReader("Models/" + path + ".obj", Encoding.UTF8);
            while ((line = reader.ReadLine()) != null)
            {
                string[] substrings = line.Split(" ");
                switch (substrings[0])
                {
                    case "v":
                        Vertices.Add((Convert.ToSingle(substrings[1], CultureInfo.InvariantCulture),
                            Convert.ToSingle(substrings[2], CultureInfo.InvariantCulture),
                            Convert.ToSingle(substrings[3], CultureInfo.InvariantCulture)));
                        break;
                    case "vt":
                        //TextureCoordinates.Add((Convert.ToSingle(substrings[1], CultureInfo.InvariantCulture),
                        //Convert.ToSingle(substrings[2], CultureInfo.InvariantCulture)));
                        break;
                    case "vn":
                        Normals.Add((Convert.ToSingle(substrings[1], CultureInfo.InvariantCulture),
                            Convert.ToSingle(substrings[2], CultureInfo.InvariantCulture),
                            Convert.ToSingle(substrings[3], CultureInfo.InvariantCulture)));
                        break;
                    case "f":
                        SplitFLine(substrings);
                        break;
                    case "usemtl":
                        current_material = substrings[1];
                        break;
                }
            }
        }
        
    }
}