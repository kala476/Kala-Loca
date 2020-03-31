using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(GoogleDocDownloader))]
public class KALALocalizer : MonoBehaviour
{
    [Header("Common Settings")]
    [Tooltip("If set to false you will have to call Initialize() manually from other script")]
    public bool initializeOnAwake = true;

    [Header("Downloading And Saving Data")]
    public string GoogleSheetURL;
    [Tooltip("Path where downloaded data is daved. Relative to presistentDataPath")]
    public string relativeSavePath;
    public bool hideLocalisationFile = false;
    [Tooltip("falbackTextAsset that will be used for localization in case the player never openes the game while connected to the internet. Updates automatically, whenever you start the game in editor.")]
    public TextAsset fallbackTextAsset;

    [Header("Languages And Fonts")]

    public List<LanguageFontPair> languagesAndFonts = new List<LanguageFontPair>();
    [Tooltip("the language that will be loaded at startup")]
    public string defaultLanguage = "English";
    private int currentLanguegeIndex;
    public string currentLanguage;
    [Tooltip("This font asset will be used for all localized TMPro components, if no language-specific font is set ")]
    public TMP_FontAsset defaultFontAsset;
    [Tooltip("This font will be used for localized Text components, if no language-specific font is set ")]
    public Font defaultFont;
    public List<string> availableLanguages;

    [Header("Data Management")]
    public List<StringList> contentPreviw = new List<StringList>();
    public static KALALocalizer instance;
    [HideInInspector] public string rawData;
    private List<List<string>> parsedTable;
    private Dictionary<string, string[]> locaTable;
    private Dictionary<string, int> languageIDsFromNames;
    private Dictionary<int, string> languageNamesFromIDs;

    public System.Action onLanguageChanged;
    public System.Action onInitialized;



    private void Awake()
    {
        instance = this;
        if (initializeOnAwake)
        {
            Initialize();
        }

    }


    private void Initialize()
    {  

        GoogleDocDownloader downloader = GetComponent<GoogleDocDownloader>();

        //try loading the file previously downloaded loca file - this file will be updated each time the game is opened with the connection to the internet
        rawData = downloader.LoadCSV(GetActualSavePath(relativeSavePath));
        if (rawData != string.Empty)
        {
            ExtractData();
            SetLanguage(defaultLanguage);
        }

#if UNITY_EDITOR
        ////if it th file was not found, use a txt file from Assets (will only happen if the game was downloaded, and was never opened with acces to the internet.
        if (rawData == string.Empty && fallbackTextAsset != null && fallbackTextAsset.text != string.Empty)
        {
            rawData = fallbackTextAsset.text;
            ExtractData();
            SetLanguage(defaultLanguage);
        }

        // if default text asset does not exist, create it.
        if(rawData != string.Empty && fallbackTextAsset == null)
        {
            TextAsset newTextAsset = new TextAsset();
            AssetDatabase.CreateAsset(newTextAsset, "Assets/LocalizationTable.txt");
            AssetDatabase.SaveAssets();
            fallbackTextAsset = newTextAsset;
            EditorUtility.SetDirty(this.fallbackTextAsset);
            File.WriteAllText(AssetDatabase.GetAssetPath(fallbackTextAsset), rawData);
            EditorUtility.SetDirty(newTextAsset);
        }
#endif

        // finally try connecting 
        downloader.SaveFromGoogleSheet(GoogleSheetURL, GetActualSavePath(relativeSavePath),hideLocalisationFile,
                                       () => 
                                        {
                                            rawData = downloader.LoadCSV(GetActualSavePath(relativeSavePath));
                                            ExtractData();
                                            SetLanguage(defaultLanguage);
#if UNITY_EDITOR
											if (fallbackTextAsset != null)
											{
												File.WriteAllText(AssetDatabase.GetAssetPath(fallbackTextAsset), rawData);
												EditorUtility.SetDirty(fallbackTextAsset);
											}

#endif
                                        });

        onInitialized?.Invoke();
    }

