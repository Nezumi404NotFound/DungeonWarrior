using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public Slider volumeSlider;
    public AudioMixer mainMixer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        volumeSlider.onValueChanged.AddListener(delegate { VolumeChange(); });
        volumeSlider.maxValue = 1;
        volumeSlider.value = 0.5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void VolumeChange()
    {
        mainMixer.SetFloat("MusicVolume", Mathf.Log10(volumeSlider.value) * 20);
    }    
}
