using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatInfoBox : MonoBehaviour {
    public class FloatInfo {
        public string headText;
        public string detailText;
        public Image icon;
        public Texture thumb;
        
        public FloatInfo(string head="",string detail="") {
            headText = head;
            detailText = detail;
        }
    }

    public Text headText;
    public Text detailText;
    public Image icon;
    public RawImage thumb;

    void Update() {
        //当前对象始终面向摄像机。
        transform.LookAt(Camera.main.transform.position);
        transform.Rotate(0, 180, 0);
    }

    public void LoadInfo(FloatInfo fi) {
        headText.text = fi.headText;
        detailText.text = fi.detailText;
        thumb.texture = fi.thumb;
    }

}
