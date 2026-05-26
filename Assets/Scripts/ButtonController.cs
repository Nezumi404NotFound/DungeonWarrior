using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ButtonController : MonoBehaviour
{
    public GameObject startButton;
    public GameObject exitButton;
    public GameObject player;
    public AudioClip exitClip;
    public GameObject defaultGUI;
    public GameObject settingGUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SetTo1920x1080()
    {
        Screen.SetResolution(1920, 1080, true);
    }
   public void SetTo2560x1440() 
    {
        Screen.SetResolution(2560, 1440, true);
    }
   public void SetTo3840x2160() 
    {
        Screen.SetResolution(3840, 2160, true);
    }
    public void GetStartButtonClicked()
    {
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("start_trigger");
        }
    }
    public void GetExitButtoClicked()
    {
        AudioSource playerAudioSouce = player.GetComponent<AudioSource>();
        playerAudioSouce.PlayOneShot(exitClip);
        StartCoroutine(ExitAfterPlayCLip());
    }
    public void GetSettingButtonClicked()
    {
        defaultGUI.SetActive(false);
        settingGUI.SetActive(true);
    }
    public void GetWindowExitClicked()
    {
        settingGUI.SetActive(false);
        defaultGUI.SetActive(true);
    }
    public void GetBackToTitleButtonClicked()
    {
        SceneManager.LoadScene("Title");
    }
IEnumerator ExitAfterPlayCLip() 
    {
        yield return new WaitForSeconds(exitClip.length);
        Application.Quit();
        //UnityEditor.EditorApplication.isPlaying = false;(退出编辑器)
    }
}
