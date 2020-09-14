using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizationText : MonoBehaviour {
    public string key;
    void Start() {
        Text text = GetComponent<Text>();
        if(text!=null) {
            text.text=LocalizationManager.I.GetValue(key);
        }
        Destroy(this);
    }
}
