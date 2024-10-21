using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{

    [SerializeField]
    private AudioMixer musicMixer;
    [SerializeField]
    private AudioMixer sfxMixer;
    [SerializeField]
    private AudioMixer mainMixer;
    [SerializeField]
    private Slider musicSlider;
    [SerializeField]
    private Slider sfxSlider;

    public void UpdateMusicVolume(){
        //Debug.Log("music vol");
        mainMixer.SetFloat("MusicVol", musicSlider.value);
    }

    public void UpdateSFXVolume(){
        mainMixer.SetFloat("SfxVol", sfxSlider.value);
    }
   
}