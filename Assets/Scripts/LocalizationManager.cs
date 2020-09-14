using fs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalizationManager {
    private static LocalizationManager _instance;
    public static LocalizationManager I {
        get {
            if(_instance == null) _instance = new LocalizationManager();
            return _instance;
        }
    }

/*    private const string chinese = "zh_cn";
    private const string english = "en_us";*/

    //选择自已需要的本地语言  
    public string language = "zh_cn";

    private Dictionary<string,string> dic = new Dictionary<string,string>();
    /// <summary>  
    /// 读取配置文件，将文件信息保存到字典里  
    /// </summary>  
    public LocalizationManager() {
        DefReader local = new DefReader(G.BasePath + "/script/"+language+"/local.def");
        dic = local.keys;
        //Debug.LogWarning(local.extdefs);
        if(!string.IsNullOrEmpty(local.extdefs)) {
            DefReader localStop = new DefReader(G.BasePath + local.extdefs);
            dic = dic.Concat(localStop.keys).ToDictionary(k => k.Key,v => v.Value);
            if(!string.IsNullOrEmpty(localStop.extdefs)) {
                DefReader localCommon = new DefReader(G.BasePath + localStop.extdefs);
                dic = dic.Concat(localCommon.keys).ToDictionary(k => k.Key,v => v.Value);
            }
        }


    }

    /// <summary>  
    /// 获取value  
    /// </summary>  
    /// <param name="key"></param>  
    /// <returns></returns>  
    public string GetValue(string key) {
        if(!dic.ContainsKey(key)) return null;
        dic.TryGetValue(key,out string value);
        return value;
    }
}
