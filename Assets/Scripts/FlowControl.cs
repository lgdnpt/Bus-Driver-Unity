using System.Collections;
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

        public Text infoTitle;
        public Text infoText;
        public Text scoreText;
        public RawImage overlayImage;
        public Text[][] block;

        public void Start() {
            //factor=new Vector2(Screen.width/1024f,Screen.height/768f);
            factor=new Vector2(1.875f,1.40625f);
        }

        public void test() {
            //Match matchImg = Regex.Match("<align hstyle=center vstyle=center><img src=/material/ui/white.mat color=FF000000 xscale=stretch yscale=stretch></align>",@"<img[^>]*?>");
            //Match matchSrc = Regex.Match(matchImg.ToString(),input.text);//"src=[^>]*\\s"
            //Match matchColor = Regex.Match(matchImg.ToString(),"color=[^>]*\\s");
            //print(matchSrc.ToString());

            DirectoryInfo dir = new DirectoryInfo(G.BasePath+"/def/route");
            FileInfo[] files = dir.GetFiles();
            //int i = 0;
            foreach(FileInfo file in files) {
                if(!file.Extension.Equals(".sii",System.StringComparison.OrdinalIgnoreCase)) {
                    Debug.LogWarning("wrong file:"+file.FullName);
                    continue;
                }
                //if(i>10) break;
                //i++;
                string routePath = "/def/route/" + file.Name;
                PreloadRoute(routePath);
            }
        }

        public GameObject root;
        public void test2() {
            //Match matchImg = Regex.Match("<align hstyle=center vstyle=center><img src=/material/ui/white.mat color=FF000000 xscale=stretch yscale=stretch></align>",@"<img[^>]*?>");
            //Match matchSrc = Regex.Match(matchImg.ToString(),input.text);//"src=[^>]*\\s"
            //Match matchColor = Regex.Match(matchImg.ToString(),"color=[^>]*\\s");
            //print(matchSrc.ToString());

            PMGLoader.LoadPMG("/model/misc/board/billboard01.pmd",root);
        }

        void LoadMissionSelection() {

        }

        public MissionTier[] missionTiers;

        void PreloadRoute(string routePath) {
            SiiNunit routeSii = new SiiNunit(G.BasePath + routePath);

            foreach(SiiNunit.Unit value in routeSii.unit.Values) {
                if(value is SiiNunit.Mission route) {
                    if(route.tier >=6 || route.rank >=6 || route.tier<0 || route.rank<0) {
                        break;
                    }

                    infoTitle.text=LocalizationManager.GetLocalization(route.short_desc);
                    infoText.text=LocalizationManager.GetLocalization(route.long_desc);

                    string busname= route.vehicle_data.Substring(route.vehicle_data.LastIndexOf('.')+1);
                    try {
                        missionTiers[route.tier].busicos[route.rank].texture = MatFile.GetTexture("/material/ui/bus/busico_" + busname + ".mat");
                    } catch(FileNotFoundException) {
                        Debug.LogWarning("缺失车辆图:"+busname);
                        missionTiers[route.tier].busicos[route.rank].color = Color.red;
                    }
                    
                    
                    string missionName = route.unitName.Substring(route.unitName.LastIndexOf('.')+1);
                    try {
                        overlayImage.texture = MatFile.GetTexture("/material/ui/mission/" + missionName + ".mat");
                    }catch(FileNotFoundException) {
                        Debug.LogWarning("缺失线路图:"+missionName);
                        overlayImage.color = Color.clear;
                    }

                    overlayImage.SetNativeSize();
                    overlayImage.rectTransform.anchoredPosition=new Vector2Int(route.overlay_offset_x, -route.overlay_offset_y);

                    //读取存档分数
                    string savePath = G.DataPath + "\\save\\" + routePath.Substring(routePath.LastIndexOf("/")+1);
                    scoreText.text=GetScore3(savePath);

                    break;
                }
            }

        }

        string GetScore3(string savePath) {
            if(!File.Exists(savePath)) {
                return "<i>1.\n2.\n3.</i>";
            }

            SiiNunit saveSii;
            try {
                saveSii = new SiiNunit(savePath);
            }catch(System.Exception) {
                return "<i>1.\n2.\n3.</i>";
            }

            SortedList sortedList = new SortedList();
            foreach(KeyValuePair<string,SiiNunit.Unit> kvp in saveSii.unit) {
                if(kvp.Value.className.Equals("high_score_data")) {
                    kvp.Value.item.TryGetValue("score",out string temp);
                    if(!string.IsNullOrEmpty(temp) && !temp.Equals("0")) {
                        sortedList.Add(temp,temp);
                    }
                }
            }

            string scorestr = "<i>";
            string[] score = new string[3];
            for(int i = 0;i<3;i++) {
                if(i<sortedList.Count) score[i] = sortedList.GetByIndex(i).ToString();
                else score[i] = "";
                scorestr+=(i+1)+"."+score[i]+"\n";
            }
            scorestr += "</i>";
            return scorestr;
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
                //MatFile mat = new MatFile(imageSrc);
                //TobjFile tobj = new TobjFile(mat.texturePath[0]);

                image.texture=MatFile.GetTexture(imageSrc,false);
            }
            if(!string.IsNullOrEmpty(imageColor)) {
                imageColor = imageColor.Substring(6,imageColor.Length-7);
                //Debug.LogWarning(imageColor);
                image.color=G.HexToABGR(imageColor);
            } else image.color=Color.white;

        }

        public void ApplyCoords(RectTransform tRT,SiiNunit.UI ui) {
            //上下镜像
            //tRT.rotation = Quaternion.AngleAxis(180,Vector3.right);
            tRT.anchoredPosition=new Vector2(ui.coords_l*factor.x,/*Screen.height - */(ui.coords_b*factor.y)/*+(ui.coords_t-ui.coords_b)*factor.y*/);

            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,(ui.coords_r-ui.coords_l)*factor.x);
            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,(ui.coords_t-ui.coords_b)*factor.y);
        }

        public void ApplyCoordsCenter(RectTransform tRT,SiiNunit.UI ui,int width,int height) {
            //上下镜像
            //tRT.rotation = Quaternion.AngleAxis(180,Vector3.right);
            tRT.anchoredPosition=new Vector2((ui.coords_r+ui.coords_l - width)*factor.x/2f,(ui.coords_t+ui.coords_b-height)*factor.y/2);

            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width*factor.x);
            tRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,height*factor.y);
        }
    }

}