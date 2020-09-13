using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using fs;

public class UIFlowControl : MonoBehaviour {
    public RawImage background;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void LoadIntro() {
        SiiNunit sii = new SiiNunit(G.I.BasePath + "/ui/intro.sii");

        sii.unit.TryGetValue(".entity1",out SiiNunit.Unit unit);
        if(unit!=null) {
            if(unit is SiiNunit.UI_Text) print("check");
            unit.item.TryGetValue("name",out string unitName);
            unit.item.TryGetValue("text",out string unitText);
            print("Render " + unitName);
            print("ImagePath " + unitText);

            string[] sp = unitText.Split(' ');
            Dictionary<string,string> dic = new Dictionary<string,string>();
            foreach(string spe in sp) {
                string[] spsp = spe.Split('=');
                if(spsp.Length!=2) continue;
                else dic.Add(spsp[0],spsp[1]);
            }
            dic.TryGetValue("src",out string imageSrc);
            dic.TryGetValue("color",out string imageColor);

            print(G.I.BasePath+imageSrc);
            print(imageColor);

            MatFile mat = new MatFile(imageSrc);
            TobjFile tobj = new TobjFile(mat.texturePath[0]);
            background.texture=tobj.texture;

        } else {
            print("null");
        }
    }
}
