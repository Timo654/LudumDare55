using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
    // The object to reach in 6 seconds
    Image m_image;
    public NoteType noteType;
    public ButtonType buttonType;
    [SerializeField] private Sprite upSprite;
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite leftSprite;
    [SerializeField] private Sprite downSprite;
    [SerializeField] private Image buttonSprite;
    [SerializeField] private ParticleSystem hitParticleSystem;
    public ParticleSystem fireParticleSystem;
    public GameObject endNote;
    public GameObject longLine;
    public AnimationCurve bounceCurve;
    public double noteLength = 0f;

    // Start is called before the first frame update
    public void InitializeNote(ButtonType button, NoteType note, float xPosition, float yPosition)
    {
        buttonType = button;
        noteType = note;
        transform.localPosition = new Vector2(xPosition, yPosition);
        switch (button)
        {
            case ButtonType.Up:
                buttonSprite.sprite = upSprite;
                break;
            case ButtonType.Right:
                buttonSprite.sprite = rightSprite;
                break;
            case ButtonType.Left:
                buttonSprite.sprite = leftSprite;
                break;
            case ButtonType.Down:
                buttonSprite.sprite = downSprite;
                break;
            default:
                Debug.Log("Invalid button type " + button);
                break;
        }

    }

    public void DestroyNote(HitGrade grade)
    {
        if (endNote != null)
        {
            endNote.GetComponent<ButtonScript>().DestroyNote(grade);
            Destroy(longLine);
            buttonSprite.DOFade(0f, 0.25f);
        }
        else
        {
            buttonSprite.DOKill();
            buttonSprite.transform.DOScale(Vector3.one * 3, 2f).SetEase(bounceCurve);
            buttonSprite.DOFade(0f, 0.5f);
            hitParticleSystem.Play();
            if (grade == HitGrade.Great)
            {
                fireParticleSystem.Play();
            }
        }
        //buttonSprite.enabled = false;

        StartCoroutine(DestroyCoroutine());

    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);

    }
}
