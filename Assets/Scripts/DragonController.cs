using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class DragonController : MonoBehaviour, IDamageable
{
    private Animator animator;
    public Camera bossRoomCamera;
    private bool isFlying = false;
    private AudioSource audioSource;
    public AudioClip screamClip;
    public GameObject player;
    private bool attackPermission = true;
    public GameObject bitePoint;
    public GameObject firePoint;
    private bool biteDetect = false;
    private bool isFiring = false;
    private bool isBiting = false;
    enum EnemyState
    {
        idle,
        bite,
        drakaris,
        fly,
        dead
    }
    private EnemyState currentState;
    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        firePoint.SetActive(false);
        StartCoroutine(DetectBite());
        StartCoroutine(DetectFire());
    }

    // Update is called once per frame
    void Update()
    {
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
            if (distance <= 5f)
            {
                currentState = EnemyState.bite;
            }
            else if (distance > 5f && distance < 9f)
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
                case EnemyState.bite:
                    StartCoroutine(SmoothLookAt(player.transform.position));
                    animator.SetTrigger("bite_trigger");
                    attackPermission = false;
                    StartCoroutine(AttackCoolDown(4));
                    break;
                case EnemyState.drakaris:
                    StartCoroutine(SmoothLookAt(player.transform.position));
                    animator.SetTrigger("drakaris_trigger");
                    attackPermission = false;
                    StartCoroutine(AttackCoolDown(4));
                    break;
            }
        }
    }
    public void AnimationEventSetFireActiveTrue()
    {
        firePoint.SetActive(true);
    }
    public void AnimationEventSetFireActiveFalse()
    {
        firePoint.SetActive(false);
    }
    public void AnimationEventSetBiteDetectTrue()
    {
        biteDetect = true;
    }
    public void AnimationEventSetBiteDetectFalse()
    {
        biteDetect = false;
    }
    public void Applydamage(GameObject target, float damage)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, transform);
        }
    }
    void IDamageable.TakeDamage(float amount, Transform transform)
    {
        throw new System.NotImplementedException();
    }
    IEnumerator AttackCoolDown(float n)
    {
        yield return new WaitForSeconds(n);
        attackPermission = true;
    }
    public IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        float elapsed = 0f;
        float duration = 0.15f;
        Quaternion startRotation = transform.rotation;
        //计算水平方向
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            while (elapsed < duration)
            {
                //平滑过渡，Quaternion.Slerp函数在两个旋转之间进行球面线性插值，返回一个新的旋转，第三个参数控制插值的程度，0返回startRotation，1返回targetRotation
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
                //增量时间，逐渐增加elapsed的值，直到达到duration
                elapsed += Time.deltaTime;
                yield return null;
            }
            //确保最终旋转是目标旋转
            transform.rotation = targetRotation;
        }
    }
    public IEnumerator DetectFire()
    {
        while (true)
        {
            if (isFiring) yield break;
            //检测场景是否存在火焰
            if (firePoint.activeInHierarchy)
            {
                //龙焰攻击检测
                RaycastHit hit;
                if (Physics.SphereCast(firePoint.transform.position, 2.0f, firePoint.transform.forward, out hit, 10f))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        isFiring = true;
                        Applydamage(hit.collider.gameObject, 20);
                        yield return new WaitForSeconds(1f);
                        isFiring = false;
                    }
                }
            }
            yield return null;
        }
    }
    public IEnumerator DetectBite() 
    {
        while (true) 
        {   
            if(isBiting) yield break;
            if (biteDetect) 
            {
                Collider[] hitColliders = Physics.OverlapSphere(bitePoint.transform.position, 1.0f);
                foreach (Collider collider in hitColliders) 
                {
                    if (collider.CompareTag("Player")) 
                    {
                        isBiting = true;
                        Applydamage(collider.gameObject, 30);
                        yield return new WaitForSeconds(1f);
                        isBiting = false;
                    }
                }
            }
            yield return null;
        }
    }
}

