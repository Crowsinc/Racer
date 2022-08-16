using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class ButtonSounds : MonoBehaviour
{
    // Start is called before the first frame update
    public AudioMixer Mixer;
    
    public AudioClip[] sounds;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void PlayButtonSound()
    {
        var random = Random.Range(0, sounds.Length);
        Debug.Log(random);
        GetComponent<AudioSource>().PlayOneShot(sounds[random]);
    }
}
