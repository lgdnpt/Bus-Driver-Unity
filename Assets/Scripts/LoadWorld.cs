﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadWorld : MonoBehaviour {
    public Dropdown dropdown;
    //public PMGLoader pmgLoader;
    List<string> modelList = new List<string>();
    public GameObject[] model;
    public GameObject[] prefab;
    public RoadLook[] road;
    public TerrainProfile[] terProfile;
    public Material[] roadMats;
    public Material[] roadSideMats;
    public Material[] terMats;
    public Material[] sidewalkMats;
    public Railing[] railings;

    private fs.DefReader def;
    private string temp;
    GameObject lib;

    public void LoadAll() {
        lib = new GameObject("lib");
        
        //StartCoroutine("LoadModelCoroutine");
        //StartCoroutine("LoadRoadLook");
        StartCoroutine("LoadPrefabCoroutine");
        //LoadModel();
        //LoadPrefab();
        //LoadRoadLook();
        //LoadTerrain();
        lib.SetActive(false);
    }
    public void Load() {
        PMGLoader.LoadPMG(modelList[dropdown.value]);
    }
    public void Load(string path) {
        PMGLoader.LoadPMG(path);
    }


    IEnumerator LoadModelCoroutine() {
        Debug.Log("Loading model data ....");
        def = new fs.DefReader(G.BasePath + "/def/world/model.def");

        def.keys.TryGetValue("model_count",out temp);
        int modelCount = int.Parse(temp);

        model=new GameObject[modelCount];

        for(int i = 0;i<modelCount;i++) {
            def.keys.TryGetValue("model"+i,out temp);
            if(temp.Equals("\"\"")) {
                continue;
            }
            temp=temp.Substring(temp.IndexOf('|')+1,temp.Length-temp.IndexOf('|')-2).Trim();
            if(temp.Contains("|")) {
                temp=temp.Substring(0,temp.IndexOf('|')).Trim();
            }

            model[i]=new GameObject(temp.Substring(temp.LastIndexOf('/')+1).Replace(".pmd",""));
            model[i].transform.parent=lib.transform;
            PMGLoader.LoadPMG(temp,model[i]);

            modelList.Add(temp);
            yield return 0;
        }
        StartCoroutine("LoadPrefabCoroutine");
    }


    IEnumerator LoadPrefabCoroutine() {
        Debug.Log("Loading prefab data ....");
        def = new fs.DefReader(G.BasePath + "/def/world/prefab.def");
        def.keys.TryGetValue("prefab_count",out temp);
        int prefabCount = int.Parse(temp);
        prefab=new GameObject[prefabCount];

        for(int i = 0;i<prefabCount;i++) {
            def.keys.TryGetValue("prefab"+i,out temp);
            if(temp.Equals("\"\"")) {
                continue;
            }
            temp=temp.Substring(1,temp.Length-2).Trim();


            prefab[i]=new GameObject(temp.Substring(temp.LastIndexOf('/')+1).Replace(".pmd",""));
            prefab[i].transform.parent=lib.transform;
            PMGLoader.LoadPMG(temp,prefab[i]);
            yield return 0;
        }
        StartCoroutine("LoadRoadLook");
    }

    IEnumerator LoadRoadLook() {
        Debug.Log("Loading road data ....");
        def = new fs.DefReader(G.BasePath + "/def/world/road.def");

        //加载路面/路牙材质
        def.keys.TryGetValue("material_count", out temp);
        int materialCount = int.Parse(temp);
        roadMats = new Material[materialCount];
        roadSideMats = new Material[materialCount];
        for(int i = 0; i<materialCount; i++) {
            def.keys.TryGetValue("material"+i, out temp);
            if(temp.Equals("\"\"")) {
                continue;
            }
            temp=temp.Substring(1, temp.Length-2).Trim();
            string[] temps = temp.Split('|');
            if(temps.Length!=2) {
                Debug.LogError("[def] 道路材质"+i+"数据错误");
                continue;
            }
            roadMats[i] = fs.Cache.LoadMat(temps[0].Trim()).Material;
            roadSideMats[i] = fs.Cache.LoadMat(temps[1].Trim()).Material;

            yield return null;
        }

        //加载人行道材质
        def.keys.TryGetValue("sidewalk_material_count", out temp);
        int sidewalkCount = int.Parse(temp);
        sidewalkMats = new Material[sidewalkCount];
        for(int i = 0; i<sidewalkCount; i++) {
            def.keys.TryGetValue("sidewalk"+i, out temp);
            if(temp.Equals("\"\"")) {
                continue;
            }
            temp=temp.Trim().Substring(1, temp.Length-2).Trim();
            sidewalkMats[i] = fs.Cache.LoadMat(temp).Material;
            yield return null;
        }

        //加载道路样式
        def.keys.TryGetValue("road_look_count",out temp);
        int roadLookCount = int.Parse(temp);
        road = new RoadLook[roadLookCount];
        for(int i = 0;i<roadLookCount;i++) {
            def.keys.TryGetValue("road_look_data"+i,out temp);
            if(temp.Equals("\"\"")) {
                continue;
            }
            temp=temp.Substring(1,temp.Length-2).Trim();
            string[] temps = temp.Split(';');
            if(temps.Length!=12) {
                Debug.LogError("[def] 道路样式"+i+"数据错误");
                continue;
            }
            road[i]=new RoadLook(temps);
            yield return null;
        }
        StartCoroutine("LoadTerrain");
    }

    public class RoadLook {
        public float roadSize;    //宽度
        public float roadOffset;  //偏移(隔离带)
        public float textureLeft;
        public float textureRight;
        public bool doubleWLine;
        public byte whiteStyle;
        public byte centerLine;
        public bool useWhiteDiv2;
        public byte aiLaneCount;
        public byte actorLaneCount;
        public bool oneWay;
        public string name;
        public RoadLook(string[] vs) {
            roadSize=float.Parse(vs[0].Trim());
            roadOffset=float.Parse(vs[1].Trim());
            textureLeft=float.Parse(vs[2].Trim());
            textureRight=float.Parse(vs[3].Trim());
            doubleWLine=bool.Parse(vs[4].Trim());
            whiteStyle=byte.Parse(vs[5].Trim());
            centerLine=byte.Parse(vs[6].Trim());
            useWhiteDiv2=bool.Parse(vs[7].Trim());
            aiLaneCount=byte.Parse(vs[8].Trim());
            actorLaneCount=byte.Parse(vs[9].Trim());
            oneWay=bool.Parse(vs[10].Trim());
            name=vs[11].Trim();
        }
    }

    IEnumerator LoadTerrain() {
        Debug.Log("Loading terrain data ....");
        def = new fs.DefReader(G.BasePath + "/def/world/terrain.def");

        //读取地形材质
        def.keys.TryGetValue("material_count",out temp);
        int matcount = int.Parse(temp);
        terMats = new Material[matcount];
        for(int i = 0;i<matcount;i++) {
            def.keys.TryGetValue("material"+i,out temp);
            temp = temp.Substring(1,temp.Length-2).Trim();

            terMats[i] = fs.Cache.LoadMat(temp).Material;
            yield return null;
        }


        //读取地形配置
        def.keys.TryGetValue("profile_count",out temp);
        int count = int.Parse(temp);
        terProfile=new TerrainProfile[count];

        for(int i = 0;i<count;i++) {
            terProfile[i]=new TerrainProfile();
            def.keys.TryGetValue("profile_name"+i,out temp);
            terProfile[i].name=temp.Substring(1,temp.Length-2).Trim();

            def.keys.TryGetValue("profile_height"+i,out temp);
            string[] temps = temp.Substring(1,temp.Length-2).Trim().Split(';');
            def.keys.TryGetValue("profile_step"+i,out temp);
            string[] temps2 = temp.Substring(1,temp.Length-2).Trim().Split(';');

            terProfile[i].height=new float[temps.Length];
            terProfile[i].step=new float[temps.Length];
            for(int j = 0;j<temps.Length;j++) {
                terProfile[i].height[j]=float.Parse(temps[j]);
                terProfile[i].step[j]=float.Parse(temps2[j]);
            }
            yield return null;
        }
        StartCoroutine(nameof(LoadRailing));
    }

    IEnumerator LoadRailing() {
        Debug.Log("Loading railing data ....");
        def = new fs.DefReader(G.BasePath + "/def/world/railing.def");

        //读取地形材质
        def.keys.TryGetValue("model_count", out temp);
        int modelCount = int.Parse(temp);
        railings = new Railing[modelCount];

        GameObject obj;
        Railing tempRailing;
        for(int i = 0; i<modelCount; i++) {
            def.keys.TryGetValue("model"+i, out temp);
            temp = temp.Substring(1, temp.Length-2).Trim();

            obj = new GameObject(temp);
            obj.transform.parent = lib.transform;
            PMGLoader.LoadPMG(temp, obj);
            tempRailing = obj.AddComponent<Railing>();


            GetRailing(tempRailing, "start5m", ref tempRailing.start5);
            GetRailing(tempRailing, "startcol5m", ref tempRailing.startcol5);
            GetRailing(tempRailing, "start15m", ref tempRailing.start15);
            GetRailing(tempRailing, "startcol15m", ref tempRailing.startcol15);

            GetRailing(tempRailing, "center5m", ref tempRailing.center5);
            GetRailing(tempRailing, "centercol5m", ref tempRailing.centercol5);
            GetRailing(tempRailing, "center15m", ref tempRailing.center15);
            GetRailing(tempRailing, "centercol15m", ref tempRailing.centercol15);

            GetRailing(tempRailing, "end5m", ref tempRailing.end5);
            GetRailing(tempRailing, "endcol5m", ref tempRailing.endcol5);
            GetRailing(tempRailing, "end15m", ref tempRailing.end15);
            GetRailing(tempRailing, "endcol15m", ref tempRailing.endcol15);


            railings[i] = tempRailing;
            yield return null;
        }

    }

    private Transform tempTransform;
    private void GetRailing(Railing railing, string name, ref MeshFilter railMesh) {
        tempTransform = railing.transform.Find(name);
        if(tempTransform != null) {
            railMesh = tempTransform.GetComponent<MeshFilter>();
        } else {
            //TODO 加载中段显示模型作为丢失部分碰撞体
            Debug.LogWarning("[railing] Missing colbox for '" +railing.transform.name+ "' in " + name);

            /*if(name.Contains("15m")) {
                tempTransform = railing.transform.Find("center15m");
                tempTransform = Instantiate(tempTransform, tempTransform.parent);
            } else {
                tempTransform = railing.transform.Find("center5m");
                tempTransform = Instantiate(tempTransform, tempTransform.parent);
            }
            if(tempTransform != null) railMesh = tempTransform.GetComponent<MeshFilter>().mesh;
            else Debug.LogError("[railing] Missing colbox for '" +railing.transform.name+ "' in " + name);*/
        }
    }

    public struct TerrainProfile {
        public string name;
        public float[] height;
        public float[] step;
    }
}
