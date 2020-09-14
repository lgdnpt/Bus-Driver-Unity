using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizationText))]
public class LocalizationTextEditor:Editor {
/*	public override void OnInspectorGUI() {
		serializedObject.Update();

		EditorGUILayout.Space();
		EditorGUILayout.PropertyField(serializedObject.FindProperty("key"),true);
	}*/
}
