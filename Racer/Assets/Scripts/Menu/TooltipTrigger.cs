using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string header;
    public string content;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Hide();
    }

    public void Hide()
    {
        TooltipManager.Hide();
    }

}
