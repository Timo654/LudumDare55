using DG.Tweening;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class WebGLWarningAnim : MonoBehaviour
{
    public TextMeshProUGUI text;
    private float delayTime;
    private bool animOver = false;
    private float startTime;
    private bool ended = false;
    private LogoState logoState = LogoState.Start;
    private bool mobileTouch = false;
    // Start is called before the first frame update
    void Start()
    {
        UIFader.InitializeFader();
        LevelChanger.Instance.FadeIn();
    }

    // Update is called once per frame
    void Update()
    {
        if (animOver && !ended)
        {
            ended = true;
            LevelChanger.Instance.FadeToLevel("MainMenu");
        }

        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began) mobileTouch = true;
            else mobileTouch = false;
        }

        if (Input.anyKeyDown || mobileTouch)
        {
            animOver = true;
        }

        if (animOver) return;
        // handle state change
        if (Time.time > startTime + delayTime)
        {
            startTime = Time.time;
            switch (logoState)
            {
                case LogoState.Start:
                    logoState = LogoState.FadeIn;
                    delayTime = 0f;
                    break;
                case LogoState.FadeIn:
                    logoState = LogoState.FadeOut;
                    text.DOFade(1f, 2f);
                    delayTime = 5f;
                    break;
                case LogoState.FadeOut:
                    logoState = LogoState.End;
                    text.DOFade(0f, 1f);
                    delayTime = 1f;
                    break;
                case LogoState.End:
                    delayTime = 0f;
                    logoState = LogoState.Start;
                    animOver = true;
                    break;
            }
        }
    }

    enum LogoState
    {
        Start,
        FadeIn,
        FadeOut,
        End
    }
}
