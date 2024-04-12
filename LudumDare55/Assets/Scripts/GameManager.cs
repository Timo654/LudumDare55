using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        LevelChanger.Instance.FadeIn();
    }

    private void OnEnable()
    {
        LevelChanger.OnFadeInFinished += HandleStart;
    }

    private void HandleStart()
    {
        Time.timeScale = 1.0f;
        Debug.Log("start!");
    }
}
