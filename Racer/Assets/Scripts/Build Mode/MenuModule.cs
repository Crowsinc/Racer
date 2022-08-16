using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuModule : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Transform _moduleHolder; // Parent to store all placed modules
    private bool _clicked = false;

    // Set module holder
    public void SetModuleHolder(Transform moduleHolder)
    {
        _moduleHolder = moduleHolder;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_clicked)
        {
            // Instantiate draggable module
            GameObject draggable = Instantiate(gameObject, transform.position, Quaternion.identity, _moduleHolder);
            Destroy(draggable.GetComponent<MenuModule>());
            draggable.AddComponent<DraggableModule>();
            _clicked = true;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _clicked = false;
    }
}
