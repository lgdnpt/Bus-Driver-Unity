using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusDriverConvert : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }


    [ContextMenu("cnm")]
    void printa() {
        Quaternion q = new Quaternion(0.0065697282f,0.5129977f,-0.005286964f,0.85834854f);
        Matrix4x4 rot = new Matrix4x4();
        rot.SetTRS(new Vector3(0,0,0),q,new Vector3(1,1,1));

        Matrix4x4 matrixL2R = new Matrix4x4(
            new Vector4(1,0,0,0),
            new Vector4(0,-1,0,0),
            new Vector4(0,0,1,0),
            new Vector4(0,0,0,1)
        );
        Matrix4x4 matrixL2R2 = new Matrix4x4(
            new Vector4(1,0,0,0),
            new Vector4(0,0,1,0),
            new Vector4(0,1,0,0),
            new Vector4(0,0,0,1)
        );

        Matrix4x4 matrix = matrixL2R2*rot;

        Vector4 vy = matrix.GetColumn(1);
        Vector4 vz = matrix.GetColumn(2);
        Quaternion q2 = Quaternion.LookRotation(new Vector3(vz.x,vz.y,vz.z),new Vector3(vy.x,vy.y,vz.z));

        print("("+q.eulerAngles.x+","+q.eulerAngles.y+","+q.eulerAngles.z+")");
        print("("+q2.eulerAngles.x+","+q2.eulerAngles.y+","+q2.eulerAngles.z+")");

        transform.rotation=q;
        print(transform.forward);
    }

    [ContextMenu("转四元数")]
    void rot2quat() {
        print(transform.rotation);
    }

}
