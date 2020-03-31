using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

[CustomEditor(typeof(KALALocalizer))]
public class KALALocalizerCustomInspector : Editor
{

    public override void OnInspectorGUI()
    {
        KALALocalizer localizer = (KALALocalizer)target;
        base.OnInspectorGUI();

        if (GUILayout.Button("Add\"KALALocalizableText\" to all TextComponents")) 
        {
            // get all textMeshProAssetsInTheScene Inthe Scene in teh scene
            List<TextMeshProUGUI> textMeshPros = new List<TextMeshProUGUI>();

            foreach (TextMeshProUGUI textMeshPro in Resources.FindObjectsOfTypeAll(typeof(TextMeshProUGUI)) as TextMeshProUGUI[])
            {
                if (!EditorUtility.IsPersistent(textMeshPro.transform.root.gameObject) && !(textMeshPro.hideFlags == HideFlags.NotEditable || textMeshPro.hideFlags == HideFlags.HideAndDontSave))
                    textMeshPros.Add(textMeshPro);
            }

            foreach(TextMeshProUGUI textMeshPro in textMeshPros)
            {
                if (textMeshPro.gameObject.GetComponent<KALALocalizableText>() == null)
                {
                    textMeshPro.gameObject.AddComponent<KALALocalizableText>();
                }
            }

            //same for regular text components

            // get all textMeshProAssetsInTheScene Inthe Scene in teh scene
            List<Text> texts = new List<Text>();

            foreach (Text text in Resources.FindObjectsOfTypeAll(typeof(Text)) as Text[])
            {
                if (!EditorUtility.IsPersistent(text.transform.root.gameObject) && !(text.hideFlags == HideFlags.NotEditable || text.hideFlags == HideFlags.HideAndDontSave))
                    texts.Add(text);
            }

            foreach (Text text in texts)
            { 
                if (text.gameObject.GetComponent<KALALocalizableText>() == null)
                {
                    text.gameObject.AddComponent<KALALocalizableText>();
                }
            }

            Debug.Log("KALALocalizableText added to text components");

        }

        if (GUILayout.Button("AutoGenerate Keys"))
        {
            Debug.Log("generating keys...");

            // load the textAsset
            localizer.rawData = localizer.fallbackTextAsset.text;
            localizer.ExtractData(true);

            // get all textAssets Inthe Scene in teh scene
            List<KALALocalizableText> texts = new List<KALALocalizableText>();

            foreach (KALALocalizableText locaText in Resources.FindObjectsOfTypeAll(typeof(KALALocalizableText)) as KALALocalizableText[])
            {
                if (!EditorUtility.IsPersistent(locaText.transform.root.gameObject) && !(locaText.hideFlags == HideFlags.NotEditable || locaText.hideFlags == HideFlags.HideAndDontSave))
                    texts.Add(locaText);
            }


            foreach(KALALocalizableText locaText in texts)
            {

                TextMeshProUGUI textMeshPro = locaText.GetComponent<TextMeshProUGUI>();
                Text regularText = locaText.GetComponent<Text>();

                if (textMeshPro != null)
                {
                    string newKey = localizer.GetKeyFromTranslation(textMeshPro.text);
                    if (newKey != string.Empty)
                    {
                        locaText.key = newKey;
                    }
                }

                if (regularText != null)
                {
                    string newKey = localizer.GetKeyFromTranslation(regularText.text);
                    if (newKey != string.Empty)
                    {
                        locaText.key = newKey;
                    }
                }
            }

            Debug.Log("Keys generated");

        }

        if (localizer.fallbackTextAsset == null)
        {
            TextAsset text = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/LocalizationTable.txt", typeof(TextAsset));
            if (text != null)
            {
                localizer.fallbackTextAsset = text;
                EditorUtility.SetDirty(target);
            }
        }
    }
}
