using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuModule : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Transform _moduleHolder; // Parent to store all placed modules
    private bool _clicked = false;
    public GameObject originalPrefab;

    // Set module holder
    private void Awake()
    {
        _moduleHolder = GameObject.FindGameObjectWithTag("GameController").GetComponent<SimulationController>().buildModeModuleHolder.transform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_clicked)
        {
            // Instantiate draggable module
            GameObject draggable = Instantiate(gameObject, transform.position, Quaternion.identity, _moduleHolder);
            Destroy(draggable.GetComponent<MenuModule>());
            draggable.AddComponent<DraggableModule>();
            draggable.GetComponent<DraggableModule>().originalPrefab = originalPrefab;
            draggable.GetComponent<VehicleModule>().Freeze();
            _clicked = true;
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _clicked = false;
    }
}
