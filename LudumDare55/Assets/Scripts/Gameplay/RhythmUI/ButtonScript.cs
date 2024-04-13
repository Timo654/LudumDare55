using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour
{
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
    public Note note;
    public double noteLength = 0f;

    public void InitializeNote(float xPosition, float yPosition, Note Note)
    {
        note = Note;
        buttonType = Note.buttonType;
        noteType = Note.noteType;
        transform.localPosition = new Vector2(xPosition, yPosition);
        switch (buttonType)
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
                Debug.Log("Invalid button type " + buttonType);
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
        StartCoroutine(DestroyCoroutine());

    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);

    }
}
