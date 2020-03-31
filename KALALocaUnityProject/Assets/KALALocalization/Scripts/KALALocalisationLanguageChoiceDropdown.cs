using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KALALocalisationLanguageChoiceDropdown : MonoBehaviour
{
    KALALocalizer localizer { get { return KALALocalizer.instance; } }

    Dropdown dropdown;
    TMP_Dropdown tMP_Dropdown;

    string[] localizedLanguageNames;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<Dropdown>();
        tMP_Dropdown = GetComponent<TMP_Dropdown>();

        // regular dropdown version
        if (dropdown != null)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(CreateLanguageList());

            tMP_Dropdown.onValueChanged.AddListener((int chosenOptionIndex) => localizer.SetLanguage(localizer.availableLanguages[chosenOptionIndex]));
        }
        // tex mesh pro dropdown
        else if(tMP_Dropdown != null)
        {
            tMP_Dropdown.ClearOptions();
            tMP_Dropdown.AddOptions(CreateLanguageList());

            tMP_Dropdown.onValueChanged.AddListener((int chosenOptionIndex) => localizer.SetLanguage(localizer.availableLanguages[chosenOptionIndex]));
        }
    }

    private List<string> CreateLanguageList()
    {
        List<string> localizedNames = new List<string>();

        for (int i = 0; i < localizer.availableLanguages.Count; i++)
        {
            string key = localizer.GetKeyFromTranslation(localizer.availableLanguages[i]);
            if (key != string.Empty) 
            {
                localizedNames.Add (localizer.GetTranslationFromKey(key, localizer.availableLanguages[i]));
            }
            else
            {
                return localizer.availableLanguages;

            }
        }

        return localizedNames;

    }



}
