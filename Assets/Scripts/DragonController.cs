using System.Collections;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Animator animator;
    public Camera bossRoomCamera;
    private bool isFlying = false;
    private AudioSource audioSource;
    public AudioClip screamClip;
    public GameObject player;
    private bool attackPermission = true;
    enum EnemyState 
    {
        idle,
        drakaris,
        fly,
        dead
    }
    private EnemyState currentState;
    /*四种状体，近距离啃咬，远距离喷火，飞行，死亡*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(currentState);
        EnemyStateSwitch();
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
    public IEnumerator EnemyStateRountine() 
    {
        while (true) 
        {
            float distance = Vector3.Distance(player.transform.position, transform.position);
            Debug.Log(distance);
            if (distance > 4f&& distance < 9f)
            {
                currentState = EnemyState.drakaris;
            }
            else if (distance > 10f) 
            {
                currentState = EnemyState.idle;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
    public void EnemyStateSwitch() 
    {
        if (attackPermission) 
        {
            switch (currentState)
            {
                case EnemyState.drakaris:
                    animator.SetTrigger("drakaris_trigger");
                    attackPermission = false;
                    StartCoroutine(AttackCoolDown(4));
                    break;
            }
        }
    }
    IEnumerator AttackCoolDown(float n) 
    {
        yield return new WaitForSeconds(n);
        attackPermission = true;
    }
}
