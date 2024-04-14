using System;
using UnityEngine;

[Serializable]
public class SubtitleEntry
{
    public float startPosition; // seconds
    public float endPosition;
    [TextAreaAttribute]
    public string text;
}

