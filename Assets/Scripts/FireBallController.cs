using System.Collections;
using System.IO.Pipes;
using UnityEngine;

public class FireBallController : MonoBehaviour
{
    public GameObject player;
    private Animator playerAnimator;
    private PlayerController playerController;
    public GameObject enemy;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy");
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player.GetComponent<Animator>();
        playerController = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(DestroyAfterTime());
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            var damageable = collision.gameObject.GetComponent<IDamageable>();
            float distance = Vector3.Distance(enemy.transform.position, player.transform.position);
            if (damageable != null && distance < 2f)
            {
                damageable.TakeDamage(30, enemy.transform);
            }
            else 
            {
                damageable.TakeDamage(30, transform);
            }
        }
        else if (collision.collider.CompareTag("Shield") && playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayerBlock")) 
        {
            playerAnimator.SetTrigger("block_hit_trigger");
            playerController.PlayClip();
        }
        Destroy(gameObject);
    }
    IEnumerator DestroyAfterTime() 
    {
        yield return new WaitForSeconds(2);
    }
}
