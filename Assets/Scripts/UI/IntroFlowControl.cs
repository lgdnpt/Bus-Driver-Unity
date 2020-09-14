using fs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ui {
    public class IntroFlowControl:MonoBehaviour {

        SiiNunit sii = new SiiNunit(G.BasePath + "/ui/intro.sii");
        FlowControl fc;

        void Start() {
            fc = G.I.flowControl;
        }

        public void Load() {
            fc.LoadUIImage(sii,".entity7");  //rating
            fc.LoadUIImage(sii,".entity5");  //brand

            RawImage bg = fc.LoadUIImage(sii,".entity3");  //bg

            RawImage black = fc.LoadUIImage(sii,".entity9");  //black
            Hashtable args = new Hashtable {
                { "color",Color.clear },
                { "time",1.6167f },
                { "delay",1f }
            };
            iTween.ColorTo(black.gameObject,args);

            RawImage scs = fc.LoadUIImage(sii,".entity11"); //scs
            Color colorTemp = scs.color;
            colorTemp.a=0;
            args = new Hashtable {
                { "color",colorTemp},
                { "time",1.3f},
                { "delay",2.6167f},
            };
            iTween.ColorFrom(scs.gameObject,args);  //淡入

            args = new Hashtable {
                { "color",colorTemp},
                { "time",1.483f},
                { "delay",6.4167f},
                {"oncomplete", "DrawLine" },
                {"onCompleteTarget", gameObject },
            };
            iTween.ColorTo(scs.gameObject,args);   //淡出
        }

        void DrawLine() {
            RawImage line = fc.LoadUIImage(sii,".entity21");     //line
            RectTransform rt = line.GetComponent<RectTransform>();
            Vector3 vector3 = new Vector3(rt.position.x,rt.position.y,rt.position.z);

            Hashtable args = new Hashtable {
                {"time",2f },
                {"position",vector3},
                {"oncomplete", "DrawBus" },
                {"onCompleteTarget", gameObject },
                {"easetype", iTween.EaseType.linear}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x-(rt.anchoredPosition.x+rt.rect.width),rt.anchoredPosition.y);
            iTween.MoveTo(line.gameObject,args);
        }

        void DrawBus() {
            RawImage towers = fc.LoadUIImage(sii,".entity15");    //towers
            RawImage title_bus = fc.LoadUIImage(sii,".entity16"); //title_bus
            RawImage bus = fc.LoadUIImage(sii,".entity18"); //bus
            RawImage title_driver = fc.LoadUIImage(sii,".entity19"); //title_driver

            RectTransform rt = towers.GetComponent<RectTransform>();
            Vector3 vector = new Vector3(rt.position.x,rt.position.y,rt.position.z);

            Hashtable args = new Hashtable {
                {"time",2f },
                {"position",vector},
                {"oncomplete", "DrawEnd" },
                {"onCompleteTarget", gameObject },
                {"easetype", iTween.EaseType.linear}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x-(rt.anchoredPosition.x+rt.rect.width),rt.anchoredPosition.y);
            iTween.MoveTo(towers.gameObject,args);

            rt = title_bus.GetComponent<RectTransform>();
            vector = new Vector3(rt.position.x,rt.position.y,rt.position.z);
            args = new Hashtable {
                {"time",2f },
                {"position",vector},
                {"oncomplete", "DrawEnd" },
                {"onCompleteTarget", gameObject },
                {"easetype", iTween.EaseType.linear}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x-(rt.anchoredPosition.x+rt.rect.width),rt.anchoredPosition.y);
            iTween.MoveTo(title_bus.gameObject,args);


            RawImage scs_game = fc.LoadUIImage(sii,".entity13"); //scs_game
        }

        void DrawEnd() {

        }
    }
}

