using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PlayerLoop;

public class Options : MonoBehaviour
{

	public List<ResItem> resolutions = new List<ResItem>();

	public Toggle fullscreenTog;
	
	private int selectedRes;

	public TMP_Text resLabel;
    // Start is called before the first frame update
    void Start()
    {
	    fullscreenTog.isOn = Screen.fullScreen;

	    int i = 0;
	    bool resFound = false;
	    while (i < resolutions.Count && resFound == false)
	    {
		    if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
		    {
			    selectedRes = i;
			    resFound = true;
			    refreshLabel();
		    }
		    i += 1;
	    }

	    if (!resFound)
	    {
		    ResItem newRes = new ResItem();
		    newRes.horizontal = Screen.width;
		    newRes.vertical = Screen.height;
		    
		    resolutions.Add(newRes);
		    selectedRes = resolutions.Count - 1;
		    refreshLabel();
	    }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public void ResRight()
	{
		if (selectedRes > 0)
		{
			selectedRes--;
			refreshLabel();
		}	
	}

	public void ResLeft()
	{
		if (selectedRes < resolutions.Count - 1)
		{
			selectedRes++;
			refreshLabel();
			
		}	
	}

	public void refreshLabel()
	{
		resLabel.text = resolutions[selectedRes].horizontal.ToString() + " X " +
		                resolutions[selectedRes].vertical.ToString();
	}

	public void ApplyGraphics()
	{
		Screen.SetResolution(resolutions[selectedRes].horizontal, resolutions[selectedRes].vertical, fullscreenTog.isOn);
	}
}

[System.Serializable]
public class ResItem
{
	public int horizontal, vertical;
}