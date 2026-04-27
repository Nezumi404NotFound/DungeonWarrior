using System.Collections;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Animator animator;
    public Camera bossRoomCamera;
    private bool isFlying = false;
    private AudioSource audioSource;
    public AudioClip screamClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void PlayClip() 
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Scream")) 
        {
            audioSource.PlayOneShot(screamClip);
        }
    }
    private void OnAnimatorMove()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Fly") && isFlying == false) 
        {
            StartCoroutine(Fly());
        }
    }
    IEnumerator Fly() 
    {
        isFlying = true;
        float startTime = Time.time;
        while (startTime + 2f > Time.time) 
        {
            transform.Translate(Vector3.up * 2 * Time.deltaTime);
            yield return null;
        }
    }
}
