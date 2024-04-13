using FMODUnity;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Song Data", menuName = "Song Data")]
[Serializable]
public class SongData : ScriptableObject
{
    public string Name;
    public Chart chart; // this is overwritten by file
    public string chartFile; // json file read from streamingassets.
    public EventReference music;
    public int musicLengthMs; // in milliseconds
    public float displayDuration = 3;
    public int greatRange = 100; // milliseconds
    public int goodRange = 200; // milliseconds
    public int badRange = 500; // milliseconds
    public SongData nextSong;
    [Range(0, 100)]
    public int holdPercentGreat = 80;
    [Range(0, 100)]
    public float holdPercentGood = 60;
}
