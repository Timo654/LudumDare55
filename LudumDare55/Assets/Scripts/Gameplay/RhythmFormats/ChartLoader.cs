using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class ChartLoader : MonoBehaviour
{
    public static event Action<Chart> OnChartLoaded;
    public void LoadChart(string filename, Difficulty difficulty)
    {
        string json;
        Debug.Log("hello file load");
        filename = $"{filename}_{difficulty}.json".ToLower();
        if (BuildConstants.isWebGL)
        {
            Debug.Log("hello webgl load");
            string fullPath = $"{Application.streamingAssetsPath}/{filename}";
            StartCoroutine(GetRequest(fullPath));
        }
        else
        {
            Debug.Log("hello pc load");
            string fullPath = Path.Combine(Application.streamingAssetsPath, filename);
            json = File.ReadAllText(fullPath);
            OnChartLoaded?.Invoke(JsonConvert.DeserializeObject<Chart>(json));
        }
    }

    IEnumerator GetRequest(string uri)
    {
        Debug.Log("hello debug web request");
        Debug.Log(uri);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived data");
                    OnChartLoaded?.Invoke(JsonConvert.DeserializeObject<Chart>(webRequest.downloadHandler.text));
                    break;
            }
        }

    }
        // for debug
        public static void SaveChart(Chart chartData, string filename)
    {
        string json = JsonConvert.SerializeObject(chartData, Formatting.Indented);
        File.WriteAllText($"{Application.streamingAssetsPath}/{filename}", json);
    }
}
