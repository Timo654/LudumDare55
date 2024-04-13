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

    private GameObject endNote;
    private GameObject longLine;

    private double noteLength = 0f;

    // Start is called before the first frame update
    public void InitializeNote(ButtonType button, NoteType note, float position)
    {
        buttonType = button;
        noteType = note;
        transform.localPosition = new Vector2(position, transform.localPosition.y);
        m_image = GetComponent<Image>();
        switch (button)
        {
            case ButtonType.Up:
                m_image.sprite = upSprite;
                break;
            case ButtonType.Right:
                m_image.sprite = rightSprite;
                break;
            case ButtonType.Left:
                m_image.sprite = leftSprite;
                break;
            case ButtonType.Down:
                m_image.sprite = downSprite;
                break;
            default:
                Debug.Log("Invalid button type " + button);
                break;
        }

    }

    public void DestroyNote()
    {
        m_image.enabled = false;
        StartCoroutine(DestroyCoroutine());

    }

    IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);

    }
}
