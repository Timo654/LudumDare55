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
    public GameObject endNote;
    public GameObject longLine;

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

    public void DestroyNote()
    {
        if (endNote != null)
        {
            endNote.GetComponent<ButtonScript>().DestroyNote();
            Destroy(longLine);
        }
        buttonSprite.enabled = false;
        StartCoroutine(DestroyCoroutine());

    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);

    }
}
