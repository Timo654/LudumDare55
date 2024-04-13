using System;

[Serializable]
public class Chart
{
    public Note[] notes;
}

[Serializable]
public class Note
{
    public float startTiming;
    public float endTiming;
    public NoteType noteType;
    public ButtonType buttonType;
}

[Serializable]
public enum ButtonType
{
    None,
    Up,
    Down,
    Left,
    Right
}

public enum NoteType
{
    Regular,
    Hold
}