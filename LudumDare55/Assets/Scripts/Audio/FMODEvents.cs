using FMODUnity;
using UnityEngine;

public class FMODEvents : MonoBehaviour
{
    [field: Header("SFX")]
    [field: SerializeField] public EventReference TestSound { get; private set; }
    [field: SerializeField] public EventReference WrongSound { get; private set; }

    [field: Header("UI")]
    [field: SerializeField] public EventReference ButtonClick { get; private set; }
    [field: SerializeField] public EventReference HoverButton { get; private set; }
    [field: SerializeField] public EventReference BackSound { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference MenuTheme { get; private set; }
    [field: SerializeField] public EventReference CreditsTheme { get; private set; }
    public static FMODEvents Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }
        Instance = this;
    }
}
