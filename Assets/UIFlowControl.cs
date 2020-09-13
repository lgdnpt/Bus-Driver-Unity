using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using fs;
using System.Text.RegularExpressions;

public class UIFlowControl : MonoBehaviour {
    public Canvas canvas;
    public RawImage rawImage;
    public InputField input;

    Vector2 factor;

    public void Start() {
        factor=new Vector2(Screen.width/1024f,Screen.height/768f);
    }

    public void test() {
        Match matchImg = Regex.Match("<align hstyle=center vstyle=center><img src=/material/ui/white.mat color=FF000000 xscale=stretch yscale=stretch></align>",@"<img[^>]*?>");
        Match matchSrc = Regex.Match(matchImg.ToString(),input.text);//"src=[^>]*\\s"
        //Match matchColor = Regex.Match(matchImg.ToString(),"color=[^>]*\\s");
        print(matchSrc.ToString());
    }

    public void LoadIntro() {
        SiiNunit sii = new SiiNunit(G.BasePath + "/ui/intro.sii");

        //sii.unit.TryGetValue(".entity3",out SiiNunit.Unit unit);
        LoadUIImage(sii,".entity3");
        LoadUIImage(sii,".entity11");
    }

    void LoadUIImage(SiiNunit sii, string unitName) {
        sii.unit.TryGetValue(unitName,out SiiNunit.Unit unit);
        if(unit!=null) {
            if(unit is SiiNunit.UI_Text) print("check");
            unit.item.TryGetValue("name",out string itemName);
            unit.item.TryGetValue("text",out string itemText);
            print("Render " + itemName);
            print("ImagePath " + itemText);

            SiiNunit.UI_Text ui = (SiiNunit.UI_Text)unit;
            RawImage imgBg = Instantiate(rawImage,canvas.transform);
            ApplyCoords(imgBg.rectTransform,ui);
            ApplyImageWithImgTag(imgBg,itemText);

        } else {
            print("null");
        }
    }

    void ApplyImageWithImgTag(RawImage image, string tag) {
        /*string[] sp = tag.Split(' ');
        Dictionary<string,string> dic = new Dictionary<string,string>();
        foreach(string spe in sp) {
            string[] spsp = spe.Split('=');
            if(spsp.Length!=2) continue;
            else dic.Add(spsp[0],spsp[1]);
        }
        dic.TryGetValue("src",out string imageSrc);
        dic.TryGetValue("color",out string imageColor);*/
        Match matchImg = Regex.Match(tag,@"<img[^>]*?>");
        Debug.LogWarning(matchImg.Value);
        Match matchSrc = Regex.Match(matchImg.ToString(),"src=[^>]*?(\\s|>)");
        Debug.LogWarning(matchSrc.Value);
        Match matchColor = Regex.Match(matchImg.ToString(),"color=[^>]*?(\\s|>)");
        Debug.LogWarning(matchColor.Value);

        string imageSrc = matchSrc.Value;
        string imageColor = matchColor.Value;

        if(!string.IsNullOrEmpty(imageSrc)) {
            imageSrc = imageSrc.Substring(4,imageSrc.Length-5);
            Debug.LogWarning(imageSrc);

            MatFile mat = new MatFile(imageSrc);
            TobjFile tobj = new TobjFile(mat.texturePath[0]);

            image.texture=tobj.texture;
        }
        if(!string.IsNullOrEmpty(imageColor)) {
            imageColor = imageColor.Substring(6,imageColor.Length-7);
            Debug.LogWarning(imageColor);
            image.color=G.HexToABGR(imageColor);
        } else image.color=Color.white;

    }

    void ApplyCoords(RectTransform tRT,SiiNunit.UI ui) {
        tRT.anchoredPosition=new Vector2(ui.coords_l*factor.x,ui.coords_b*factor.y);
        tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,(ui.coords_r-ui.coords_l)*factor.x);
        tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(ui.coords_t-ui.coords_b)*factor.y);
    }
}
