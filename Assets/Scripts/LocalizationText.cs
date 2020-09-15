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
            text.text = LocalizationManager.GetLocalization(key);
        }
        Destroy(this);
    }
}
