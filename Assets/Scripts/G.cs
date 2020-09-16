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

    public static string DataPath {
        get {
            if(string.IsNullOrEmpty(dataPath)) {
                return dataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\Bus Driver";
            } else {
                return dataPath;
            }
        }
        set => dataPath = DataPath;
    }
    private static string dataPath;

    public ui.FlowControl flowControl;


    public static Color HexToABGR(string strHex) {
        if(strHex.Length != 8) return Color.white;
        byte a = byte.Parse(strHex.Substring(0,2),NumberStyles.HexNumber);
        byte g = byte.Parse(strHex.Substring(2,2),NumberStyles.HexNumber);
        byte b = byte.Parse(strHex.Substring(3,2),NumberStyles.HexNumber);
        byte r = byte.Parse(strHex.Substring(6,2),NumberStyles.HexNumber);
        return new Color32(r,g,b,a);
    }

    public static int HexToFloat(string temp) {
        if(temp.IndexOf('&')<0) return int.Parse(temp);
        else {
            temp = temp.Replace("&","");
            byte[] bytes = new byte[4];
            bytes[0]=Convert.ToByte(temp.Substring(0,2),16);
            bytes[1]=Convert.ToByte(temp.Substring(2,2),16);
            bytes[2]=Convert.ToByte(temp.Substring(4,2),16);
            bytes[3]=Convert.ToByte(temp.Substring(6,2),16);

            float f = BitConverter.ToSingle(bytes,0);
            return (int)f;
        }
    }
}
