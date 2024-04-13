using FMODUnity;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Song Data", menuName = "Song Data")]
[Serializable]
public class SongData : ScriptableObject
{
    public string Name;
    public Chart chart;
    public EventReference music;
    public int musicLengthMs; // in milliseconds
    public float displayDuration = 3;
    public float greatRange = 0.1f;
    public float goodRange = 0.2f;
    [Range(0, 100)]
    public int holdPercentGreat = 80;
    [Range(0, 100)]
    public float holdPercentGood = 60;
}
