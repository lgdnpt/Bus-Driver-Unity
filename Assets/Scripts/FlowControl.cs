﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using fs;
using System.Text.RegularExpressions;
using System.IO;

namespace ui {
    public class FlowControl:MonoBehaviour {
        public Canvas canvas;
        public RawImage rawImage;
        public InputField input;

        public RectTransform[] layer;

        Vector2 factor;

        public void Start() {
            //factor=new Vector2(Screen.width/1024f,Screen.height/768f);
            factor=new Vector2(1.875f,1.40625f);
        }

        public void test() {
            Match matchImg = Regex.Match("<align hstyle=center vstyle=center><img src=/material/ui/white.mat color=FF000000 xscale=stretch yscale=stretch></align>",@"<img[^>]*?>");
            Match matchSrc = Regex.Match(matchImg.ToString(),input.text);//"src=[^>]*\\s"
                                                                         //Match matchColor = Regex.Match(matchImg.ToString(),"color=[^>]*\\s");
            print(matchSrc.ToString());
        }



        public RawImage LoadUIImage(SiiNunit sii,string unitName) {
            sii.unit.TryGetValue(unitName,out SiiNunit.Unit unit);
            if(unit!=null) {
                if(!(unit is SiiNunit.UI_Text)) {
                    Debug.LogError("unit不是ui::text");
                    return null;
                }
                unit.item.TryGetValue("name",out string itemName);
                unit.item.TryGetValue("text",out string itemText);
                unit.item.TryGetValue("layer",out string layerStr);
                if(!string.IsNullOrEmpty(itemName))
                    print("Render " + itemName);
                //print("ImagePath " + itemText);

                SiiNunit.UI_Text ui = (SiiNunit.UI_Text)unit;
                RawImage image;
                if(string.IsNullOrEmpty(layerStr)) {
                    image = Instantiate(rawImage,layer[0]);
                } else {
                    image = Instantiate(rawImage,layer[int.Parse(layerStr)]);
                }

                try {
                    ApplyImageWithImgTag(image,itemText);
                } catch(FileNotFoundException e) {
                    //找不到mat
                    Debug.LogWarning(e);
                    Destroy(image);
                    return null;
                }

                Match matchAlign = Regex.Match(itemText,@"<align[^>]*?>");
                if(!string.IsNullOrEmpty(matchAlign.Value)) {
                    //居中
                    //Debug.LogWarning("居中"+itemText);
                    ApplyCoordsCenter(image.rectTransform,ui,image.texture.width,image.texture.height);
                } else {
                    ApplyCoords(image.rectTransform,ui);
                }
                return image;
            } else {
                print("null");
                return null;
            }
        }

        public void ApplyImageWithImgTag(RawImage image,string tag) {

            Match matchImg = Regex.Match(tag,@"<img[^>]*?>");
            string imageSrc = Regex.Match(matchImg.ToString(),"src=[^>]*?(\\s|>)").Value;
            string imageColor = Regex.Match(matchImg.ToString(),"color=[^>]*?(\\s|>)").Value;

            if(!string.IsNullOrEmpty(imageSrc)) {
                imageSrc = imageSrc.Substring(4,imageSrc.Length-5);
                //Debug.LogWarning(imageSrc);

                if(!File.Exists(G.BasePath + imageSrc)) {
                    throw new FileNotFoundException(imageSrc);
                }
                MatFile mat = new MatFile(imageSrc);
                TobjFile tobj = new TobjFile(mat.texturePath[0]);

                image.texture=tobj.texture;
            }
            if(!string.IsNullOrEmpty(imageColor)) {
                imageColor = imageColor.Substring(6,imageColor.Length-7);
                //Debug.LogWarning(imageColor);
                image.color=G.HexToABGR(imageColor);
            } else image.color=Color.white;

        }

        public void ApplyCoords(RectTransform tRT,SiiNunit.UI ui) {
            //上下镜像
            tRT.rotation = Quaternion.AngleAxis(180,Vector3.right);
            tRT.anchoredPosition=new Vector2(ui.coords_l*factor.x,/*Screen.height - */(ui.coords_b*factor.y)+(ui.coords_t-ui.coords_b)*factor.y);

            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,(ui.coords_r-ui.coords_l)*factor.x);
            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(ui.coords_t-ui.coords_b)*factor.y);
        }

        public void ApplyCoordsCenter(RectTransform tRT,SiiNunit.UI ui,int width,int height) {
            //上下镜像
            tRT.rotation = Quaternion.AngleAxis(180,Vector3.right);
            tRT.anchoredPosition=new Vector2((ui.coords_r+ui.coords_l - width)*factor.x/2f,(ui.coords_t+ui.coords_b-height)*factor.y/2 + height*factor.y);

            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width*factor.x);
            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height*factor.y);
        }
    }

}