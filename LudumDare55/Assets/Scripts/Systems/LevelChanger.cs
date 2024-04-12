using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    public static event Action OnFadeInFinished;
    public Animator animator;
    public float transitionTime = 1f; // couldnt get animation events to work right now
    private bool loadInProgess = false;
    public static LevelChanger Instance { get; private set; }

    private Coroutine co_HideCursor;

    void Update()
    {
#if !UNITY_EDITOR
        if (Input.GetAxis("Mouse X") == 0 && (Input.GetAxis("Mouse Y") == 0))
        {
            if (co_HideCursor == null)
            {
                co_HideCursor = StartCoroutine(HideCursor());
            }
        }
        else
        {
            if (co_HideCursor != null)
            {
                StopCoroutine(co_HideCursor);
                co_HideCursor = null;
                Cursor.visible = true;
            }
        }
#endif
    }

    private IEnumerator HideCursor()
    {
        yield return new WaitForSeconds(5);
        Cursor.visible = false;
    }

    public void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
    }
    public void FadeIn()
    {
        StartCoroutine(FadeInCoroutine());
    }
    public void FadeToLevel(string levelName)
    {
        if (!loadInProgess) StartCoroutine(LoadLevel(levelName));
        else Debug.LogWarning($"Already loading a level, cannot load {levelName}!");
    }
    public void FadeToDesktop()
    {
        if (!loadInProgess) StartCoroutine(QuitToDesktop());
        else Debug.LogWarning($"Already loading a level!");
    }
    IEnumerator LoadLevel(string levelToLoad)
    {
        if (SaveManager.Instance != null) SaveManager.Instance.runtimeData.previousSceneName = SceneManager.GetActiveScene().name;
        loadInProgess = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic();
            AudioManager.Instance.StopSFX();
        }
        animator.SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(transitionTime);
        SceneManager.LoadSceneAsync(levelToLoad);
        loadInProgess = false;
    }
    IEnumerator QuitToDesktop()
    {
        Debug.Log("Quitting.");
        loadInProgess = true;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic();
            AudioManager.Instance.StopSFX();
        }
        animator.SetTrigger("FadeOut");
        yield return new WaitForSecondsRealtime(transitionTime);
        loadInProgess = false;
        Application.Quit();
    }
    IEnumerator FadeInCoroutine()
    {
        animator.SetTrigger("FadeIn");
        yield return new WaitForSecondsRealtime(transitionTime);
        OnFadeInFinished?.Invoke();
    }
}