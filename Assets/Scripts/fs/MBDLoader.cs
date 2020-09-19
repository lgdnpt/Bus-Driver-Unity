using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using fs;

public class MBDLoader : MonoBehaviour {
    MbdFile mbd;

    void Update() {
        if(Input.GetKeyDown(KeyCode.P)) {
            StartCoroutine("LoadMBD");
        }
    }

    IEnumerator LoadMBD() {
        string path = "/map/bus1.mbd";
        mbd = new MbdFile(G.BasePath + path);
        Debug.Log(mbd.originCount);
        Debug.Log(mbd.nodeCount);
        for(uint i = 0;i<mbd.originCount;i++) {//mbd.originCount
            switch(mbd.origins[i].nodeType) {
                case OriginType.Model: LoadModel((MbdFile.Model)mbd.origins[i]); break;
                //case OriginType.MissionModel: LoadMissionModel((MbdFile.MissionModel)mbd.origins[i]); break;
                case OriginType.Road: LoadRoad((MbdFile.Road)mbd.origins[i]); break;
                case OriginType.Prefab: LoadPrefab((MbdFile.Prefab)mbd.origins[i]); break;
            }
            yield return null;
        }
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
        uint i = mbd.nodes[road.startIndex].thisOrigin;
        //Debug.Log(i+"是Road");
        if(mbd.nodes[road.endIndex].thisOrigin==0xFFFFFFFF
            || mbd.origins[mbd.nodes[road.endIndex].thisOrigin].nodeType!=OriginType.Road) {
            //一段路结束

            //if((road.flag & 0x00010000)!=0x00010000) return;
            //Debug.Log(i+"是Road结束");
            GameObject root = new GameObject("root_"+i);
            Bezier bz = root.AddComponent<Bezier>();

            Stack<Node> stack = new Stack<Node>();

            stack.Push(mbd.nodes[road.endIndex]);
            Node pr = mbd.nodes[road.startIndex];
            stack.Push(pr);

            //回溯到road起点
            while(pr.backOrigin!=0xFFFFFFFF) {
                if(mbd.origins[pr.backOrigin].nodeType==OriginType.Road) {
                    pr = mbd.nodes[((MbdFile.Road)mbd.origins[pr.backOrigin]).startIndex];
                    stack.Push(pr);
                } else {
                    break;
                }
            }

            bz.controlPoints=new Transform[stack.Count];
            bz.nodes=new Bezier.BezierNode[stack.Count];


            Node temp;
            for(int j = 0;stack.Count>0;j++) {
                temp = stack.Pop();
                //Debug.Log("出栈");

                Vector3 pos = temp.position.GetVector();
                GameObject item = new GameObject("road_"+temp.thisOrigin);
                item.transform.position=pos;
                item.transform.parent=root.transform;

                //bz.controlPoints[j] = item.transform;
                bz.nodes[j]=new Bezier.BezierNode {
                    position = pos,
                    tangent=temp.direction.GetVector()
                };

                if(temp.thisOrigin!=0xFFFFFFFF && mbd.origins[temp.thisOrigin].nodeType==OriginType.Road) {
                    MbdFile.Road road1 = (MbdFile.Road)mbd.origins[temp.thisOrigin];
                    bz.nodes[j].length = road1.tangent/3;
                    bz.nodes[j].width = (G.I.loadWorld.road[road1.roadNum].roadSize+
                        G.I.loadWorld.road[road1.roadNum].roadOffset)*2;
                    bz.nodes[j].road = road1;

                    if((road1.flag & 0x01000000) == 0x01000000) {
                        //平滑
                        bz.nodes[j].segmentNum = System.Math.Max((uint)(bz.nodes[j].length*0.4f),1);
                    } else {
                        bz.nodes[j].segmentNum = System.Math.Max((uint)(bz.nodes[j].length*0.2f),1);
                    }
                    
                } else {
                    bz.nodes[j].length = bz.nodes[j-1].length;
                    bz.nodes[j].width = bz.nodes[j-1].width;
                    bz.nodes[j].road = bz.nodes[j-1].road;
                    bz.nodes[j].segmentNum=bz.nodes[j-1].segmentNum;
                }
                
            }
            //bz.nodes[bz.nodes.Length-1].tangent=bz.nodes[bz.nodes.Length-2].tangent;
            bz.show=true;

            if((road.flag & 0x00010000)==0x00010000) {
                //Debug.Log(i+"是地形");
                bz.UpadteTerrain();
            } else {
                bz.UpdateMesh();
            }
        }
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
