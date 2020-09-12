#if !UNITY_5_0 && !UNITY_5_1 && !UNITY_5_2
#define UNITY_53_OR_GREATER
#endif

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace EVP {

    public class SceneTools : MonoBehaviour {
	    public bool slowTimeMode = false;
	    public float slowTime = 0.3f;

	    public KeyCode hotkeyReset = KeyCode.R;
	    public KeyCode hotkeyTime = KeyCode.T;


		void Start () {
            hotkeyTime = GlobalClass.Instance.commonKey.slowMode;
        }

	    void Update () {
		    if (Input.GetKeyDown(hotkeyReset)) {
			    SceneManager.LoadScene(0, LoadSceneMode.Single);
		    }

		    if(Input.GetKeyDown(hotkeyTime)) {
				if(slowTimeMode) {
					Time.timeScale=1.0f;
				} else {
					Time.timeScale=slowTime;
				}
				slowTimeMode = !slowTimeMode;
			}

			//Time.timeScale = slowTimeMode? slowTime : 1.0f;
		}
	}
}