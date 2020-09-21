using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using fs;

public class MBDLoader : MonoBehaviour {
    MbdFile mbd;

    void Update() {
        if(Input.GetKeyDown(KeyCode.P)) {
            StartCoroutine("LoadMBD200");
        }
        if(Input.GetKeyDown(KeyCode.O)) {
            StartCoroutine("LoadMBD");
        }
        if(Input.GetKeyDown(KeyCode.Q)) {
            Debug.Log(Quaternion.FromToRotation(new Vector3(1,0,0),new Vector3(0,1,0)).eulerAngles);
            Debug.Log(Quaternion.FromToRotation(new Vector3(1,0,0),new Vector3(0.60106f,-0.05511f,0.79730f)).eulerAngles);
        }
    }

    IEnumerator LoadMBD() {
        string path = "/map/bus1.mbd";
        mbd = new MbdFile(G.BasePath + path);
        Debug.Log(mbd.originCount);
        Debug.Log(mbd.nodeCount);
        for(uint i = 0;i<mbd.originCount;i++) {
            switch(mbd.origins[i].nodeType) {
                //case OriginType.Model: LoadModel((MbdFile.Model)mbd.origins[i]); break;
                //case OriginType.MissionModel: LoadMissionModel((MbdFile.MissionModel)mbd.origins[i]); break;
                case OriginType.Road: LoadRoad((MbdFile.Road)mbd.origins[i]); break;
                //case OriginType.Prefab: LoadPrefab((MbdFile.Prefab)mbd.origins[i]); break;
            }
            yield return null;
        }
        Debug.Log("加载完成");
    }
    IEnumerator LoadMBD200() {
        string path = "/map/bus1.mbd";
        mbd = new MbdFile(G.BasePath + path);
        Debug.Log(mbd.originCount);
        Debug.Log(mbd.nodeCount);
        for(uint i = 0;i<200/*mbd.originCount*/;i++) {//mbd.originCount
            switch(mbd.origins[i].nodeType) {
                //case OriginType.Model: LoadModel((MbdFile.Model)mbd.origins[i]); break;
                //case OriginType.MissionModel: LoadMissionModel((MbdFile.MissionModel)mbd.origins[i]); break;
                case OriginType.Road: LoadRoad((MbdFile.Road)mbd.origins[i]); break;
                    //case OriginType.Prefab: LoadPrefab((MbdFile.Prefab)mbd.origins[i]); break;
            }
            yield return null;
        }
        Debug.Log("加载完成");
    }

    void LoadModel(MbdFile.Model model) {
        Vector3 pos = mbd.nodes[model.nodeIndex].position.GetVector();
        Vector3 rot = model.rotation.GetVector();
        GameObject a = Instantiate(G.I.loadWorld.model[model.modelNum],pos,Quaternion.Euler(rot));
        a.transform.localScale=model.scale.GetVector();
    }
    void LoadMissionModel(MbdFile.MissionModel misModel) {
        Vector3 pos = mbd.nodes[misModel.nodeIndex].position.GetVector();
        Vector3 rot = misModel.rotation.GetVector();

        GameObject a = Instantiate(G.I.loadWorld.model[misModel.modelNum],pos,Quaternion.Euler(rot));
        a.transform.localScale=misModel.scale.GetVector();
        Debug.Log("Mission model:");
    }


    void LoadRoad(MbdFile.Road road) {
        //开头node
        Node nodeStart = mbd.GetNode(road.startIndex);
        GameObject root = new GameObject("root_" + nodeStart.thisOrigin);
        root.transform.position=nodeStart.Position;

        GameObject item = new GameObject("road_" + nodeStart.thisOrigin);
        item.transform.position = nodeStart.Position;
        item.transform.rotation = Quaternion.FromToRotation(Vector3.right, nodeStart.Direction);
        item.transform.parent=root.transform;

        Bezier bz = root.AddComponent<Bezier>();
        bz.nodeStart = new Bezier.BezierNode {
            position = nodeStart.Position,
            tangent=nodeStart.direction.GetVector()
        };

        MbdFile.Road road1 = (MbdFile.Road)mbd.origins[nodeStart.thisOrigin];
        bz.nodeStart.length = road1.tangent/3;
        bz.nodeStart.width = (G.I.loadWorld.road[road1.roadNum].roadSize + G.I.loadWorld.road[road1.roadNum].roadOffset) * 2;
        bz.road = road1;
        if(G.GetFlag(road1.flag,(uint)MbdFile.Road.Flag.HalfRoadStep)) {
            //平滑
            bz.nodeStart.segmentNum = System.Math.Max((uint)(bz.nodeStart.length*0.4f),1);
        } else {
            bz.nodeStart.segmentNum = System.Math.Max((uint)(bz.nodeStart.length*0.2f),1);
        }

        //结尾node
        Node nodeEnd = mbd.GetNode(road.endIndex);

        GameObject item2 = new GameObject("road_"+nodeEnd.thisOrigin);
        item2.transform.position=nodeEnd.Position;
        item2.transform.rotation = Quaternion.FromToRotation(Vector3.right,nodeEnd.Direction);
        item2.transform.parent=root.transform;

        bz.nodeEnd=new Bezier.BezierNode {
            position = nodeEnd.Position,
            tangent = nodeEnd.direction.GetVector()
        };

        bz.show=true;
        bz.UpdateMeshWithTerrain();
    }

    void LoadPrefab(MbdFile.Prefab prefab) {
        short nodeIndex = prefab.nodeIndex;

        int isOrigin = 0;
        for(int i = 0;i<prefab.indexs.Length;i++) {
            uint thisOrigin = mbd.nodes[prefab.indexs[nodeIndex]].thisOrigin;
            if(thisOrigin<0||thisOrigin>mbd.origins.Length) continue;
            if(mbd.origins[thisOrigin].nodeType==OriginType.Prefab) {
                isOrigin=i;
                break;
            }
        }

        Vector3 pos = mbd.nodes[prefab.indexs[isOrigin]].position.GetVector();// (prefab.dataHead.bound_1.GetVector()+prefab.dataHead.bound_2.GetVector())/2;
        Vector3 rot = new Vector3(0,prefab.rotY,prefab.rotZ);
        PddFile pdd = DefLib.pdds[prefab.prefabNum];

        GameObject prefabNode = new GameObject("prefab"+prefab.prefabNum);
        prefabNode.transform.position=pos;
        prefabNode.transform.rotation=Quaternion.Euler(rot);
        GameObject n = Instantiate(G.I.loadWorld.prefab[prefab.prefabNum]);
        n.transform.parent=prefabNode.transform;
        n.transform.localRotation=new Quaternion();
        n.transform.localPosition=-pdd.node[nodeIndex].pos.GetVector();
    }
}
