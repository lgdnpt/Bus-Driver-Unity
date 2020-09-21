﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using fs;

public class Bezier : MonoBehaviour {
    [System.Serializable]
    public class BezierNode {
        public Vector3 position;
        public Vector3 tangent;
        public float length;
        public uint segmentNum;
        public float width;
        public float offset;
        //public MbdFile.Road road;
    }

    public RoadType roadType;

    public Transform[] controlPoints;
    public BezierNode nodeStart;
    public BezierNode nodeEnd;
    
    public MeshFilter filterBase;
    public MeshRenderer renderBase;
    public float optimize = 0.5f;
    public bool fixedLength = true;
    public bool show = false;
    public MbdFile.Road road;

    private void Start() {
        if(filterBase==null) filterBase=gameObject.AddComponent<MeshFilter>();
        if(renderBase==null) renderBase=gameObject.AddComponent<MeshRenderer>();
    }


    void OnDrawGizmos() {
        if(show) { 
            //DrawCurve(fixedLength);
        }
    }

    /*void DrawCurve(bool fixedLength) {
        Gizmos.color = Color.red;
        Vector3 ppp1 = nodeEnd.position-nodeStart.position, ppp2;            //计算切线和长度

        if(!fixedLength) {
            ppp2 = ppp1;
            ppp1 = nodeEnd.position-nodeStart.position;
            nodeStart.length=Distance(nodeStart.position,nodeEnd.position)/3;//Vector3.Distance(nodes[i+1].position,nodes[i].position)/4;
            nodeStart.tangent=(ppp1.normalized+ppp2.normalized).normalized;
        }
        nodeStart.segmentNum=(uint)(nodeStart.length*optimize);
        //画切线
        Gizmos.DrawLine(nodes[i].position,nodes[i].position+(nodes[i].length*nodes[i].tangent));
        if(!fixedLength) {
            ppp2 = ppp1;
            ppp1 = nodes[i+1].position-nodes[i].position;
            nodes[i].length=Distance(nodes[i].position,nodes[i+1].position)/3;//Vector3.Distance(nodes[i+1].position,nodes[i].position)/4;
            nodes[i].tangent=(ppp1.normalized+ppp2.normalized).normalized;
        }
        nodes[i].segmentNum=(uint)(nodes[i].length*optimize);
        //画切线
        Gizmos.DrawLine(nodes[i].position,nodes[i].position+(nodes[i].length*nodes[i].tangent));


        if(nodes.Length>2) {
            if(fixedLength) { 
                nodes[nodes.Length-1].tangent=(nodes[nodes.Length-1].position-nodes[nodes.Length-2].position).normalized;
                nodes[nodes.Length-1].length=Distance(nodes[nodes.Length-1].position,nodes[nodes.Length-2].position)/3;
            }
        }

        start = nodes[0].position;
        Gizmos.color = Color.magenta;
        //插值
        for(int nodeIndex = 0;nodeIndex<nodes.Length-1;nodeIndex++) {
            for(int i = 1;i <= nodes[nodeIndex].segmentNum;i++) {
                float t = i / (float)nodes[nodeIndex].segmentNum;

                Vector3 c1 = nodes[nodeIndex].position+ nodes[nodeIndex].tangent *nodes[nodeIndex].length;
                Vector3 c2 = nodes[nodeIndex+1].position- nodes[nodeIndex+1].tangent *nodes[nodeIndex].length;
                end = CalculateBezierPoint(nodes[nodeIndex].position, nodes[nodeIndex+1].position, c1,c2,t);
                Gizmos.DrawLine(start,end);
                start=end;
            }
        }
    }*/

    Vector3 CalculateBezierPoint(Vector3 p0,Vector3 p1,Vector3 c1,Vector3 c2,float t) {
        float u = 1-t;
        return u*u*u * p0 + 3*t*u*u * c1 + 3*t*t*u * c2 + t*t*t*p1;
    }
    Vector3 CalculateBezierTangent(Vector3 p0,Vector3 p1,Vector3 c1,Vector3 c2,float t) {
        float u = 1-t;
        return (-3*u*u * p0 + 3*(u*u - 2*t*u)*c1 + 3*(2*t*u-t*t) * c2 + 3*t*t*p1).normalized;
    }

