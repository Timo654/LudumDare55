using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ChartLoader
{
    // will chatgpt work
    public static Chart LoadChart(string filename)
    {
        string json;

        // Check if we're running in WebGL
        if (BuildConstants.isWebGL)
        {
            // Use UnityWebRequest to load the file from StreamingAssets
            string filePath = Path.Combine(Application.streamingAssetsPath, filename);
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(filePath);
            www.SendWebRequest();

            while (!www.isDone) { } // Wait for the request to complete

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                json = www.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Error loading file: " + www.error);
                return null; // Handle the error appropriately
            }
        }
        else
        {
            // For standalone builds (Windows), read directly from the file system
            string fullPath = Path.Combine(Application.streamingAssetsPath, filename);
            json = File.ReadAllText(fullPath);
        }

        return JsonConvert.DeserializeObject<Chart>(json);
    }

    // for debug
    public static void SaveChart(Chart chartData, string filename)
    {
        string json = JsonConvert.SerializeObject(chartData, Formatting.Indented);
        File.WriteAllText($"{Application.streamingAssetsPath}/{filename}", json);
    }
}
