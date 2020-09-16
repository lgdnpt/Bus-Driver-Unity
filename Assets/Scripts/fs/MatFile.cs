using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;

namespace fs {
    class MatFile {
        public string matPath;
        public string shader;
        public string[] texturePath;
        public TobjFile[] tobj;
        public float[] ambient;// = new float[3] { 1.0f,1.0f,1.0f };
        public float[] diffuse;// = new float[3] { 1.0f,1.0f,1.0f };
        public float[] specular;// = new float[3] { 1.0f,1.0f,1.0f };
        public float[] env_factor;// = new float[3] { 1.0f,1.0f,1.0f };
        public long shininess = 100;
        public float tint_opacity = 0.5f;
        public float[] tint;// = new float[3] { 1.0f,1.0f,1.0f };

        //mat缓存库
        public static Dictionary<string,Material> lib=new Dictionary<string,Material>();

        public MatFile(string path) {
            matPath=path;
            DefReader reader;// = new DefReader(GlobalClass.GetBasePath() + path);
            //获得全部属性
            try {
                reader = new DefReader(G.BasePath + path);
            } catch(FileNotFoundException e) {
                throw e;
            }
            string temp;
            if(reader.keys.ContainsKey("material")) {
                reader.keys.TryGetValue("material",out temp);
                shader=temp.Substring(1,temp.LastIndexOf('"')-1);
            } else {
                Debug.LogError("mat格式错误");
                return;
            }

            //读取贴图路径,默认容量3
            List<string> tobjPath=new List<string>(3);
            for(int i=0; reader.keys.ContainsKey("texture["+i+"]"); i++) {
                reader.keys.TryGetValue("texture["+i+"]",out temp);
                tobjPath.Add(temp.Substring(1,temp.Length-2));
            }

            texturePath=tobjPath.ToArray();
            if(texturePath.Length<1) {
                if(reader.keys.ContainsKey("texture")) {
                    texturePath=new string[1];
                    reader.keys.TryGetValue("texture",out temp);
                    texturePath[0]=temp.Substring(1,temp.Length-2);
                    if(!texturePath[0].Contains("/")) {
                        //Debug.Log("修正相对路径");
                        texturePath[0]=path.Substring(0,path.LastIndexOf('/')+1)+texturePath[0];
                    }
                }
            }
/*          texture=new TobjFile[texturePath.Length];
            for(int i = 0;i<texturePath.Length;i++) {
                temp = GlobalClass.GetBasePath() + texturePath[i];
                texture[i]=new TobjFile(temp);
            }*/

            if(reader.keys.ContainsKey("ambient")) {
                reader.keys.TryGetValue("ambient",out temp);
                ambient=GetFloat(temp);
            }
            if(reader.keys.ContainsKey("diffuse")) {
                reader.keys.TryGetValue("diffuse",out temp);
                diffuse=GetFloat(temp);
            }
            if(reader.keys.ContainsKey("specular")) {
                reader.keys.TryGetValue("specular",out temp);
                specular=GetFloat(temp);
            }
            if(reader.keys.ContainsKey("env_factor")) {
                reader.keys.TryGetValue("env_factor",out temp);
                env_factor=GetFloat(temp);
            }
            if(reader.keys.ContainsKey("tint")) {
                reader.keys.TryGetValue("tint",out temp);
                tint=GetFloat(temp);
            }
            if(reader.keys.ContainsKey("shininess")) {
                reader.keys.TryGetValue("shininess",out temp);
                shininess=long.Parse(temp);
            }
            if(reader.keys.ContainsKey("tint_opacity")) {
                reader.keys.TryGetValue("tint_opacity",out temp);
                tint_opacity=float.Parse(temp);
            }
        }

        public static Texture GetTexture(string matPath,bool useCache=true) {
            if(useCache) {
                try {
                    TobjFile tobj = Cache.LoadTobj(new MatFile(matPath).texturePath[0]);
                    return tobj.texture;
                } catch(FileNotFoundException e) {
                    throw e;
                }
            } else {
                TobjFile tobj = new TobjFile(new MatFile(matPath).texturePath[0]);
                return tobj.texture;
            }
        }

        float[] GetFloat(string str) {
            int startIndex = str.IndexOf('{');
            string[] temps=str.Substring(startIndex+1,str.IndexOf('}')-startIndex-1).Split(',');
            return new float[3] { float.Parse(temps[0].Trim()),float.Parse(temps[1].Trim()),float.Parse(temps[2].Trim()) };
        }

        
        public Material GetMaterial() {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Debug.Log("  shader:"+shader);
            //mat.SetFloat("_WorkflowMode",0.0f);


            //反光,设源为alpha
            if(shader.Contains("dif_spec")) {
                //启用albedo的alpha
                mat=new Material(GlobalBusDriver.Instance.dif_spec_add_env);
                mat.SetFloat("_Smoothness",1f);
            } else {
                //mat.SetFloat("_Smoothness",0f);
            }

            if(shader.Contains(".a.")) {
                mat=new Material(GlobalBusDriver.Instance.dif_a_decal_over);
            }

            if(shader.Contains("none_spec")) {
                //玻璃
                mat=new Material(GlobalBusDriver.Instance.none_spec_add_env);
            }

            /*if(mat == null)
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));*/
            mat.name=matPath;

            //读取tobj
            if(texturePath.Length>0) {
                tobj=new TobjFile[texturePath.Length];
                for(int i = 0;i<texturePath.Length;i++) {
                    if(texturePath[i].IndexOf('/')==-1) {
                        texturePath[i] = matPath.Substring(0,matPath.LastIndexOf('/')+1)+texturePath[i];

                    }
                    //读取缓存
                    if(TobjFile.lib.ContainsKey(texturePath[i])) {
                        TobjFile.lib.TryGetValue(texturePath[i],out tobj[i]);
                    } else {
                        tobj[i]=new TobjFile(texturePath[i]);
                        TobjFile.lib.Add(texturePath[i],tobj[i]);
                    }
                }
                //应用主纹理
                if(tobj[0].texture!=null) {
                    mat.SetTexture("_BaseMap",tobj[0].texture);
                }
            }

            //应用颜色
            if(diffuse!=null) {
                mat.SetColor("_BaseColor",new Color(diffuse[0],diffuse[1],diffuse[2]));
            }
            if(specular!=null) {
                mat.SetColor("_SpecColor",new Color(specular[0],specular[1],specular[2]));
            } else {
                mat.SetColor("_SpecColor",Color.black);
            }
            return mat;
        }
    }
}