    private float dx, dy, dz;
    float Distance(Vector3 a,Vector3 b) {
        dx = b.x - a.x;
        dy = b.y - a.y;
        dz = b.z - a.z;
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }


/*    public void NodeInit() {
        Vector3 ppp1 = nodeEnd.position-nodeStart.position, ppp2;            //计算切线和长度
        for(int i = 0;i<nodes.Length-1;i++) {
            ppp2 = ppp1;
            ppp1 = nodes[i+1].position-nodes[i].position;
            nodes[i].length=Distance(nodes[i].position,nodes[i+1].position)/3;//Vector3.Distance(nodes[i+1].position,nodes[i].position)/4;
            nodes[i].tangent=(ppp1.normalized+ppp2.normalized).normalized;            //nodes[i].segmentNum=(uint)(nodes[i].length*optimize);
        }
        if(nodes.Length>2) {
            nodes[nodes.Length-1].tangent=(nodes[nodes.Length-1].position-nodes[nodes.Length-2].position).normalized;
            nodes[nodes.Length-1].length=Distance(nodes[nodes.Length-1].position,nodes[nodes.Length-2].position)/3;
        }
    }*/
    
        
    private class MeshParams {
        private List<Vector3> vertices;
        private List<Vector2> uvs;
        private List<int> triangles;
        public MeshParams() {
            vertices = new List<Vector3>();
            uvs = new List<Vector2>();
            triangles = new List<int>();
        }
        /*public void AddVert(Vector3 pos) => vertices.Add(pos);
        public void AddUV(Vector2 uv) => uvs.Add(uv);
        public void AddTriangles(int index) => triangles.Add(index);*/
        public void Add(Vector3 pos) => vertices.Add(pos);
        public void Add(Vector2 uv) => uvs.Add(uv);
        public void Add(int index) => triangles.Add(index);

        public Mesh GetMesh(string meshName) {
            return new Mesh {
                name=meshName,
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray()
            };
        }
    }

    public void UpdateMeshWithTerrain() {
        bool terL, terR, hasRoad, hasSidewalk=false;
        ushort quadL = road.dataLeft.terrQuad;
        ushort quadR = road.dataRight.terrQuad;

        if(G.GetFlag(road.flag, (uint)MbdFile.Road.Flag.Terrain)) { 
            //仅地形，禁用左地形
            hasRoad=false; 
            terL=false;
        } else {
            //道路
            hasRoad=true;
            if(quadL>0) terL=true; else terL=false;
            if(G.GetFlag(road.flag, (uint)MbdFile.Road.Flag.CityRoad)) hasSidewalk=true;
            else hasSidewalk=false;
        }
        if(quadR>0) terR=true; else terR=false;


        MeshParams meshRoad = hasRoad ? new MeshParams() : null;
        MeshParams meshTerL = terL ? new MeshParams() : null;
        MeshParams meshTerR = terR ? new MeshParams() : null;

        Vector3 leftV, rightV;

        Vector3 thisPos;    //曲线上的点
        Vector3 fwd, right; //方向向量
        
        Vector3 c1 = nodeStart.position + nodeStart.tangent * nodeStart.length;
        Vector3 c2 = nodeEnd.position - nodeEnd.tangent * nodeStart.length;

        //地形相关
        Vector3 pos;   //地形网格点位置
        Vector3 terOffsetL, terOffsetR; //地形偏移
        float step, height;
        LoadWorld.TerrainProfile profileL = G.I.loadWorld.terProfile[road.dataLeft.terrType];
        LoadWorld.TerrainProfile profileR = G.I.loadWorld.terProfile[road.dataRight.terrType];

        for(int i = 0;i <= nodeStart.segmentNum;i++) {
            //每段从头到尾
            if(i == 0) {
                thisPos = nodeStart.position - transform.position;
                fwd = nodeStart.tangent;
            } else if(i == nodeStart.segmentNum) {
                thisPos = nodeEnd.position - transform.position;
                fwd = nodeEnd.tangent;
            } else {
                float t = i / (float)nodeStart.segmentNum;
                thisPos = CalculateBezierPoint(nodeStart.position,nodeEnd.position,c1,c2,t)-transform.position;
                fwd = CalculateBezierTangent(nodeStart.position,nodeEnd.position,c1,c2,t);
            }
            right = Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up) * fwd,Vector3.up).normalized;

            terOffsetL = Vector3.zero;
            terOffsetR = Vector3.zero;
            if(hasRoad) {
                //生成路面网格点
                leftV = thisPos - (nodeStart.width/2)*right;
                rightV = thisPos + (nodeStart.width/2)*right;
                meshRoad.Add(leftV);
                meshRoad.Add(rightV);
                terOffsetL = -(nodeStart.width/2)*right;
                terOffsetR = (nodeStart.width/2)*right;
                meshRoad.Add(new Vector2(0,i/(nodeStart.length/nodeStart.segmentNum)));
                meshRoad.Add(new Vector2(1,i/(nodeStart.length/nodeStart.segmentNum)));
            }

            if(hasSidewalk) {
                //生成人行道

            }

