using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuModule : MonoBehaviour
    , IPointerDownHandler
    , IPointerUpHandler
{
    public GameObject draggblePrefab;

    private bool _clicked = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_clicked)
        {
            GameObject draggable = Instantiate(draggblePrefab, transform.position, Quaternion.identity, transform.parent.parent);
            Image draggableImage = draggable.GetComponent<Image>();
            draggableImage.sprite = this.GetComponent<Image>().sprite;
            draggableImage.preserveAspect = true;
            draggable.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, draggableImage.sprite.rect.height / 2f);
            draggable.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, draggableImage.sprite.rect.width / 2f);
            _clicked = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _clicked = false;   
    }
}
