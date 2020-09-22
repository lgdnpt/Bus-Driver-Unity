using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using fs;

public class PMGLoader : MonoBehaviour {
    public static void LoadPMG(string pmdPath) => LoadPMG(pmdPath,new GameObject(pmdPath.Substring(pmdPath.LastIndexOf('/')+1).Replace(".pmd","")));
    public static void LoadPMG(string pmdPath,GameObject root) {

        //print("读取模型pmd:"+pmdPath);
        /*pmg = new PmgFile(pmgPath);
        pmd = new PmdFile(pmdPath);*/
        PmdFile pmd = new PmdFile(pmdPath);
        PmgFile pmg = pmd.pmgFile;


        //创建根物体
        for(int groupi = 0;groupi<pmg.groupCount;groupi++) {
            GameObject groupObj = new GameObject(pmg.groups[groupi].name.Text);
            groupObj.transform.parent=root.transform;

            if(pmg.groups[groupi].meshCount>0) {
                //读取mesh
                for(int meshi=0;meshi<pmg.groups[groupi].meshCount;meshi++) {
                    //print("读取组:"+pmg.groups[groupi].name.Text);
                    string meshName = pmg.groups[groupi].name.Text+" "+meshi;

                    GameObject meshObj = groupObj;
                    if(pmg.groups[groupi].meshCount>1 || pmg.groups[groupi].locatorCount>1) {
                        meshObj = new GameObject(meshName);
                        meshObj.transform.parent=groupObj.transform;
                    }

                    int meshID = pmg.groups[groupi].meshID + meshi;



                    //创建顶点和UV
                    Vector3[] vertices = new Vector3[pmg.meshes[meshID].vertsCount];
                    Vector2[] uv = new Vector2[pmg.meshes[meshID].vertsCount];
                    for(int i = 0;i<vertices.Length;i++) {
                        vertices[i]=new Vector3(pmg.meshes[meshID].vertex[i].x,pmg.meshes[meshID].vertex[i].y,-pmg.meshes[meshID].vertex[i].z);
                        uv[i] = new Vector2(pmg.meshes[meshID].uv[i].x,-pmg.meshes[meshID].uv[i].y);
                    }


                    //三角形index
                    int[] triangles = new int[pmg.meshes[meshID].triangleCount];
                    for(int index = 0;index<triangles.Length;index+=3) {
                        triangles[index] = pmg.meshes[meshID].triangle[index];
                        triangles[index+1] = pmg.meshes[meshID].triangle[index+1];
                        triangles[index+2] = pmg.meshes[meshID].triangle[index+2];
                    }

                    //创建mesh
                    Mesh mesh = new Mesh {
                        name=meshName,
                        vertices = vertices,
                        uv = uv,
                        triangles = triangles //三角面
                    };
                    mesh.RecalculateNormals();  //计算法线
                    MeshFilter meshFilter = meshObj.AddComponent<MeshFilter>();
                    meshFilter.mesh = mesh;

                    //碰撞体
                    switch(pmg.groups[groupi].name.Text) {
                        case "coll": meshObj.AddComponent<MeshCollider>(); break;
                        case "wcoll": meshObj.AddComponent<MeshCollider>(); break;
                        case "coll1": meshObj.AddComponent<MeshCollider>(); break;
                        case "centercol5m": meshObj.AddComponent<MeshCollider>(); break;
                        case "centercol15m": meshObj.AddComponent<MeshCollider>(); break;
                        case "startcol5m": meshObj.AddComponent<MeshCollider>(); break;
                        case "startcol15m": meshObj.AddComponent<MeshCollider>(); break;
                        case "endcol5m": meshObj.AddComponent<MeshCollider>(); break;
                        case "endcol15m": meshObj.AddComponent<MeshCollider>(); break;
                        default: {
                            //非碰撞体,读材质
                            int materialID = pmg.meshes[meshID].material;
                            string matPath = pmd.matPath[materialID];
                            if(matPath.IndexOf('/')<0) {
                                matPath = pmdPath.Substring(0, pmdPath.LastIndexOf('/')+1) + matPath;
                            }

                            //读取/应用材质
                            MeshRenderer renderer = meshObj.AddComponent<MeshRenderer>();
                            renderer.material = fs.Cache.LoadMat(matPath).Material;
                        }
                        break;
                    }
                }
            }
            if(pmg.groups[groupi].locatorCount>0) {
                //读取空物体
                for(int i = 0;i<pmg.groups[groupi].locatorCount;i++) {
                    PmgFile.Locator locator = pmg.locators[pmg.groups[groupi].locatorID+i];
                    GameObject obj = new GameObject(locator.name.Text);
                    obj.transform.parent=groupObj.transform;
                    obj.transform.localPosition=new Vector3(locator.position.x,locator.position.y,locator.position.z);
                    obj.transform.localRotation=Quaternion.Euler(locator.rotation.x,locator.rotation.y,locator.rotation.z);
                    obj.transform.localScale*=locator.scale;
                }
            }
        }

    }
}
