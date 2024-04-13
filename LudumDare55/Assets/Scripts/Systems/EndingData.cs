using FMODUnity;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ending", menuName = "Story/Ending")]
[Serializable]
public class EndingData : ScriptableObject
{
    public EndingType endingType;
    public Sprite endingSprite;
    public EventReference endingMusic;
    public string endingText;
    public Sprite endingSprite2;
}

public enum EndingType
{
    Good,
    Bad
}
