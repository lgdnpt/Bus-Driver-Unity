using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class G:Singleton<G>{
    public string BasePath {
        get {
            if(string.IsNullOrEmpty(basePath)) {
                return basePath = "./base";
            } else {
                return basePath;
            }
        }
        set => basePath = BasePath;
    }
    private string basePath;
}
