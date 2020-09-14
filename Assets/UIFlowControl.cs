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
        LoadUIImage(sii,".entity13");
        LoadUIImage(sii,".entity15");
        LoadUIImage(sii,".entity16");
        LoadUIImage(sii,".entity18");
        LoadUIImage(sii,".entity19");
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

            ApplyImageWithImgTag(imgBg,itemText);

            Match matchAlign = Regex.Match(itemText,@"<align[^>]*?>");
            if(!string.IsNullOrEmpty(matchAlign.Value)) {
                //居中
                Debug.LogWarning("居中"+itemText);
                print(imgBg.texture.width);
                print(imgBg.texture.height);
                ApplyCoordsCenter(imgBg.rectTransform,ui,imgBg.texture.width,imgBg.texture.height);
            } else {
                ApplyCoords(imgBg.rectTransform,ui);
            }

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
        string imageSrc = Regex.Match(matchImg.ToString(),"src=[^>]*?(\\s|>)").Value;
        string imageColor = Regex.Match(matchImg.ToString(),"color=[^>]*?(\\s|>)").Value;

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
        //上下镜像
        tRT.rotation = Quaternion.AngleAxis(180,Vector3.right);
        tRT.anchoredPosition=new Vector2(ui.coords_l*factor.x,/*Screen.height - */(ui.coords_b*factor.y)+(ui.coords_t-ui.coords_b)*factor.y);

        tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,(ui.coords_r-ui.coords_l)*factor.x);
        tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(ui.coords_t-ui.coords_b)*factor.y);
    }

    void ApplyCoordsCenter(RectTransform tRT,SiiNunit.UI ui,int width,int height) {
        //上下镜像
        tRT.rotation = Quaternion.AngleAxis(180,Vector3.right);
        tRT.anchoredPosition=new Vector2( (ui.coords_r+ui.coords_l - width)*factor.x/2f, (ui.coords_t+ui.coords_b-height)*factor.y/2 + height*factor.y);

        tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width*factor.x);
        tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height*factor.y);
    }
}
