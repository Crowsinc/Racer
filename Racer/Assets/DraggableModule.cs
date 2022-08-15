using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableModule : MonoBehaviour
    , IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        transform.position = new Vector3(((int)Mathf.Round(transform.position.x / 26.0f)) * 26 + 13f, ((int)Mathf.Round(transform.position.y / 26.0f)) * 26 + 13f, transform.position.z);
        Debug.Log(transform.position);
    }
}
