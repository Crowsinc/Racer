using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{

    private static TooltipManager current;

    public Tooltip tooltip;
    public void Awake()
    {
        current = this;
    }

    public static void Show(string content, string header="")
    {
        current.tooltip.ShowText(content, header);
        current.tooltip.gameObject.SetActive(true);
    }

    public static void Hide()
    {
        current.tooltip.gameObject.SetActive(false);
    }
}
