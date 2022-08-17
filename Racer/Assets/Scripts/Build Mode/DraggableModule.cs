using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableModule : MonoBehaviour
    , IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private VehicleConstructor _vehicleConstructor;
    public GameObject originalPrefab;
    private void Awake()
    {
        _vehicleConstructor = GameObject.FindGameObjectWithTag("GameController").GetComponent<VehicleConstructor>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y), 0); //((int)Mathf.Round(transform.position.y / 26.0f)) * 26 + 13f
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        bool successful = _vehicleConstructor.TryAddModule(gameObject, originalPrefab);
        if (!successful)
        {
            Destroy(gameObject);
        }
    }

    public void OnPointerDown(PointerEventData eventData) { }
}
