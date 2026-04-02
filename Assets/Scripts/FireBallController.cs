using System.Collections;
using System.IO.Pipes;
using UnityEngine;

public class FireBallController : MonoBehaviour
{
    public GameObject player;
    private Animator playerAnimator;
    private PlayerController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
            if (damageable != null)
            {
                damageable.TakeDamage(30, transform);
            }
        }
        else if (collision.collider.CompareTag("Shield")) 
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
