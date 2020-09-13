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
    public BezierNode[] nodes;
    
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

    void DrawCurve(bool fixedLength) {
        if(nodes.Length<=1) {
            Debug.LogWarning("有内鬼|"+gameObject.name);
            return;
        }

        //计算位置
        if(!fixedLength) {
            for(int i = 0;i<controlPoints.Length;i++) {
                nodes[i].position=controlPoints[i].position;
            }
        }
        

        Gizmos.color = Color.red;
        Vector3 ppp1 = nodes[1].position-nodes[0].position, ppp2;            //计算切线和长度

        for(int i = 0;i<nodes.Length-1;i++) {
            if(!fixedLength) {
                ppp2 = ppp1;
                ppp1 = nodes[i+1].position-nodes[i].position;
                nodes[i].length=Distance(nodes[i].position,nodes[i+1].position)/3;//Vector3.Distance(nodes[i+1].position,nodes[i].position)/4;
                nodes[i].tangent=(ppp1.normalized+ppp2.normalized).normalized;
            }
            nodes[i].segmentNum=(uint)(nodes[i].length*optimize);

            //画切线
            Gizmos.DrawLine(nodes[i].position,nodes[i].position+(nodes[i].length*nodes[i].tangent));
        }

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
    }

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


    public void NodeInit() {
        Vector3 ppp1 = nodes[1].position-nodes[0].position, ppp2;            //计算切线和长度
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
    }
    private Vector3 start, end;
    public void UpdateMesh() {
        //初始化节点
        if(!fixedLength)
            NodeInit();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        Vector3 leftV, rightV, left;
        Vector3 c1, c2, fwd;
        int faceCount = 0;
        start = nodes[0].position-transform.position;
        for(int nodeIndex = 0;nodeIndex<nodes.Length-1;nodeIndex++) {
            //遍历每一段
            for(int i = 1;i <= nodes[nodeIndex].segmentNum;i++) {
                //每段从头到尾
                float t = i / (float)nodes[nodeIndex].segmentNum;

                c1 = nodes[nodeIndex].position+ nodes[nodeIndex].tangent *nodes[nodeIndex].length;
                c2 = nodes[nodeIndex+1].position- nodes[nodeIndex+1].tangent *nodes[nodeIndex].length;
                end = CalculateBezierPoint(nodes[nodeIndex].position,nodes[nodeIndex+1].position,c1,c2,t)-transform.position;
                fwd= CalculateBezierTangent(nodes[nodeIndex].position,nodes[nodeIndex+1].position,c1,c2,t);
                left=Vector3.ProjectOnPlane(Quaternion.AngleAxis(-90,Vector3.up)*fwd,Vector3.up).normalized;
                
                //画线(生成网格点)
                leftV=start+(nodes[nodeIndex].width/2)*left;
                rightV=start-(nodes[nodeIndex].width/2)*left;
                vertices.Add(leftV);
                vertices.Add(rightV);
                uvs.Add(new Vector2(0,0));
                uvs.Add(new Vector2(0,1));
                faceCount++;
                start = end;
            }
        }
        if(nodes.Length>1) { }
        fwd= nodes[nodes.Length-1].tangent;
        left=Vector3.ProjectOnPlane(Quaternion.AngleAxis(-90,Vector3.up)*fwd,Vector3.up).normalized;
        leftV=start+(nodes[nodes.Length-1].width/2)*left;
        rightV=start-(nodes[nodes.Length-1].width/2)*left;
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
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();*/

        filterBase=gameObject.AddComponent<MeshFilter>();
        renderBase=gameObject.AddComponent<MeshRenderer>();

        filterBase.mesh = meshBase;
        renderBase.material = GlobalBusDriver.Instance.dif;
        gameObject.AddComponent<MeshCollider>();
    }
    

    public void UpadteTerrain() {
        //for(int i = 0;i<nodes.Length-1;i++) {
        //    nodes[i].segmentNum=(uint)(nodes[i].length*optimize);
        //}


        GameObject terrain = new GameObject("terrain");
        terrain.transform.parent = transform;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector3 pos;
        Vector3 c1, c2, fwd, right;
        LoadWorld.TerrainProfile profile = GlobalBusDriver.Instance.loadWorld.terProfile[nodes[0].road.dataRight.terrType];

        //添加第一条边
        start = nodes[0].position-terrain.transform.position;
        fwd= nodes[0].tangent;
        right=Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up)*fwd,Vector3.up).normalized;
        vertices.Add(start);

        float step = 0, height = 0;
        for(int j = 1;j<=nodes[0].road.dataRight.terrQuad;j++) {
            //地形网格从内到外
            step+=profile.step[Mathf.Min(j-1,profile.step.Length-1)];
            height+=profile.height[Mathf.Min(j,profile.height.Length-1)];
            pos=start+ step*right + nodes[0].road.dataRight.terrCoef*height*Vector3.up;
            vertices.Add(pos);
        }

        ushort quad;
        for(int nodeIndex = 0;nodeIndex<nodes.Length-1;nodeIndex++) {

            //关键段从头到尾
            quad = nodes[nodeIndex].road.dataRight.terrQuad;
            profile = GlobalBusDriver.Instance.loadWorld.terProfile[nodes[nodeIndex].road.dataRight.terrType];

            c1 = nodes[nodeIndex].position+ nodes[nodeIndex].tangent *nodes[nodeIndex].length;
            c2 = nodes[nodeIndex+1].position- nodes[nodeIndex+1].tangent *nodes[nodeIndex].length;
            for(int i = 1;i <= nodes[nodeIndex].segmentNum;i++) {
                //插值段从头到尾
                float t = i / (float)nodes[nodeIndex].segmentNum;
                end = CalculateBezierPoint(nodes[nodeIndex].position,nodes[nodeIndex+1].position,c1,c2,t)-terrain.transform.position;
                fwd= CalculateBezierTangent(nodes[nodeIndex].position,nodes[nodeIndex+1].position,c1,c2,t);
                right=Vector3.ProjectOnPlane(Quaternion.AngleAxis(90,Vector3.up)*fwd,Vector3.up).normalized;

                pos=end;
                vertices.Add(pos);

                step = 0; height = 0;
                Debug.Log(nodes[nodeIndex].road.dataRight.terrCoef);
                for(int j = 1; j<=quad; j++) {
                    //地形网格从内到外
                    step+=profile.step[Mathf.Min(j-1,profile.step.Length-1)];
                    height+=profile.height[Mathf.Min(j,profile.height.Length-1)];

                    pos=end+ step*right + nodes[nodeIndex].road.dataRight.terrCoef*height*Vector3.up;
                    int vIndex = vertices.Count;
                    vertices.Add(pos);
                    triangles.Add(vIndex);
                    triangles.Add(vIndex-quad-2);
                    triangles.Add(vIndex-1);
                    triangles.Add(vIndex);
                    triangles.Add(vIndex-quad-1);
                    triangles.Add(vIndex-quad-2);
                }
                start = end;
            }
        }



        Mesh mesh = new Mesh {
            name=terrain.name,
            vertices = vertices.ToArray(),
            //uv = uvs.ToArray(),
            triangles = triangles.ToArray() //三角面
        };
        mesh.Optimize();
        mesh.RecalculateNormals();
        
        terrain.AddComponent<MeshFilter>().mesh = mesh;
        terrain.AddComponent<MeshRenderer>().material = GlobalBusDriver.Instance.dif;
        terrain.AddComponent<MeshCollider>();
    }

}