using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Globe {
    public static string nextSceneName;
    public static float[] entryPosition;
}

public class AsyncLoadScene:MonoBehaviour {
    public Slider loadingSlider;
    public Text loadingText;
    private float loadingSpeed = 1;
    private float targetValue;

    public VideoPlayer vpLoading;
    public VideoPlayer vp;

    private AsyncOperation operation;

    public AnimationCurve sizeCurve;
    public Image bgBlur;

    void Start() {
        loadingSlider.value = 0.0f;

        

        if (SceneManager.GetActiveScene().name == "Loading") {
            //启动协程
            StartCoroutine(AsyncLoading());
        }
    }

    IEnumerator AsyncLoading() {
        operation = SceneManager.LoadSceneAsync(Globe.nextSceneName);
        //阻止当加载完成自动切换
        operation.allowSceneActivation = false;

        yield return operation;
    }

    // Update is called once per frame
    void Update() {
        targetValue = operation.progress;

        if (operation.progress >= 0.9f) {
            targetValue = 1.0f;
        }

        if (targetValue != loadingSlider.value) {
            loadingSlider.value = Mathf.Lerp(loadingSlider.value, targetValue, Time.deltaTime * loadingSpeed);
            if (Mathf.Abs(loadingSlider.value - targetValue) < 0.01f) {
                loadingSlider.value = targetValue;
            }
        }

        bgBlur.material.SetFloat("_Size",sizeCurve.Evaluate(loadingSlider.value));

        loadingText.text = ((int)(loadingSlider.value * 100)).ToString() + "%";

        if ((int)(loadingSlider.value * 100) == 100) {
/*            if (!vp.isPlaying && !loaded) {
                vpLoading.Stop();
                vp.Play();
                loaded = true;
            }
            if(!vp.isPlaying && loaded) {
                operation.allowSceneActivation = true;
            }*/
            operation.allowSceneActivation = true;
            //StopAllCoroutines();
        }
    }
}