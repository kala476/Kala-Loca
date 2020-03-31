using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KALALocalizableText : MonoBehaviour
{
    protected KALALocalizer localizer;

    public string key;

    protected TextMeshProUGUI textMeshPro;
    protected Text regularText;

    // Start is called before the first frame update
    void Awake()
    {
        localizer = FindObjectOfType<KALALocalizer>();
        textMeshPro = GetComponent<TextMeshProUGUI>();
        regularText = GetComponent<Text>();
    }

    private void OnEnable()
    {
        // subscribe to event in case language changes when you are enabled
        localizer.onLanguageChanged += UpdateText;
        localizer.onLanguageChanged += UpdateFont;
    }

    private void OnDisable()
    {
        // unsbscribe when you are disabled
        localizer.onLanguageChanged -= UpdateText;
        localizer.onLanguageChanged -= UpdateFont;
    }


    private void UpdateText()
    {
        if (textMeshPro != null)
        {  if (localizer.TryGetCurrentranslationFromKey(key))
            {
                textMeshPro.text = localizer.GetCurrentTranslationFromKey(key);
            }
        }
        if(regularText != null )
        {
            if (localizer.TryGetCurrentranslationFromKey(key))
            {
                regularText.text = localizer.GetCurrentTranslationFromKey(key);
            }
        }
    }

    private void UpdateFont()
    {
        if (textMeshPro != null )
        {
            TMP_FontAsset newFontAsset = localizer.GetCurrentTMP_FontAssset();

            if(newFontAsset!= null)
            {
                textMeshPro.font = newFontAsset;
            }
        }

        if (regularText != null)
        {
            Font newFont = localizer.GetCurrentFont();

            if (newFont != null)
            {
                regularText.font = newFont;
            }

        }

    }
}
