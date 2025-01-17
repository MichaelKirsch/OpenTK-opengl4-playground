﻿using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using ImGuiNET;
using System.Xml;
using YEET.Engine.Core;

namespace YEET.Engine.ECS
{
    public class House : Entity
    {
        Random t = new Random();
        private UInt32 grassMesh, house;
        public bool isInstanced;

        private float highlightTest;
        private List<string> modelnames=new List<string>();
        public House()
        {
            Name = "House";
            grassMesh = AddComponent(new Mesh(this,"2x2_grass"));
            house = AddComponent(new Mesh(this,"house_type12"));
            AddComponent(new RotateToObject(this));
            AddComponent(new Collider(this,new Vector3(0,0,0),new Vector3(2,2,2)));
        }
        
        public House(Vector3 position)
        {
            Name = "House";
            grassMesh = AddComponent(new Mesh(this,"2x2_grass"));
            house = AddComponent(new Mesh(this,"house_type12"));
            GetComponent<Transform>().Position = position;
            AddComponent(new Collider(this,new Vector3(0,0,0),new Vector3(2,2,2)));
        }
        /// <summary>
        /// construct a house from a xml file
        /// </summary>
        /// <param name="name"></param>
        /// <param name="position"></param>
        public House(String name,Vector3 position,bool Instanced=true)
        {
            isInstanced = Instanced;
            Name = "House";
            XmlDocument doc = new XmlDocument();
            doc.Load($"Data/Models/{name}.xml");
            
            var variants = doc.GetElementsByTagName("variant");
            
            var chosen = variants[t.Next(variants.Count-1)].FirstChild;

            for (int i = 0; i < chosen.ChildNodes.Count; i++)
            {
                if (isInstanced)
                {
                      modelnames.Add(chosen.ChildNodes[i].InnerText);
                }
                else
                {
                    AddComponent(new Mesh(this,chosen.ChildNodes[i].InnerText));
                }
            }
            
            highlightTest = t.Next()%100>20?0f:1f;
            GetComponent<Transform>().Position = position;
            AddComponent(new Collider(this,new Vector3(0,0,0),new Vector3(2,2,2)));
        }

        public override void OnGui()    
        {
            ImGui.Checkbox("Active", ref Active);
            base.OnGui();
            ShowGUI = !ImGui.Button("Dont Show");

        }

        public override void OnUpdate()
        {
            base.OnUpdate();
        }


        public override void OnRender(){
            if (isInstanced)
            {
                if(Math.Abs(new Vector3(GetComponent<Transform>().Position-Camera.Position).Length)>Camera.RenderingDistance){return;}
                foreach (var modelname in modelnames)
                {
                    List<float> data = new List<float>();
                    //TODO transform all the data to a float array
                    data.Add(highlightTest);
                    data.Add(GetComponent<Transform>().Position.Z);
                    data.Add(GetComponent<Transform>().Position.Y);
                    data.Add(GetComponent<Transform>().Position.X);
                    InstanceRenderer.AddToStack(modelname,data);
                }
                return;
            }
            foreach (var item in GetComponents<Mesh>())
            {
                item.SetUniform("UsingOverwriteColor",true);
                item.OnDraw();
            }
        }
    }
}