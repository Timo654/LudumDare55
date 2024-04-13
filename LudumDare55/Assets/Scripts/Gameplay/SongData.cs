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
}
