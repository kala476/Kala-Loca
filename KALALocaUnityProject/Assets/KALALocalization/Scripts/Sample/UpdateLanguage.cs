using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLanguage : MonoBehaviour
{
    public string languageToSet;
    private KALALocalizer localizer;


    public void SetLanguageFromName()
    {
        localizer = FindObjectOfType<KALALocalizer>();
        if (localizer.isLanguageAvailable(languageToSet))
        {
            localizer.SetLanguage(languageToSet);
        } else
        {
            Debug.LogError($"chosen language \" {languageToSet} \"is not available");
        }

    }
}
