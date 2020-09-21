using System.Collections;
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
        bool terL, terR, hasRoad;
        bool hasSide=false, hasSideL=false, hasSideR=false, hasCenter=false;
        ushort quadL = road.dataLeft.terrQuad;
        ushort quadR = road.dataRight.terrQuad;

        float size=nodeStart.width, offset=0;

        hasRoad = !G.GetFlag(road.flag, (uint)MbdFile.Road.Flag.Terrain);
        if(hasRoad) {
            //道路
            terL = quadL>0;
            hasSide = G.GetFlag(road.flag, (uint)MbdFile.Road.Flag.CityRoad);
            size = G.I.loadWorld.road[road.roadNum].roadSize;
            offset = G.I.loadWorld.road[road.roadNum].roadOffset;
            hasCenter = offset > 0.1f;
        } else {
            //仅地形，禁用左地形
            terL=false;
        }
        terR = quadR>0;

        MeshParams meshRoad = hasRoad ? new MeshParams() : null;
        MeshParams meshCenter = hasCenter ? new MeshParams() : null;
        MeshParams meshTerL = terL ? new MeshParams() : null;
        MeshParams meshTerR = terR ? new MeshParams() : null;
        MeshParams meshSide = hasSide ? new MeshParams() : null;
        MeshParams meshSideLP = hasSide ? new MeshParams() : null;
        MeshParams meshSideRP = hasSide ? new MeshParams() : null;
        
        //曲线相关
        Vector3 thisPos;    //曲线上的点
        Vector3 c1 = nodeStart.position + nodeStart.tangent * nodeStart.length;
        Vector3 c2 = nodeEnd.position - nodeEnd.tangent * nodeStart.length;
        Vector3 fwd, right; //方向向量

        //道路相关
        Vector3 leftV, rightV;

        //人行道相关
        float widthL=0f, widthR=0f;
        if(hasSide) {
            widthL = road.SidewalkWidthL;
            widthR = road.SidewalkWidthR;
            if(widthL>0f) hasSideL=true;
            if(widthR>0f) hasSideR=true;
            print("人行道 左:"+widthL+"\t|右"+widthR);
        }

        //地形相关
        Vector3 pos;   //地形网格点位置
        Vector3 offsetL, offsetR; //地形偏移
        float step, height;
        LoadWorld.TerrainProfile profileL = G.I.loadWorld.terProfile[road.dataLeft.terrType];
        LoadWorld.TerrainProfile profileR = G.I.loadWorld.terProfile[road.dataRight.terrType];

        //=====================================================
        //生成网格顶点
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

            offsetL = Vector3.zero;
            offsetR = Vector3.zero;
            if(hasRoad) {
                if(hasCenter) {
                    //生成隔离带网格
                    leftV = thisPos - offset*right;
                    rightV = thisPos + offset*right;
                    meshCenter.Add(leftV);
                    meshCenter.Add(rightV);
                    meshCenter.Add(new Vector2(leftV.x *0.25f, leftV.z *0.25f));
                    meshCenter.Add(new Vector2(rightV.x *0.25f, rightV.z *0.25f));

                    //生成左右路面网格
                    meshRoad.Add(thisPos - offset*right);
                    meshRoad.Add(thisPos - (offset+size)*right);
                    meshRoad.Add(thisPos + offset*right);
                    meshRoad.Add(thisPos + (offset+size)*right);
                    meshRoad.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    meshRoad.Add(new Vector2(1, i/(nodeStart.length/nodeStart.segmentNum)));
                    meshRoad.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    meshRoad.Add(new Vector2(1, i/(nodeStart.length/nodeStart.segmentNum)));

                } else {
                    //生成路面网格
                    meshRoad.Add(thisPos - (nodeStart.width/2)*right);
                    meshRoad.Add(thisPos + (nodeStart.width/2)*right);
                    meshRoad.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    meshRoad.Add(new Vector2(1, i/(nodeStart.length/nodeStart.segmentNum)));
                }

                offsetL = -nodeStart.width/2*right;
                offsetR = nodeStart.width/2*right;
                if(hasSide) {
                    //左路牙
                    meshSide.Add(thisPos + offsetL);
                    meshSide.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    offsetL += 0.2f * Vector3.up;
                    meshSide.Add(thisPos + offsetL);
                    meshSide.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    offsetL -= 0.2f * right;
                    meshSide.Add(thisPos + offsetL);
                    meshSide.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));

                    //右路牙
                    meshSide.Add(thisPos + offsetR);
                    meshSide.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    offsetR += 0.2f * Vector3.up;
                    meshSide.Add(thisPos + offsetR);
                    meshSide.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    offsetR += 0.2f * right;
                    meshSide.Add(thisPos + offsetR);
                    meshSide.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));

                    //生成人行道
                    if(hasSideL) {
                        //左平面
                        meshSideLP.Add(thisPos + offsetL);
                        meshSideLP.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                        offsetL -= widthL*right;
                        meshSideLP.Add(thisPos + offsetL);
                        meshSideLP.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    }
                    if(hasSideR) {
                        //右平面
                        meshSideRP.Add(thisPos + offsetR);
                        meshSideRP.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                        offsetR += widthR*right;
                        meshSideRP.Add(thisPos + offsetR);
                        meshSideRP.Add(new Vector2(0, i/(nodeStart.length/nodeStart.segmentNum)));
                    }
                }
            }


            if(terL) {
                //生成地形网格点 左
                meshTerL.Add(thisPos + offsetL);
                meshTerL.Add(new Vector2((thisPos + offsetL).x*0.5f,(thisPos + offsetL).z*0.5f));
                step = 0;
                for(int j = 1;j<=quadL;j++) {
                    //地形网格从内到外
                    step += profileL.step[Mathf.Min(j-1,profileL.step.Length-1)];
                    if(j>profileL.height.Length) height=0;
                    else height = profileL.height[Mathf.Min(j,profileL.height.Length-1)];
                    pos = thisPos + offsetL - step*right + road.dataLeft.terrCoef * height * Vector3.up;
                    meshTerL.Add(pos);
                    meshTerL.Add(new Vector2(pos.x*0.5f,pos.z*0.5f));
                }
            }

            if(terR) {
                //生成地形网格点 右
                meshTerR.Add(thisPos + offsetR);
                meshTerR.Add(new Vector2((thisPos + offsetR).x*0.5f,(thisPos + offsetR).z*0.5f));
                step = 0;
                for(int j = 1;j<=quadR;j++) {
                    //地形网格从内到外
                    step += profileR.step[Mathf.Min(j-1,profileR.step.Length-1)];
                    if(j>profileR.height.Length) height=0;
                    else height = profileR.height[Mathf.Min(j,profileR.height.Length-1)];

                    pos = thisPos + offsetR + step*right + road.dataRight.terrCoef * height * Vector3.up;
                    meshTerR.Add(pos);
                    meshTerR.Add(new Vector2(pos.x*0.5f,pos.z*0.5f));
                }
            }

        }

        //=====================================================
        //连接三角形
        int terIndex;
        for(int index = 1; index<=nodeStart.segmentNum; index++) {
            if(hasRoad) {
                if(hasCenter) {
                    //连接隔离带三角形
                    meshCenter.Add(index*2);
                    meshCenter.Add(index*2 +1);
                    meshCenter.Add(index*2 -2);
                    meshCenter.Add(index*2 +1);
                    meshCenter.Add(index*2 -1);
                    meshCenter.Add(index*2 -2);

                    //左路面
                    meshRoad.Add(index*4);
                    meshRoad.Add(index*4 -4);
                    meshRoad.Add(index*4 -3);
                    meshRoad.Add(index*4);
                    meshRoad.Add(index*4 -3);
                    meshRoad.Add(index*4 +1);
                    //右路面
                    meshRoad.Add(index*4 +3);
                    meshRoad.Add(index*4 -1);
                    meshRoad.Add(index*4 -2);
                    meshRoad.Add(index*4 +3);
                    meshRoad.Add(index*4 -2);
                    meshRoad.Add(index*4 +2);

                } else {
                    //连接路面三角形
                    meshRoad.Add(index*2);
                    meshRoad.Add(index*2 +1);
                    meshRoad.Add(index*2 -2);
                    meshRoad.Add(index*2 +1);
                    meshRoad.Add(index*2 -1);
                    meshRoad.Add(index*2 -2);
                }

                if(hasSide) {
                    //连接人行道三角形
                    //左_侧面
                    meshSide.Add(index*6);
                    meshSide.Add(index*6 -6);
                    meshSide.Add(index*6 -5);
                    meshSide.Add(index*6);
                    meshSide.Add(index*6 -5);
                    meshSide.Add(index*6 +1);
                    //左_上面
                    meshSide.Add(index*6 +1);
                    meshSide.Add(index*6 -5);
                    meshSide.Add(index*6 -4);
                    meshSide.Add(index*6 +1);
                    meshSide.Add(index*6 -4);
                    meshSide.Add(index*6 +2);

                    //右_侧面
                    meshSide.Add(index*6 +4);
                    meshSide.Add(index*6 -2);
                    meshSide.Add(index*6 -3);
                    meshSide.Add(index*6 +4);
                    meshSide.Add(index*6 -3);
                    meshSide.Add(index*6 +3);
                    //右_上面
                    meshSide.Add(index*6 +5);
                    meshSide.Add(index*6 -1);
                    meshSide.Add(index*6 -2);
                    meshSide.Add(index*6 +5);
                    meshSide.Add(index*6 -2);
                    meshSide.Add(index*6 +4);

                    if(hasSideL) {
                        //左_延伸面
                        meshSideLP.Add(index*2);
                        meshSideLP.Add(index*2 -2);
                        meshSideLP.Add(index*2 +1);
                        meshSideLP.Add(index*2 +1);
                        meshSideLP.Add(index*2 -2);
                        meshSideLP.Add(index*2 -1);
                    }
                    if(hasSideR) {
                        //右_延伸面
                        meshSideRP.Add(index*2);
                        meshSideRP.Add(index*2 +1);
                        meshSideRP.Add(index*2 -2);
                        meshSideRP.Add(index*2 +1);
                        meshSideRP.Add(index*2 -1);
                        meshSideRP.Add(index*2 -2);
                    }
                }
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

        //=====================================================
        //mesh生成
        if(hasRoad) {
            if(hasCenter) {
                //路面
                Mesh meshBase = meshRoad.GetMesh(gameObject.name);
                meshBase.RecalculateNormals();  //计算法线
                filterBase=gameObject.AddComponent<MeshFilter>();
                renderBase=gameObject.AddComponent<MeshRenderer>();
                filterBase.mesh = meshBase;
                renderBase.material = G.I.loadWorld.roadMats[road.matNum];
                gameObject.AddComponent<MeshCollider>();

                //隔离带
                GameObject centerObj = new GameObject("center");
                centerObj.transform.parent = transform;
                centerObj.transform.localPosition=Vector3.zero;
                Mesh meshCenterMesh = meshCenter.GetMesh(centerObj.name);
                meshCenterMesh.Optimize();
                meshCenterMesh.RecalculateNormals();
                centerObj.AddComponent<MeshFilter>().mesh = meshCenterMesh;
                centerObj.AddComponent<MeshRenderer>().material = G.I.loadWorld.terMats[road.centerNum];
                centerObj.AddComponent<MeshCollider>();

            } else {
                //路面
                Mesh meshBase = meshRoad.GetMesh(gameObject.name);
                meshBase.RecalculateNormals();  //计算法线
                filterBase=gameObject.AddComponent<MeshFilter>();
                renderBase=gameObject.AddComponent<MeshRenderer>();
                filterBase.mesh = meshBase;
                renderBase.material = G.I.loadWorld.roadMats[road.matNum];
                gameObject.AddComponent<MeshCollider>();
            }

            if(hasSide) {
                //路牙
                GameObject sideObj = new GameObject("side");
                sideObj.transform.parent = transform;
                sideObj.transform.localPosition=Vector3.zero;
                Mesh meshSideMesh = meshSide.GetMesh(sideObj.name);
                meshSideMesh.Optimize();
                meshSideMesh.RecalculateNormals();
                sideObj.AddComponent<MeshFilter>().mesh = meshSideMesh;
                sideObj.AddComponent<MeshRenderer>().material = G.I.loadWorld.roadSideMats[road.matNum];
                sideObj.AddComponent<MeshCollider>();

                if(hasSideL) {
                    //左人行道面
                    GameObject sideObjL = new GameObject("side");
                    sideObjL.transform.parent = transform;
                    sideObjL.transform.localPosition=Vector3.zero;
                    Mesh meshSideMeshL = meshSideLP.GetMesh(sideObjL.name);
                    meshSideMeshL.Optimize();
                    meshSideMeshL.RecalculateNormals();
                    sideObjL.AddComponent<MeshFilter>().mesh = meshSideMeshL;
                    sideObjL.AddComponent<MeshRenderer>().material = G.I.loadWorld.sidewalkMats[road.dataLeft.sidewalk];
                    sideObjL.AddComponent<MeshCollider>();
                }
                if(hasSideR) {
                    //右人行道面
                    GameObject sideObjR = new GameObject("side");
                    sideObjR.transform.parent = transform;
                    sideObjR.transform.localPosition=Vector3.zero;
                    Mesh meshSideMeshR = meshSideRP.GetMesh(sideObjR.name);
                    meshSideMeshR.Optimize();
                    meshSideMeshR.RecalculateNormals();
                    sideObjR.AddComponent<MeshFilter>().mesh = meshSideMeshR;
                    sideObjR.AddComponent<MeshRenderer>().material = G.I.loadWorld.sidewalkMats[road.dataRight.sidewalk];
                    sideObjR.AddComponent<MeshCollider>();
                }
            }
        }

        if(terL) {
            GameObject terrainL = new GameObject("terrainL");
            terrainL.transform.parent = transform;
            terrainL.transform.localPosition=Vector3.zero;
            Mesh meshL = meshTerL.GetMesh(terrainL.name);
            meshL.Optimize();
            meshL.RecalculateNormals();
            terrainL.AddComponent<MeshFilter>().mesh = meshL;
            terrainL.AddComponent<MeshRenderer>().material = G.I.loadWorld.terMats[road.dataLeft.terrMatNum];
            terrainL.AddComponent<MeshCollider>();
        }
        if(terR) {
            GameObject terrainR = new GameObject("terrainR");
            terrainR.transform.parent = transform;
            terrainR.transform.localPosition=Vector3.zero;
            Mesh meshR = meshTerR.GetMesh(terrainR.name);
            meshR.Optimize();
            meshR.RecalculateNormals();
            terrainR.AddComponent<MeshFilter>().mesh = meshR;
            terrainR.AddComponent<MeshRenderer>().material = G.I.loadWorld.terMats[road.dataRight.terrMatNum];
            terrainR.AddComponent<MeshCollider>();
        }
    }
}
