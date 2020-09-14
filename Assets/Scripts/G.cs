using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

class G:Singleton<G>{
    public static string BasePath {
        get {
            if(string.IsNullOrEmpty(basePath)) {
                return basePath = "./base";
            } else {
                return basePath;
            }
        }
        set => basePath = BasePath;
    }
    private static string basePath;

    public ui.FlowControl flowControl;


    public static Color HexToABGR(string strHex) {
        if(strHex.Length != 8) return Color.white;
        byte a = byte.Parse(strHex.Substring(0,2),NumberStyles.HexNumber);
        byte g = byte.Parse(strHex.Substring(2,2),NumberStyles.HexNumber);
        byte b = byte.Parse(strHex.Substring(3,2),NumberStyles.HexNumber);
        byte r = byte.Parse(strHex.Substring(6,2),NumberStyles.HexNumber);
        return new Color32(r,g,b,a);
    }
}
