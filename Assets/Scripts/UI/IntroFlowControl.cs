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

        RawImage black;
        public RawImage blackBg;
        public GameObject introGroup;

        Stack<GameObject> tempObjs = new Stack<GameObject>();

        public void Load() {
            fc.LoadUIImage(sii,".entity7");  //rating
            fc.LoadUIImage(sii,".entity5");  //brand

            tempObjs.Push(fc.LoadUIImage(sii,".entity3").gameObject);

            black = fc.LoadUIImage(sii,".entity9");  //black
            Hashtable args = new Hashtable {
                { "color",Color.clear },
                { "time",1.6167f },
                { "delay",1f }
            };
            iTween.ColorTo(black.gameObject,args);

            RawImage scs = fc.LoadUIImage(sii,".entity11"); //scs
            tempObjs.Push(scs.gameObject);
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
            tempObjs.Push(line.gameObject);
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
            tempObjs.Push(towers.gameObject);
            tempObjs.Push(title_bus.gameObject);
            tempObjs.Push(bus.gameObject);
            tempObjs.Push(title_driver.gameObject);

            RectTransform rt;
            rt = towers.GetComponent<RectTransform>();
            Hashtable args = new Hashtable {
                {"time",1.9667f },
                {"delay",0.21f },
                {"position",rt.position},
                {"easetype", iTween.EaseType.easeInOutQuad}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x-(rt.anchoredPosition.x+rt.rect.width),rt.anchoredPosition.y);
            iTween.MoveTo(towers.gameObject,args);

            rt = title_bus.GetComponent<RectTransform>();
            args = new Hashtable {
                {"time",1.9667f },
                {"delay",0.21f },
                {"position",rt.position},
                {"easetype", iTween.EaseType.easeInOutQuad}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x-(rt.anchoredPosition.x+rt.rect.width),rt.anchoredPosition.y);
            iTween.MoveTo(title_bus.gameObject,args);
            
            rt = bus.GetComponent<RectTransform>();
            args = new Hashtable {
                {"time",1.7333f },
                {"delay",0.2f },
                {"position",rt.position},
                {"easetype", iTween.EaseType.easeInOutQuad}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x+(1920-rt.anchoredPosition.x),rt.anchoredPosition.y);
            iTween.MoveTo(bus.gameObject,args);

            rt = title_driver.GetComponent<RectTransform>();
            args = new Hashtable {
                {"time",1.7333f },
                {"delay",0.2f },
                {"position",rt.position},
                {"easetype", iTween.EaseType.easeInOutQuad}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x+(1920-rt.anchoredPosition.x),rt.anchoredPosition.y);
            iTween.MoveTo(title_driver.gameObject,args);
            
            RawImage scs_game = fc.LoadUIImage(sii,".entity13"); //scs_game
            tempObjs.Push(scs_game.gameObject);
            rt = scs_game.GetComponent<RectTransform>();
            args = new Hashtable {
                {"time",1.1167f },
                {"delay",1.333f },
                {"position",rt.position},
                {"oncomplete", "DrawEnd" },
                {"onCompleteTarget", gameObject },
                {"easetype", iTween.EaseType.easeInOutQuad}
            };
            rt.anchoredPosition=new Vector2(rt.anchoredPosition.x+(1920-rt.anchoredPosition.x),rt.anchoredPosition.y);
            iTween.MoveTo(scs_game.gameObject,args);
        }

        void DrawEnd() {
            Hashtable args = new Hashtable {
                { "color",Color.black },
                { "time",1.6167f },
                { "delay",2.3f },
                {"oncomplete", "DestroyImages" },
                {"onCompleteTarget", gameObject }
            };
            iTween.ColorTo(black.gameObject,args);
        }

        void DestroyImages() {
            while(tempObjs.Count>0) {
                Destroy(tempObjs.Pop());
            }
            Destroy(blackBg);
            Hashtable args = new Hashtable {
                {"color",Color.clear },
                {"time",1.6167f },
                {"oncomplete", "IntoMainMenu" },
                {"onCompleteTarget", gameObject }
            };
            iTween.ColorTo(black.gameObject,args);
        }

        void IntoMainMenu() {
            introGroup.SetActive(false);
        }
    }
}

