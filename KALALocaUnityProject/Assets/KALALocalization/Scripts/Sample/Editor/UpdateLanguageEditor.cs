using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdateLanguage))]
public class UpdateLanguageEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdateLanguage updateLanguage = (UpdateLanguage)target;
        if(GUILayout.Button("Update Language"))
        {
            updateLanguage.SetLanguageFromName();
        }
    }
}
