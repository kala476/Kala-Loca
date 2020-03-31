using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleDocDownloader : MonoBehaviour
{
    private string downloadedData;

    /// <summary>
    /// downloads data from a google sheet with given url and saves it as a csv document at savePath
    /// </summary>
    public void SaveFromGoogleSheet(string googleSheetUrl, string savePath, bool hideFile, System.Action OnSaved = null)
    {
        Debug.Log("parsing localization url....");
        // parse the URL
        string[] separators = new string[1] { "/edit" };
        string[] splitUrl = googleSheetUrl.Split(separators, System.StringSplitOptions.None);

        string downloadLink = splitUrl[0] + "/export?format=csv";


        // download CSV
        StartCoroutine(DownloadCSV(downloadLink, () =>
        {
            SaveCSV(downloadedData, savePath, hideFile);
            OnSaved?.Invoke();
        }
                                  ));

    }

    /// <summary>
    /// Downloads data from the given downloadfile and saves it in "downloadedData"
    /// </summary>
    public IEnumerator DownloadCSV(string downloadUrl, System.Action onSuccesfullDownload)
    {
        yield return new WaitForEndOfFrame();

        Debug.Log(" Downloading file from url:  " + downloadUrl);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(downloadUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.LogError("Newtwork Error while downloading google sheet!");
            }
            else
            {
                Debug.Log("No network error");

                downloadedData = null;
                downloadedData = webRequest.downloadHandler.text;

                Debug.Log("WebRequest sucessfull");

                onSuccesfullDownload.Invoke();
            }
        }
    }

    #region TEXT FILES
    /// <summary>
    /// saves given string as a csv file at the given path
    /// </summary>
    public void SaveCSV(string dataToSave, string path, bool hideFile = false)
    {
        Debug.Log("Saving CSV");
        // parse path
        string[] splitPath = path.Split('/');
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
        {
            Debug.Log($"Creating Directory {directory}");
            Directory.CreateDirectory(directory);

        }

        using (StreamWriter sw = File.CreateText(path))
        {
            sw.Write(dataToSave);
            Debug.Log("File saved");
        }

        if(hideFile)
        {
            Debug.Log("Hiding file...");
            HideFile(path);
        }
    }

    public string LoadCSV(string filePath)
    {
        if (File.Exists(filePath))
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                return sr.ReadToEnd();

            }

        }
        else
        {
            return string.Empty;
        }
    }

    public void HideFile(string filePath)
    {
        File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.Hidden);
    }
}
#endregion