using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;


public class Options : MonoBehaviour
{

	public List<ResItem> resolutions = new List<ResItem>();

	public Toggle fullscreenTog;
	
	private int selectedRes;

	public TMP_Text resLabel;

	public AudioMixer Mixer;

	public TMP_Text masterVolLabel, musicVolLabel, SFXVolLabel;
	public Slider masterVolSlider, musicVolSlider, SFXVolSlider;
	
	
    // Start is called before the first frame update
    void Start()
    {
	    fullscreenTog.isOn = Screen.fullScreen;

	    // int i = 0;
	    // bool resFound = false;
	    // while (i < resolutions.Count && resFound == false)
	    // {
		   //  if (Screen.width == resolutions[i].horizontal && Screen.height == resolutions[i].vertical)
		   //  {
			  //   selectedRes = i;
			  //   resFound = true;
			  //   refreshLabel();
		   //  }
		   //  i += 1;
	    // }
	    //
	    // if (!resFound)
	    // {
		   //  ResItem newRes = new ResItem();
		   //  newRes.horizontal = Screen.width;
		   //  newRes.vertical = Screen.height;
		   //  
		   //  resolutions.Add(newRes);
		   //  selectedRes = resolutions.Count - 1;
		   //  refreshLabel();
	    // }

	    float vol = 0f;
	    Mixer.GetFloat("MasterVol", out vol);
	    masterVolSlider.value = UnRubberBandVolume(vol);
	    
	    Mixer.GetFloat("MusicVol", out vol);
	    musicVolSlider.value = UnRubberBandVolume(vol);
	    
	    Mixer.GetFloat("SFXVol", out vol);
	    SFXVolSlider.value = UnRubberBandVolume(vol);
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

	public void SetMasterVol()
	{
		masterVolLabel.text = Mathf.RoundToInt(masterVolSlider.value + 80).ToString();

		var application = RubberBandVolume(masterVolSlider.value);
		
		Mixer.SetFloat("MasterVol", application);
		
		PlayerPrefs.SetFloat("MasterVol", application);
	}
	
	public void SetMusicVol()
	{
		musicVolLabel.text = Mathf.RoundToInt(musicVolSlider.value + 80).ToString();

		var application = RubberBandVolume(musicVolSlider.value);
		
		Mixer.SetFloat("MusicVol", application);
		
		PlayerPrefs.SetFloat("MusicVol", application);
	}
	
	public void SetSFXVol()
	{
		SFXVolLabel.text = Mathf.RoundToInt(SFXVolSlider.value + 80).ToString();

		var application = RubberBandVolume(SFXVolSlider.value);
		
		Mixer.SetFloat("SFXVol", application);
		
		PlayerPrefs.SetFloat("SFXVol", application);
	}

	public float RubberBandVolume(float volumeSlider)
	{
		var value = -20 + 40 * (volumeSlider + 80) / 100;
		
		if (value <= -20)
		{
			value = -80;
		}

		return value;
	}

	public float UnRubberBandVolume(float volumeValue)
	{
		var value = -80 + 100 * (volumeValue + 20) / 40;
		
		return value;
	}
}

[System.Serializable]
public class ResItem
{
	public int horizontal, vertical;
}