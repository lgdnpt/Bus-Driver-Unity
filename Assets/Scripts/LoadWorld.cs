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
    public Material[] terMats;

    private fs.DefReader def;
    private string temp;
    GameObject lib;
    void Start() {

    }

    public void LoadAll() {
        lib = new GameObject("lib");
        
        StartCoroutine("LoadModelCoroutine");
        //LoadModel();
        //LoadPrefab();
        //LoadRoadLook();
        //LoadTerrain();
        lib.SetActive(false);
    }
    public void Load() {
        PMGLoader.LoadPMG(modelList[dropdown.value]);
    }


    IEnumerator LoadModelCoroutine() {
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
        Debug.Log("读取模型结束");
        StartCoroutine("LoadPrefabCoroutine");
    }


    IEnumerator LoadPrefabCoroutine() {
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
        Debug.Log("读取路口结束");
        StartCoroutine("LoadRoadLook");
    }

    IEnumerator LoadRoadLook() {
        def = new fs.DefReader(G.BasePath + "/def/world/road.def");
        def.keys.TryGetValue("road_look_count",out temp);
        int roadLookCount = int.Parse(temp);
        road=new RoadLook[roadLookCount];

        for(int i = 0;i<roadLookCount;i++) {
            def.keys.TryGetValue("road_look_data"+i,out temp);
            if(temp.Equals("\"\"")) {
                continue;
            }
            temp=temp.Substring(1,temp.Length-2).Trim();
            string[] temps = temp.Split(';');
            if(temps.Length!=12) Debug.LogError("道路"+i+"数据错误");
            road[i]=new RoadLook(temps);
            yield return null;
        }
        Debug.Log("读取道路样式结束");
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
        //Debug.LogWarning(terProfile.Length);
        Debug.Log("读取地形结束");
    }
    public struct TerrainProfile {
        public string name;
        public float[] height;
        public float[] step;
    }
}
