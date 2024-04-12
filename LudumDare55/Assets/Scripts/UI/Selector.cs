using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Selector : MonoBehaviour, IPointerEnterHandler
{
    Selectable currentItem;

    void Awake()
    {
        currentItem = GetComponent<Selectable>();

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem.interactable && currentItem.navigation.mode != Navigation.Mode.None)
        {
            currentItem.Select();
        }
    }
}