using SharpConfig;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class UIGlobal : MonoBehaviour {
    public VolumeProfile profile;
    public GameObject dialogPrefeb;
    //public UISettingGraphics uiSettingGraphics;

    private void OnEnable() {
/*        if(uiSettingGraphics!=null)
            uiSettingGraphics.loadFromFileToRealtime();*/
    }

/*    public void showDialog(string text) {
        UIDialogFrame n = Instantiate(dialogPrefeb,transform).GetComponent<UIDialogFrame>();
        n.contentText.text=text;
    }*/
/*    public void showDialog(string title,string text,int size) {
        UIDialogFrame n = Instantiate(dialogPrefeb,transform).GetComponent<UIDialogFrame>();
        n.contentText.text=text;

        if(!string.IsNullOrEmpty(title)) {
            n.titleText.text=title;
        }
        Vector3 v3 = new Vector3(3,3,3);
        print(v3);
        if(size!=0) {
            n.contentText.fontSize=size;
        }
    }*/

    public void startGame() {
        if(SceneManager.GetSceneByName("Nanjing").isLoaded) {
            SceneManager.LoadScene("Nanjing");
        } else {
            Loom.RunAsync(() => {
                System.Threading.Thread.Sleep(20);
                Loom.QueueOnMainThread(() => {
                    //保存需要加载的目标场景
                    Globe.nextSceneName = "Nanjing";
                    SceneManager.LoadScene("Loading");
                });
            });
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            
        }
    }

    public void exitGame() {
        Application.Quit();
    }

    public void pauseGame() {
        Time.timeScale=0;
    }
    public void resumeGame() {
        Time.timeScale=1;
    }
    public void backToMenu() {
/*        Loom.RunAsync(() => {
            System.Threading.Thread.Sleep(20);
            Loom.QueueOnMainThread(() => {
                //保存需要加载的目标场景
                Globe.nextSceneName = "MainMenu";
                SceneManager.LoadScene("Loading");
                SceneManager.UnloadScene("Nanjing");
            });
        });*/
        SceneManager.LoadScene("MainMenu",LoadSceneMode.Single);
        //SceneManager.UnloadScene("Nanjing");
        //UnityEngine.SceneManagement.SceneManager.LoadScene(1,UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
