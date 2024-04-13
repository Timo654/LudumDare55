using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

// from https://gamedevbeginner.com/how-to-keep-score-in-unity-with-loading-and-saving/#save_with_xml
public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance { get; set; }
    public SystemData systemData;
    public GameData gameData;
    public RuntimeData runtimeData; // for values that won't get saved to disk, but we use in-game
    private static string mPersistentDataPath;
    public static SaveManager Instance
    {
        get
        {
            // if exists directly return
            if (_instance) return _instance;

            // otherwise search it in the scene
            _instance = FindObjectOfType<SaveManager>();

            // found it?
            if (_instance) return _instance;

            // otherwise create and initialize it
            CreateInstance();

            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void CreateInstance()
    {
        // skip if already exists
        if (_instance) return;
        InitializeInstance(new GameObject(nameof(SaveManager)).AddComponent<SaveManager>());
    }

    private static void InitializeInstance(SaveManager _instance)
    {
        SaveManager._instance = _instance;
        DontDestroyOnLoad(SaveManager._instance.gameObject);
#if UNITY_WEBGL && !UNITY_EDITOR
        mPersistentDataPath = $"idbfs/{BuildConstants.gameName}"; 
#else
        mPersistentDataPath = Application.persistentDataPath;
#endif
        if (!Directory.Exists(mPersistentDataPath)) // webgl does not automatically make the folder
        {
            Directory.CreateDirectory(mPersistentDataPath);
        }
        SaveManager._instance.systemData = LoadData<SystemData>("options.json");
        SaveManager._instance.gameData = LoadData<GameData>("data.json");
        SaveManager._instance.runtimeData = new();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SaveAll();
    }

    // Autosave on scene load.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveAll();
    }

    private static void SaveAll()
    {
        if (_instance.systemData != null) { SaveData(_instance.systemData, "options.json"); }
        if (_instance.gameData != null) { SaveData(_instance.gameData, "data.json"); }
    }

    private static void SaveData<T>(T saveData, string fileName)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new InvalidOperationException("A serializable type is required");
        }
        string json = JsonConvert.SerializeObject(saveData, Formatting.Indented);
        File.WriteAllText(mPersistentDataPath + "/" + fileName, json);
    }

    private static T LoadData<T>(string fileName) where T : new()
    {
        if (!typeof(T).IsSerializable)
        {
            throw new InvalidOperationException("A serializable type is required");
        }
        T data = new();
        if (File.Exists(mPersistentDataPath + "/" + fileName))
        {
            string json = File.ReadAllText(mPersistentDataPath + "/" + fileName);
            data = JsonConvert.DeserializeObject<T>(json);
        }
        return data;
    }


}

[Serializable]
public class SystemData
{
    public float SFXVolume = 50f;
    public float UIVolume = 40f;
    public float MusicVolume = 20f;
}

[Serializable]
public class GameData
{
    public int previousScore = 0;
}

[Serializable]
public class RuntimeData
{
    public string previousSceneName;
}


