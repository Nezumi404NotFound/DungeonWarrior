using UnityEngine;
using UnityEngine.SceneManagement;

public class TitlePlayerController : MonoBehaviour
{
    public AudioClip playerStartClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void PlayClip() 
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.PlayOneShot(playerStartClip);
        }
    }
    public void ChangeSceneAfterPlayAnimation() 
    {
        SceneManager.LoadScene("MainScene");
    }
}
