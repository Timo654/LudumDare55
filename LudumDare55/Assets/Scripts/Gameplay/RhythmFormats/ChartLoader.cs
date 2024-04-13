using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class ChartLoader
{
    public static Chart LoadChart(string filename)
    {
        string json = File.ReadAllText($"{Application.streamingAssetsPath}/{filename}");
        return JsonConvert.DeserializeObject<Chart>(json);
    }

    // for debug
    public static void SaveChart(Chart chartData, string filename)
    {
        string json = JsonConvert.SerializeObject(chartData, Formatting.Indented);
        File.WriteAllText($"{Application.streamingAssetsPath}/{filename}", json);
    }
}
