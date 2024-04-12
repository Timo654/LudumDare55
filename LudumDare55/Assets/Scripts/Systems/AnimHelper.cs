using UnityEngine;

public class AnimHelper : MonoBehaviour
{
    // maybe this is hacky but oh well. this exists to let me pause and unpause in animations
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1f;
    }
}
