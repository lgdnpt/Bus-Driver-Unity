using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AVC {
    public class Retarder:MonoBehaviour {
        public AdvancedVehicleController avc;
        [Header("Retarder")]
        public AnimationCurve curve;
        /// <summary>
        /// 驱动轴转缓速器的速比
        /// </summary>
        public float rpmRatio;
        public float rpm;
        /// <summary>
        /// 用于计算曲线X的固定值
        /// </summary>
        public float rpmMax;
        /// <summary>
        /// 用于计算曲线Y的固定值
        /// </summary>
        public float brakeRatio = 0.5f;
        /// <summary>
        /// 自动缓速的比例(固定值)
        /// </summary>
        public float autoBrake = 0.1f;
        /// <summary>
        /// 刹车输出值
        /// </summary>
        public float brakeOutput = 0f;

        [Header("Audio")]
        public AudioSource audioSource;
        [Range(0, 3)]
        public float minPitch = 0.2f;
        [Range(0, 3)]
        public float maxPitch = 1.1f;

        public float maxVolume = 0.2f;

        private void Update() {
            rpm = avc.rpmAxle * rpmRatio;

            brakeOutput = curve.Evaluate(rpm / rpmMax) * Mathf.Max(avc.brakeInput, autoBrake) * brakeRatio;

            //if (avc.target.speed < 3) audioSource.mute = true;
            //else audioSource.mute = false;
            ProcessContinuousAudio(audioSource, 0.2f, minPitch, maxPitch, (avc.throttleInput<0.05f)?(brakeOutput / brakeRatio) :0, maxVolume);
        }

        public void ProcessContinuousAudio(AudioSource audio, float ratio, float minPitch, float maxPitch, float ratioVolume, float maxVolume) {
            audio.pitch = Mathf.Lerp(minPitch, maxPitch, ratio);
            audio.volume = Mathf.Lerp(0, maxVolume, ratioVolume);
            if (!audio.isPlaying) audio.Play();
        }
    }
}