    public void ExtractData(bool generatePreview = true)
    {
        if (rawData != string.Empty)
        {
            parsedTable = ParseCSVText(rawData);
            locaTable = ParseContentIntoDictionary(parsedTable);
            languageIDsFromNames = ParseLanguagesIntoIDDictionary(parsedTable);
            languageNamesFromIDs = ParseLanguagesIntoNameDictionary(parsedTable);
            ExtractLanguages();

			if (generatePreview)
			{
				ParseIntoPreview(parsedTable);
			}


        }
    }

    
   
    #region accesing data

    //auto generate keys

    public TMP_FontAsset GetTMP_FontAssetFromLanguage(string languageName)
    {
        TMP_FontAsset languageSpecificFont = languagesAndFonts.Find(item => item.languageName == languageName).TMP_fontAsset;
       
        if (languageSpecificFont != null)
        {
            return languageSpecificFont;
        }

        return defaultFontAsset;

    }

    public Font GetFontFromLanguage(string languageName)
    {
        Font languageSpecificFont = languagesAndFonts.Find(item => item.languageName == languageName).font;

        if (languageSpecificFont != null)
        {
            return languageSpecificFont;
        }

        return defaultFont;
    }


    public TMP_FontAsset GetCurrentTMP_FontAssset()
    {
        return GetTMP_FontAssetFromLanguage(currentLanguage);
    }

    public Font GetCurrentFont()
    {
        return GetFontFromLanguage(currentLanguage);
    }

