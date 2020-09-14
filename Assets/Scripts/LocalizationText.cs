using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationText : MonoBehaviour {
    public string key;
    void Start() {
        Text text = GetComponent<Text>();
        if(text!=null) {

            text.text = GetLocalization(key);
        }
        Destroy(this);
    }

    string GetLocalization(string key) {
        string str = LocalizationManager.I.GetValue(key);
        if(str==null) {
            Debug.LogError("Can find key"+key);
            return key;
        }
        if(str.Contains("align")) {
            Match matchAlign = Regex.Match(str,@"<align[^>]*?>");
            str = matchAlign.Value;
            str = str.Substring(str.IndexOf(' ')+1,str.IndexOf('>')-str.IndexOf(' ')-1);
        }
        if(str.Contains("@@")) {
            str=str.Substring(str.IndexOf("@@")+2);
            str=str.Substring(0,str.IndexOf("@@"));
            str = GetLocalization(str);
        }
        if(str.Substring(0,1).Equals("\"") && str.Substring(str.Length-1,1).Equals("\"")) {
            str=str.Substring(1,str.Length-2);
        }
        return str;
    }
}
