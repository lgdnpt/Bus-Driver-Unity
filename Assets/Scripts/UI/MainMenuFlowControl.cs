using fs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ui {
    public class MainMenuFlowControl : MonoBehaviour {
        // Start is called before the first frame update
        SiiNunit sii = new SiiNunit(G.BasePath + "/ui/main_menu.sii");
        FlowControl fc;

        void Start() {
            fc = G.I.flowControl;
        }



    }
}