            if(terL) {
                //生成地形网格点 左
                meshTerL.Add(thisPos + terOffsetL);
                meshTerL.Add(new Vector2((thisPos + terOffsetL).x*0.5f,(thisPos + terOffsetL).z*0.5f));
                step = 0;
                for(int j = 1;j<=quadL;j++) {
                    //地形网格从内到外
                    step += profileL.step[Mathf.Min(j-1,profileL.step.Length-1)];
                    if(j>profileL.height.Length) height=0;
                    else height = profileL.height[Mathf.Min(j,profileL.height.Length-1)];
                    pos = thisPos + terOffsetL - step*right + road.dataLeft.terrCoef * height * Vector3.up;
                    meshTerL.Add(pos);
                    meshTerL.Add(new Vector2(pos.x*0.5f,pos.z*0.5f));
                }
            }

            if(terR) {
                //生成地形网格点 右
                meshTerR.Add(thisPos + terOffsetR);
                meshTerR.Add(new Vector2((thisPos + terOffsetR).x*0.5f,(thisPos + terOffsetR).z*0.5f));
                step = 0;
                for(int j = 1;j<=quadR;j++) {
                    //地形网格从内到外
                    step += profileR.step[Mathf.Min(j-1,profileR.step.Length-1)];
                    if(j>profileR.height.Length) height=0;
                    else height = profileR.height[Mathf.Min(j,profileR.height.Length-1)];

                    pos = thisPos + terOffsetR + step*right + road.dataRight.terrCoef * height * Vector3.up;
                    meshTerR.Add(pos);
                    meshTerR.Add(new Vector2(pos.x*0.5f,pos.z*0.5f));
                }
            }

        }

        //连接路面三角形
        int terIndex;
        for(int index = 1; index<=nodeStart.segmentNum; index++) {
            if(hasRoad) {
                meshRoad.Add(index*2);
                meshRoad.Add(index*2 +1);
                meshRoad.Add(index*2 -2);
                meshRoad.Add(index*2 +1);
                meshRoad.Add(index*2 -1);
                meshRoad.Add(index*2 -2);
            }

            if(terL) {
                //连接地形三角形 左
                for(int j = 1;j<=quadL;j++) {
                    terIndex = j + index * (quadL+1);
                    meshTerL.Add(terIndex);        //4
                    meshTerL.Add(terIndex-1);      //3
                    meshTerL.Add(terIndex-quadL-2);//0
                    meshTerL.Add(terIndex);        //4
                    meshTerL.Add(terIndex-quadL-2);//0
                    meshTerL.Add(terIndex-quadL-1);//1
                }
            }
            if(terR) {
                //连接地形三角形 右
                for(int j = 1;j<=quadR;j++) {
                    terIndex = j + index * (quadR+1);
                    meshTerR.Add(terIndex);        //4
                    meshTerR.Add(terIndex-quadR-2);//0
                    meshTerR.Add(terIndex-1);      //3
                    meshTerR.Add(terIndex);        //4
                    meshTerR.Add(terIndex-quadR-1);//1
                    meshTerR.Add(terIndex-quadR-2);//0
                }
            }
        }

        if(hasRoad) {
            Mesh meshBase = meshRoad.GetMesh(gameObject.name);
            meshBase.RecalculateNormals();  //计算法线
            filterBase=gameObject.AddComponent<MeshFilter>();
            renderBase=gameObject.AddComponent<MeshRenderer>();
            filterBase.mesh = meshBase;
            renderBase.material = G.I.dif;
            gameObject.AddComponent<MeshCollider>();
        }

        if(terL) {
            Material matL = G.I.loadWorld.terMats[road.dataLeft.terrMatNum];
            GameObject terrainL = new GameObject("terrainL");
            terrainL.transform.parent = transform;
            terrainL.transform.localPosition=Vector3.zero;
            Mesh meshL = meshTerL.GetMesh(terrainL.name);
            meshL.Optimize();
            meshL.RecalculateNormals();
            terrainL.AddComponent<MeshFilter>().mesh = meshL;
            terrainL.AddComponent<MeshRenderer>().material = matL;
            terrainL.AddComponent<MeshCollider>();
        }
        if(terR) {
            Material matR = G.I.loadWorld.terMats[road.dataRight.terrMatNum];
            GameObject terrainR = new GameObject("terrainR");
            terrainR.transform.parent = transform;
            terrainR.transform.localPosition=Vector3.zero;
            Mesh meshR = meshTerR.GetMesh(terrainR.name);
            meshR.Optimize();
            meshR.RecalculateNormals();
            terrainR.AddComponent<MeshFilter>().mesh = meshR;
            terrainR.AddComponent<MeshRenderer>().material = matR;
            terrainR.AddComponent<MeshCollider>();
        }
    }
}
