using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ChartLoader
{
    public static Chart LoadChart(string filename, Difficulty difficulty)
    {
        string json;

        filename = $"{filename}_{difficulty}.json";
        if (BuildConstants.isWebGL)
        {
            // TODO - rework this to work in coroutine
            json = "";
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
