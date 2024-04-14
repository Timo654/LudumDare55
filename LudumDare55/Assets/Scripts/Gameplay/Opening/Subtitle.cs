using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Subtitle Data", menuName = "Subtitle Data")]
[Serializable]
public class Subtitle : ScriptableObject
{
    public SubtitleEntry[] subtitles;
}