    public bool TryGetTranslationFromKey(string key, int languageIndex )
    {
        try
        {
            if (locaTable.ContainsKey(key) && locaTable[key][languageIndex] != string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        } 
        catch
        {
            return false;
        }
        
    }

    public bool TryGetCurrentranslationFromKey(string key)
    {
        return TryGetTranslationFromKey(key, currentLanguegeIndex);

    }

    public string GetTranslationFromKey(string key, int languageIndex)
    { 
        return locaTable[key][languageIndex];
    }

    public string GetTranslationFromKey(string key, string languageName)
    {
        return locaTable[key][languageIDsFromNames[languageName]];
    }


    public string GetCurrentTranslationFromKey(string key)
    {
        return GetTranslationFromKey(key, currentLanguegeIndex);
    }


    public void SetLanguage(int index)
    {
        currentLanguegeIndex = index;
        currentLanguage = languageNamesFromIDs[index];
        onLanguageChanged?.Invoke();
    }

    public void SetLanguage(string languageName)
    {
        Debug.Log("setting language to:" +languageName);
        currentLanguage = languageName;
        currentLanguegeIndex = languageIDsFromNames[languageName];
        onLanguageChanged?.Invoke();
    }

    public bool isLanguageAvailable(string languageName)
    {
        return availableLanguages.Contains(languageName);
    }

    public string GetKeyFromTranslation(string text)
    {
			KeyValuePair<string, string[]> myEntry = locaTable.FirstOrDefault(x => x.Value.Contains(text));

			// if an entry was found return its key
			if (!myEntry.Equals(default(KeyValuePair<string, string[]>))) // this is a fanc way of checking if a key value pair is "null"
			{
				return myEntry.Key;
			}

			else
			{
				return string.Empty;

			}

    }

    #endregion

    #region  parsing data

    private string GetActualSavePath(string relativePath)
    {
        string actualSavePath = Application.persistentDataPath + "/" + relativePath;

        if (!actualSavePath.EndsWith(".txt"))
        {
            actualSavePath += ".txt";

        }

        return actualSavePath;


    }

    private List<List<string>> ParseCSVText(string textToParse)
    {
        char lineSeparator = '\n';
        char fieldSeparator = ',';
        string currentEntry = "";
        List<string> currentRow = new List<string>();
        List<List<string>> parsedTable = new List<List<string>>();
        int currentIndex = 0;
        bool isInQuote = false;
        bool justChangedQuote = false;

        Debug.Log("Parsing CSV");

        while (currentIndex < textToParse.Length)
        {
            //check how many quotemarks are there in a row
            int quotesInARow = QuotemarksInARow(textToParse, currentIndex);

            // are we inside a quotation? (== actualcquotation is closed and opened with just a single " )
            if (quotesInARow % 2 == 1)
            {
                isInQuote = !isInQuote;
                justChangedQuote = true;


            }
            // if there is a quotation within an entry, the quotation marks will be doubled,
            // hence if number of quotation marks is divisible by 2, we are dealing with quotation withi an entry
            if (quotesInARow > 1)
            {
                // add them to the final string
                for (int i = 1; i < quotesInARow; i++)
                {
                    currentEntry += "\"";
                }
                currentIndex += quotesInARow;
                continue;
            }

            //add other stuff o the entry
            if (isInQuote)
            {
                if (justChangedQuote)
                {
                    justChangedQuote = false;
                }
                else
                {
                    if ((byte)textToParse[currentIndex] > 31)
                    {
                        currentEntry += textToParse[currentIndex];
                    }
                }
            }
            else
            {
                if (textToParse[currentIndex] == fieldSeparator)
                {
                    currentRow.Add(currentEntry);
                    currentEntry = "";
                }
                else if (textToParse[currentIndex] == lineSeparator)
                {

                    currentRow.Add(currentEntry);
                    currentEntry = "";
                    parsedTable.Add(new List<string>(currentRow));
                    currentRow = new List<string>();

                }
                else
                {
                    if (justChangedQuote)
                    {
                        justChangedQuote = false;
                    }
                    else 
                    {
                        if ((byte)textToParse[currentIndex] > 31)
                        {
                            currentEntry += textToParse[currentIndex];
                        }

                    }

                }
            }

            currentIndex++;

        }

        currentRow.Add(currentEntry);
        currentEntry = "";
        parsedTable.Add(new List<string>(currentRow));
        currentRow = new List<string>();

        return parsedTable;


    }


    private int QuotemarksInARow(string stringToCheck, int indexToStart)
    {
        int checkedIndex = indexToStart;

        while (checkedIndex < stringToCheck.Length && stringToCheck[checkedIndex] == '\"')
        {
            checkedIndex++;
        }

        return checkedIndex - indexToStart;

    }

     
    public bool TryGetIDFromLanguageName( string languageName)
    {
        if (languageIDsFromNames.ContainsKey(languageName))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public int GetIDFromLanguageName(string languageName)
    {
        return languageIDsFromNames[languageName];
    }


    private void ParseIntoPreview(List<List<string>> listOflists)
    {
        Debug.Log("Generating content preview....");

        contentPreviw = new List<StringList>();

        foreach (List<string> list in listOflists)
        {
            StringList newItem = new StringList();
            newItem.strings = list;
            contentPreviw.Add(newItem);

        }
    }

    private Dictionary<string, string[]> ParseContentIntoDictionary(List<List<string>> table)
    {
        Dictionary<string, string[]> parsedContent = new Dictionary<string, string[]>();

        for (int i = 1; i < table.Count; i++)
        {
            parsedContent.Add(table[i][0], table[i].ToArray());
        }

        return parsedContent;
    }

    private Dictionary<string, int> ParseLanguagesIntoIDDictionary(List<List<string>> table)
    {
        Dictionary<string, int> languageIDs = new Dictionary<string, int>();

        for (int i = 0; i < table[0].Count; i++)
        {
            if (table[0][i] != string.Empty)
            {
                languageIDs.Add(table[0][i], i);
            }

        }

        return languageIDs;

    }

    private Dictionary<int, string> ParseLanguagesIntoNameDictionary(List<List<string>> table)
    {
        Dictionary<int, string> languageNames = new Dictionary<int, string>();

        for (int i = 0; i < table[0].Count; i++)
        {
            if (table[0][i] != string.Empty)
            {
                languageNames.Add(i, table[0][i]);
            }

        }

        return languageNames;

    }

    private void ExtractLanguages()
    {
        availableLanguages.Clear();
        for (int i = 0; i < languagesAndFonts.Count; i++)
        {
            if (languageIDsFromNames.ContainsKey(languagesAndFonts[i].languageName))
            {
                availableLanguages.Add(languagesAndFonts[i].languageName);
            }
        }
    }
    #endregion

    // support classes
    [System.Serializable]
    public class LanguageFontPair
    {
        public string languageName;
        public TMP_FontAsset TMP_fontAsset;
        public Font font;
    }

    [System.Serializable]
    public class StringList
    {
        public List<string> strings;

    }
}


