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
        public MbdFile.Road road;
    }

    public RoadType roadType;

    public Transform[] controlPoints;
    //public BezierNode[] nodes;
    public BezierNode nodeStart;
    public BezierNode nodeEnd;
    
    public MeshFilter filterBase;
    public MeshRenderer renderBase;
    public float optimize = 0.5f;
    public bool fixedLength = true;
    public bool show = false;

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

        for(int i = 0;i<nodes.Length-1;i++) {
            
        }
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
    private Vector3 start, end;
    public void UpdateMesh() {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3 leftV, rightV, left;
        Vector3 c1, c2, fwd;
        int faceCount = 0;
        start = nodeStart.position-transform.position;

        
        for(int i = 1;i <= nodeStart.segmentNum;i++) {
            //每段从头到尾
            float t = i / (float)nodeStart.segmentNum;

            c1 = nodeStart.position + nodeStart.tangent * nodeStart.length;
            c2 = nodeEnd.position - nodeEnd.tangent * nodeStart.length;

            end = CalculateBezierPoint(nodeStart.position, nodeEnd.position, c1, c2, t)-transform.position;
            if(i==1) fwd = nodeStart.tangent;
            else if(i==nodeStart.segmentNum) fwd = nodeEnd.tangent;
            else fwd = CalculateBezierTangent(nodeStart.position, nodeEnd.position, c1, c2, t);
            left = Vector3.ProjectOnPlane(Quaternion.AngleAxis(-90,Vector3.up)*fwd,Vector3.up).normalized;

            //画线(生成网格点)
            leftV = start + (nodeStart.width/2)*left;
            rightV = start - (nodeStart.width/2)*left;
            vertices.Add(leftV);
            vertices.Add(rightV);
            uvs.Add(new Vector2(0,0));
            uvs.Add(new Vector2(0,1));
            faceCount++;
            start = end;
        }

        
        fwd = nodeEnd.tangent;
        left = Vector3.ProjectOnPlane(Quaternion.AngleAxis(-90,Vector3.up)*fwd, Vector3.up).normalized;
        leftV = start + (nodeEnd.width/2)*left;
        rightV = start - (nodeEnd.width/2)*left;
        vertices.Add(leftV);
        vertices.Add(rightV);
        uvs.Add(new Vector2(0,0));
        uvs.Add(new Vector2(0,1));


        //三角形index
        int[] triangles = new int[faceCount*6];
        for(int index = 0;index<faceCount;index++) {
            triangles[index*6] = index*2;
            triangles[index*6+1] = index*2+3;
            triangles[index*6+2] = index*2+1;
            triangles[index*6+3] = index*2;
            triangles[index*6+4] = index*2+2;
            triangles[index*6+5] = index*2+3;
        }

        Mesh meshBase = new Mesh {
            name=gameObject.name,
            vertices = vertices.ToArray(),
            uv = uvs.ToArray(),
            triangles = triangles //三角面
        };
        meshBase.RecalculateNormals();  //计算法线
        /*
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();*/

        filterBase=gameObject.AddComponent<MeshFilter>();
        renderBase=gameObject.AddComponent<MeshRenderer>();

        filterBase.mesh = meshBase;
        renderBase.material = G.I.dif;
        gameObject.AddComponent<MeshCollider>();
    }
    

    public void UpadteTerrain2(Material mat) {
        GameObject terrain = new GameObject("terrain");
        //terrain.transform.position = transform.position;
        terrain.transform.parent = transform;
        //terrain.transform.localPosition=Vector3.zero;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector3 pos;
        Vector3 c1, c2, fwd, right;
        LoadWorld.TerrainProfile profile = G.I.loadWorld.terProfile[nodeStart.road.dataRight.terrType];

        //添加第一条边
        float step = 0, height = 0;
        start = nodeStart.position-terrain.transform.position;
        fwd= nodeStart.tangent;
        right=Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up)*fwd,Vector3.up).normalized;
        vertices.Add(start);
        uvs.Add(new Vector2(start.x,start.z));

        for(int j = 1;j<= nodeStart.road.dataRight.terrQuad;j++) {
            //地形网格从内到外
            step += profile.step[Mathf.Min(j-1,profile.step.Length-1)];
            height += profile.height[Mathf.Min(j,profile.height.Length-1)];
            pos = start + step*right + nodeStart.road.dataRight.terrCoef*height*Vector3.up;
            vertices.Add(pos);
            uvs.Add(new Vector2(pos.x,pos.z));
        }

        ushort quad;
        quad = nodeStart.road.dataRight.terrQuad;
        profile = G.I.loadWorld.terProfile[nodeStart.road.dataRight.terrType];

        c1 = nodeStart.position + nodeStart.tangent * nodeStart.length;
        c2 = nodeEnd.position- nodeEnd.tangent * nodeStart.length;
        for(int i = 1;i <= nodeStart.segmentNum;i++) {
            //插值段从头到尾
            float t = i / (float)nodeStart.segmentNum;
            end = CalculateBezierPoint(nodeStart.position, nodeEnd.position, c1,c2,t)-terrain.transform.position;

            if(i==1) fwd = nodeStart.tangent;
            else if(i==nodeStart.segmentNum) fwd = nodeEnd.tangent;
            else fwd = CalculateBezierTangent(nodeStart.position,nodeEnd.position,c1,c2,t);

            right = Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up)*fwd, Vector3.up).normalized;

            pos=end;
            vertices.Add(pos);
            uvs.Add(new Vector2(pos.x,pos.z));

            step = 0; height = 0;
            //Debug.Log(nodes[nodeIndex].road.dataRight.terrCoef);
            for(int j = 1; j<=quad; j++) {
                //地形网格从内到外
                step += profile.step[Mathf.Min(j-1, profile.step.Length-1)];
                height += profile.height[Mathf.Min(j, profile.height.Length-1)];

                pos = end + step*right + nodeStart.road.dataRight.terrCoef*height*Vector3.up;
                int vIndex = vertices.Count;
                vertices.Add(pos);
                uvs.Add(new Vector2(pos.x,pos.z));
                triangles.Add(vIndex);
                triangles.Add(vIndex-quad-2);
                triangles.Add(vIndex-1);
                triangles.Add(vIndex);
                triangles.Add(vIndex-quad-1);
                triangles.Add(vIndex-quad-2);
            }
            start = end;
        }
        //====================================
        //插值段从头到尾
        end = nodeEnd.position;
        fwd = nodeEnd.tangent;

        right = Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up)*fwd,Vector3.up).normalized;

        pos=end;
        vertices.Add(pos);
        uvs.Add(new Vector2(pos.x,pos.z));

        step = 0; height = 0;
        //Debug.Log(nodes[nodeIndex].road.dataRight.terrCoef);
        for(int j = 1;j<=quad;j++) {
            //地形网格从内到外
            step += profile.step[Mathf.Min(j-1,profile.step.Length-1)];
            height += profile.height[Mathf.Min(j,profile.height.Length-1)];

            pos = end + step*right + nodeStart.road.dataRight.terrCoef*height*Vector3.up;
            int vIndex = vertices.Count;
            vertices.Add(pos);
            uvs.Add(new Vector2(pos.x,pos.z));
            triangles.Add(vIndex);
            triangles.Add(vIndex-quad-2);
            triangles.Add(vIndex-1);
            triangles.Add(vIndex);
            triangles.Add(vIndex-quad-1);
            triangles.Add(vIndex-quad-2);
        }
        start = end;
        //====================================

        Mesh mesh = new Mesh {
            name=terrain.name,
            vertices = vertices.ToArray(),
            uv = uvs.ToArray(),
            triangles = triangles.ToArray() //三角面
        };
        mesh.Optimize();
        mesh.RecalculateNormals();
        
        terrain.AddComponent<MeshFilter>().mesh = mesh;
        terrain.AddComponent<MeshRenderer>().material = mat;
        terrain.AddComponent<MeshCollider>();
    }

    public void UpadteTerrain(Material mat, bool isRight=true) {
        ushort quad = nodeStart.road.dataRight.terrQuad;
        if(quad<=0) return;
        LoadWorld.TerrainProfile profile = G.I.loadWorld.terProfile[nodeStart.road.dataRight.terrType];

        GameObject terrain = new GameObject("terrain");
        terrain.transform.parent = transform;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector3 thisPos;    //曲线上的点
        Vector3 pos;        //地形网格的点
        Vector3 fwd, right; //方向向量
        
        Vector3 c1 = nodeStart.position + nodeStart.tangent * nodeStart.length;
        Vector3 c2 = nodeEnd.position- nodeEnd.tangent * nodeStart.length;

        float step, height;
        //插值段从头到尾
        for(int i = 0; i <= nodeStart.segmentNum; i++) {
            //首尾/中间段 -> 坐标/前向单位向量
            if(i == 0) {
                thisPos = nodeStart.position - terrain.transform.position;
                fwd = nodeStart.tangent;
            }else if(i == nodeStart.segmentNum) {
                thisPos = nodeEnd.position - terrain.transform.position;
                fwd = nodeEnd.tangent;
            }else {
                float t = i / (float)nodeStart.segmentNum;
                thisPos = CalculateBezierPoint(nodeStart.position,nodeEnd.position,c1,c2,t)-terrain.transform.position;
                fwd = CalculateBezierTangent(nodeStart.position,nodeEnd.position,c1,c2,t);
            }
            //右侧方向向量
            right = Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up) * fwd,Vector3.up).normalized;

            pos = thisPos;
            vertices.Add(pos);
            uvs.Add(new Vector2(pos.x,pos.z));

            step = 0;
            for(int j = 1;j<=quad;j++) {
                //地形网格从内到外
                step += profile.step[Mathf.Min(j-1, profile.step.Length-1)];
                if(j>profile.height.Length) height=0;
                else height = profile.height[Mathf.Min(j,profile.height.Length-1)];

                pos = thisPos + step*right + nodeStart.road.dataRight.terrCoef * height * Vector3.up;
                vertices.Add(pos);
                uvs.Add(new Vector2(pos.x,pos.z));
            }
        }

        //连接三角形
        for(int i = 1; i <= nodeStart.segmentNum;i++) {
            for(int j = 1;j<=quad;j++) {
                int thisIndex = j + i*(quad+1);
                triangles.Add(thisIndex);       //4
                triangles.Add(thisIndex-quad-2);//0
                triangles.Add(thisIndex-1);     //3
                triangles.Add(thisIndex);       //4
                triangles.Add(thisIndex-quad-1);//1
                triangles.Add(thisIndex-quad-2);//0
            }
        }


        Mesh mesh = new Mesh {
            name=terrain.name,
            vertices = vertices.ToArray(),
            uv = uvs.ToArray(),
            triangles = triangles.ToArray()
        };
        mesh.Optimize();
        mesh.RecalculateNormals();

        terrain.AddComponent<MeshFilter>().mesh = mesh;
        terrain.AddComponent<MeshRenderer>().material = mat;
        terrain.AddComponent<MeshCollider>();
    }

    public void UpdateMeshWithTerrain() {
        bool terL, terR, hasRoad;
        ushort quadL = nodeStart.road.dataLeft.terrQuad;
        ushort quadR = nodeStart.road.dataRight.terrQuad;

        if(G.GetFlag(nodeStart.road.flag,0x00010000)) { 
            //仅地形，禁用左地形
            hasRoad=false; 
            terL=false;
        } else {
            //道路
            hasRoad=true;
            if(quadL>0) terL=true; else terL=false;
        }
        if(quadR>0) terR=true; else terR=false;


        List<Vector3> vertices = hasRoad ? new List<Vector3>() : null;
        List<Vector2> uvs = hasRoad ? new List<Vector2>() : null;
        List<int> triangles = hasRoad ? new List<int>() : null;

        List<Vector3> verticesL = terL ? new List<Vector3>() : null;
        List<Vector2> uvsL = terL ? new List<Vector2>() : null;
        List<int> trianglesL = terL ? new List<int>() : null;

        List<Vector3> verticesR = terR ? new List<Vector3>() : null;
        List<Vector2> uvsR = terR ? new List<Vector2>() : null;
        List<int> trianglesR = terR ? new List<int>() : null;

        Vector3 leftV, rightV;

        Vector3 thisPos;    //曲线上的点
        Vector3 fwd, right; //方向向量
        
        Vector3 c1 = nodeStart.position + nodeStart.tangent * nodeStart.length;
        Vector3 c2 = nodeEnd.position - nodeEnd.tangent * nodeStart.length;

        //地形相关
        Vector3 pos;   //地形网格点位置
        Vector3 terOffsetL, terOffsetR; //地形偏移
        float step, height;
        LoadWorld.TerrainProfile profileL = G.I.loadWorld.terProfile[nodeStart.road.dataLeft.terrType];
        LoadWorld.TerrainProfile profileR = G.I.loadWorld.terProfile[nodeStart.road.dataRight.terrType];

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
                vertices.Add(leftV);
                vertices.Add(rightV);
                terOffsetL = -(nodeStart.width/2)*right;
                terOffsetR = (nodeStart.width/2)*right;
                uvs.Add(new Vector2(0,i%2==0 ? 0 : 1));
                uvs.Add(new Vector2(1,i%2==0 ? 0 : 1));
            }

            if(terL) {
                //生成地形网格点 左
                verticesL.Add(thisPos + terOffsetL);
                uvsL.Add(new Vector2(thisPos.x,thisPos.z));
                step = 0;
                for(int j = 1;j<=quadL;j++) {
                    //地形网格从内到外
                    step += profileL.step[Mathf.Min(j-1,profileL.step.Length-1)];
                    if(j>profileL.height.Length) height=0;
                    else height = profileL.height[Mathf.Min(j,profileL.height.Length-1)];
                    pos = thisPos + terOffsetL - step*right + nodeStart.road.dataLeft.terrCoef * height * Vector3.up;
                    verticesL.Add(pos);
                    uvsL.Add(new Vector2(pos.x,pos.z));
                }
            }

            if(terR) {
                //生成地形网格点 右
                verticesR.Add(thisPos + terOffsetR);
                uvsR.Add(new Vector2(thisPos.x,thisPos.z));
                step = 0;
                for(int j = 1;j<=quadR;j++) {
                    //地形网格从内到外
                    step += profileR.step[Mathf.Min(j-1,profileR.step.Length-1)];
                    if(j>profileR.height.Length) height=0;
                    else height = profileR.height[Mathf.Min(j,profileR.height.Length-1)];

                    pos = thisPos + terOffsetR + step*right + nodeStart.road.dataRight.terrCoef * height * Vector3.up;
                    verticesR.Add(pos);
                    uvsR.Add(new Vector2(pos.x,pos.z));
                }
            }

        }

        //连接路面三角形
        int terIndex;
        for(int index = 1; index<=nodeStart.segmentNum; index++) {
            if(hasRoad) {
                triangles.Add(index*2);
                triangles.Add(index*2 +1);
                triangles.Add(index*2 -2);
                triangles.Add(index*2 +1);
                triangles.Add(index*2 -1);
                triangles.Add(index*2 -2);
            }

            if(terL) {
                //连接地形三角形 左
                for(int j = 1;j<=quadL;j++) {
                    terIndex = j + index * (quadL+1);
                    trianglesL.Add(terIndex);        //4
                    trianglesL.Add(terIndex-1);      //3
                    trianglesL.Add(terIndex-quadL-2);//0
                    trianglesL.Add(terIndex);        //4
                    trianglesL.Add(terIndex-quadL-2);//0
                    trianglesL.Add(terIndex-quadL-1);//1
                }
            }
            if(terR) {
                //连接地形三角形 右
                for(int j = 1;j<=quadR;j++) {
                    terIndex = j + index * (quadR+1);
                    trianglesR.Add(terIndex);        //4
                    trianglesR.Add(terIndex-quadR-2);//0
                    trianglesR.Add(terIndex-1);      //3
                    trianglesR.Add(terIndex);        //4
                    trianglesR.Add(terIndex-quadR-1);//1
                    trianglesR.Add(terIndex-quadR-2);//0
                }
            }
        }

        if(hasRoad) {
            Mesh meshBase = new Mesh {
                name=gameObject.name,
                vertices = vertices.ToArray(),
                uv = uvs.ToArray(),
                triangles = triangles.ToArray() //三角面
            };
            meshBase.RecalculateNormals();  //计算法线
            filterBase=gameObject.AddComponent<MeshFilter>();
            renderBase=gameObject.AddComponent<MeshRenderer>();
            filterBase.mesh = meshBase;
            renderBase.material = G.I.dif;
            gameObject.AddComponent<MeshCollider>();
        }

        if(terL) {
            Material matL = G.I.loadWorld.terMats[nodeStart.road.dataLeft.terrMatNum];
            GameObject terrainL = new GameObject("terrainL");
            terrainL.transform.parent = transform;
            terrainL.transform.localPosition=Vector3.zero;
            Mesh meshL = new Mesh {
                name=terrainL.name,
                vertices = verticesL.ToArray(),
                uv = uvsL.ToArray(),
                triangles = trianglesL.ToArray()
            };
            meshL.Optimize();
            meshL.RecalculateNormals();
            terrainL.AddComponent<MeshFilter>().mesh = meshL;
            terrainL.AddComponent<MeshRenderer>().material = matL;
            terrainL.AddComponent<MeshCollider>();
        }
        if(terR) {
            Material matR = G.I.loadWorld.terMats[nodeStart.road.dataRight.terrMatNum];
            GameObject terrainR = new GameObject("terrainR");
            terrainR.transform.parent = transform;
            terrainR.transform.localPosition=Vector3.zero;
            Mesh meshR = new Mesh {
                name=terrainR.name,
                vertices = verticesR.ToArray(),
                uv = uvsR.ToArray(),
                triangles = trianglesR.ToArray()
            };
            meshR.Optimize();
            meshR.RecalculateNormals();
            terrainR.AddComponent<MeshFilter>().mesh = meshR;
            terrainR.AddComponent<MeshRenderer>().material = matR;
            terrainR.AddComponent<MeshCollider>();
        }
    }
}
