using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndingHandler : MonoBehaviour
{
    [SerializeField] private EndingData debugEndingData;
    private Image endingImage;
    private TextMeshProUGUI endingText;
    private bool running = true;
    private Sprite endSprite1;
    private Sprite endSprite2;
    // Start is called before the first frame update
    private void Awake()
    {
        endingImage = transform.GetChild(0).GetComponent<Image>();
        endingText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }
    void Start()
    {
        EndingData endingData = SaveManager.Instance.runtimeData.currentEnding;
        if (endingData == null)
        {
            Debug.LogWarning("no ending found, loading debug ending...");
            endingData = debugEndingData;
        }
        endingImage.sprite = endingData.endingSprite;
        endingText.text = endingData.endingText;

        if (endingData.endingSprite2 != null)
        {
            endSprite1 = endingData.endingSprite;
            endSprite2 = endingData.endingSprite2;
            StartCoroutine(AnimateEnding());
        }
        AudioManager.Instance.InitializeMusic(endingData.endingMusic);
        AudioManager.Instance.StartMusic();
        LevelChanger.Instance.FadeIn();
        StartCoroutine(Ending());
    }

    IEnumerator AnimateEnding()
    {
        while (running)
        {
            yield return new WaitForSeconds(0.5f);
            endingImage.sprite = endSprite2;
            yield return new WaitForSeconds(0.5f);
            endingImage.sprite = endSprite1;
        }

    }
    IEnumerator Ending()
    {
        while (AudioManager.IsPlaying() == true)
        {
            yield return null;
        }
        running = false;
        LevelChanger.Instance.FadeToLevel("Credits");
    }
}
