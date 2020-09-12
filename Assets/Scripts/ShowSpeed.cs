using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EVP {
    public class ShowSpeed : MonoBehaviour {
		public Text speedText;


		private void Start() {
			if(speedText == null)
				speedText = GetComponent<Text>();
		}

		void Update () {
			if(GlobalClass.Instance.InputManager.target!=null)
				speedText.text = string.Format("{0,5:0}",GlobalClass.Instance.InputManager.target.speed * 3.6f).Trim();
		}

		public void clear() {
			speedText.text="0.0";
			enabled=false;
		}
	}
}
