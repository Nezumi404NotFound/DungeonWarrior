using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DragonController : MonoBehaviour, IDamageable
{
    private Animator animator;
    public Camera bossRoomCamera;
    private AudioSource audioSource;
    public AudioClip screamClip;
    public GameObject player;
    private bool attackPermission = true;
    public GameObject bitePoint;
    public GameObject firePoint;
    private bool biteDetect = false;
    private bool isFiring = false;
    protected float maxHP;
    protected float currentHP;
    public Slider slider;
    public GameObject musicManager;
    public AudioClip BGM;
    public GameObject sceneManager;
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
        maxHP = 100f;
        currentHP = maxHP;
        slider.maxValue = maxHP;
        slider.value = currentHP;
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
        currentHP -= (int)amount;
        slider.value = currentHP;
        if (currentHP <= 0) 
        {
            Dead();
        }
    }
    void Dead() 
    {
        currentState = EnemyState.dead;
        animator.SetTrigger("dead_trigger");
        StartCoroutine(WhenDeadSetSound());
        this.enabled = false;
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
                // スムーズな遷移：Quaternion.Slerp関数は2つの回転間を球面線形補間し、新しい回転を返す。第3引数で補間の度合いを制御し、0ならstartRotation、1ならtargetRotationを返す。
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
                // 経過時間：durationに達するまで、elapsedの値を徐々に増加させる
                elapsed += Time.deltaTime;
                yield return null;
            }
            // 最終的な回転が目標の回転（targetRotation）になることを保証する
            transform.rotation = targetRotation;
        }
    }
    public IEnumerator DetectFire()
    {
        while (true)
        {
            if (isFiring) yield break;
            // シーン内に炎が存在するか検出
            if (firePoint.activeInHierarchy)
            {
                // ドラゴンブレスの攻撃検出
                RaycastHit hit;
                if (Physics.SphereCast(firePoint.transform.position, 2.0f, firePoint.transform.forward, out hit, 10f))
                {
                    if (hit.collider.CompareTag("Shield")) 
                    {
                        var playerAnimator = hit.collider.GetComponentInParent<Animator>();
                        if (playerAnimator != null)
                        {
                            playerAnimator.SetTrigger("block_hit_trigger");
                            yield return new WaitForSeconds(0.05f);
                        }
                    }
                    else if (hit.collider.CompareTag("Player"))
                    {
                        isFiring = true;
                        Applydamage(hit.collider.gameObject, 20);
                        yield return new WaitForSeconds(1f);
                        isFiring = false;
                    }
                }
            }
            yield return new WaitForSeconds(0.02f);
        }
    }
    public IEnumerator DetectBite() 
    {
        while (true) 
        {   
            if (biteDetect) 
            {
                Collider[] hitColliders = Physics.OverlapSphere(bitePoint.transform.position, 1.0f);
                foreach (Collider collider in hitColliders) 
                {
                    var playerAnimator = collider.GetComponentInParent<Animator>();
                    if (collider.CompareTag("Shield") && playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Block")) 
                    {
                        playerAnimator.SetTrigger("block_hit_trigger");
                        yield return new WaitForSeconds(1f);
                        break;
                    }
                    else if (collider.CompareTag("Player")) 
                    {
                        Applydamage(collider.gameObject, 30);
                        yield return new WaitForSeconds(1f);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.02f);
        }
    }
    IEnumerator WhenDeadSetSound() 
    {
        audioSource.PlayOneShot(screamClip, 0.5f);
        AudioSource sceneAudioSouce = musicManager.GetComponent<AudioSource>();
        float startTime = 0f;
        float fadeDuration = 1.5f;
        while (startTime < fadeDuration)
        {
            startTime += Time.deltaTime;
            sceneAudioSouce.volume -= Time.deltaTime * 0.1f;
            yield return null;
        }
        sceneAudioSouce.volume = 1f;
        sceneAudioSouce.generator = BGM;
        sceneAudioSouce.Stop();
        sceneAudioSouce.Play();
        MainSceneManager mainSceneManager = sceneManager.GetComponent<MainSceneManager>();
        mainSceneManager.isFinish = true;
    }
}

