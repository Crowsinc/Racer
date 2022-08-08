using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Options : MonoBehaviour
{

	public List<ResItem> resolutions = new List<ResItem>();
	
	private int selectedRes;

	public TMP_Text resLabel;
    // Start is called before the first frame update
    void Start()
    {
        
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
		
	}
}

[System.Serializable]
public class ResItem
{
	public int horizontal, vertical;
}