using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BoneController : MonoBehaviour,  IDamageable
{
    public Animator animator;
    protected Rigidbody rb;
    public float speed = 2f;
    protected bool startAINaVi = false;
    protected GameObject target;
    public NavMeshAgent agent;
    public GameObject stepCheak;
    public Transform rayStart;
    public Transform rayEnd;
    protected float radius = 0.2f;
    protected LayerMask targetMask;
    protected List<GameObject> hitTargets = new List<GameObject>();
    protected bool startDetectCollision = false;
    public int attackIndex = 1;
    protected int attackDamage = 0;
    protected string attackTriggerName = "";
    protected enum EnemyState
    {
        Chasing,
        Attack,
        Dead
    }
    protected EnemyState currentState = EnemyState.Chasing;
    protected GameObject player;
    protected Animator playerAnimator;
    protected AudioSource enemytAudioSource;
    private PlayerController playerController;
    public AudioClip slashClip;
    public AudioClip deathClip;
    public AudioClip screamClip;
    protected float maxHP;
    protected float currentHP;
    public Slider slider;
    private bool attackPermission;
    //接口实现
    void IDamageable.TakeDamage(float amount, Transform transform)
    {
        StartCoroutine(SmoothLookAt(transform.position));
        animator.SetTrigger("damage_trigger");
        currentHP -= (int)amount;
        slider.value = currentHP;
        if (currentHP <= 0)
        {
            Dead();
            currentState = EnemyState.Dead;
        }
    }
    // startis called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        targetMask = LayerMask.GetMask("Player", "Shield");
        attackDamage = 10;
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player.GetComponent<Animator>();
        playerController = player.GetComponent<PlayerController>();
        enemytAudioSource = GetComponent<AudioSource>();
        maxHP = 30;
        currentHP = maxHP;
        slider.maxValue = maxHP;
        slider.value = currentHP;
        StartCoroutine(Scream());
        StartCoroutine(EnemyStateRuntine());
        target = GameObject.FindGameObjectWithTag("Player");
        attackPermission = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (startDetectCollision) { DetectCollision(); }
        StepClimb();
        RefreshAttackIndex(2);
    }
    void FixedUpdate() 
    {
        if (startAINaVi == false)
        {
            Vector3 rbVelocity = transform.forward * speed;
            rb.linearVelocity = new Vector3(rbVelocity.x, rb.linearVelocity.y, rbVelocity.z);
            animator.SetFloat("speed", rbVelocity.z);
        }
    }
    public void PlayClip() 
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("BoneScream")) 
        {
            enemytAudioSource.PlayOneShot(screamClip,0.5f);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) 
        {
            enemytAudioSource.PlayOneShot(slashClip,0.7f);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("BoneDeath")) 
        {
            enemytAudioSource.PlayOneShot(deathClip);
        }
    }
    public void RefreshAttackIndex(int n)
    {
        // 攻撃インデックスが上限を超えた場合、または最後の攻撃から4秒以上経過した場合は1を返す（攻撃インデックスは攻撃アニメーションに対応）
        if (attackIndex > n)
        {
            attackIndex = 1;
        }
        attackTriggerName = "attack_trigger" + attackIndex;
    }
    public void Dead()
    {
        animator.SetTrigger("death_trigger");
        StartCoroutine(DestroyAfterDeath());
    }
    public void StepClimb()
    {
        Vector3 stepCheakPoint = stepCheak.transform.position;
        float stepHeight = 0.5f;
        float climbSmooth = 5f;
        // 低い位置のレイキャスト検知
        RaycastHit hitLower;
        if (Physics.Raycast(stepCheakPoint, transform.forward, out hitLower, 0.5f))
        {
            // 高い位置のレイキャスト検知
            RaycastHit hitupper;
            if (!Physics.Raycast(stepCheakPoint + new Vector3(0, stepHeight, 0), transform.forward, out hitupper, 1f))
            {
                rb.position += new Vector3(0, climbSmooth * Time.deltaTime, 0);
            }
        }
    }
    public void DetectCollision()
    {
        // レイキャストによる敵の検出
        Vector3 direction = rayEnd.position - rayStart.position;
        float distance = direction.magnitude;
        RaycastHit[] hits = Physics.SphereCastAll(rayStart.position, radius, direction.normalized, distance, targetMask);
        foreach (RaycastHit hit in hits)
        {
            GameObject target = hit.collider.gameObject;
            if (!hitTargets.Contains(target))
            {
                Vector3 attackDirection = (transform.position - player.transform.position).normalized;
                float dot = Vector3.Dot(player.transform.forward, attackDirection);
                if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Block") && target.gameObject.layer == LayerMask.NameToLayer("Shield") && dot > 0.5f)
                {
                    playerAnimator.SetTrigger("block_hit_trigger");
                    hitTargets.Add(target);
                    startDetectCollision = false;
                    break;
                }
                else if (target.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    ApplyDamage(target, hit.point);
                    hitTargets.Add(target);
                    startDetectCollision = false;
                    break;
                }
            }
        }
    }
    // 状態遷移機械（ステートマシン）
    public void EnemyStateSwitch()
    {
        if (agent.enabled)
        {
            switch (currentState)
            {
                case EnemyState.Chasing:
                    agent.SetDestination(target.transform.position);
                    animator.SetFloat("speed", agent.velocity.magnitude);
                    break;
                case EnemyState.Attack:
                    if(agent.hasPath)
                    {
                        agent.ResetPath();
                    }
                    if (attackPermission)
                    {
                        animator.SetFloat("speed", 0);
                        hitTargets.Clear();
                        animator.SetTrigger(attackTriggerName);
                        attackPermission = false;
                        StartCoroutine(AttackCoolDown(2));
                    }
                    break;
                case EnemyState.Dead:
                    agent.enabled = false;
                    animator.SetFloat("speed", 0);
                    break;
            }

        }
    }
    public void StartDetectCollision()
    {
        startDetectCollision = true;
    }
    public void StopDetectCollision()
    {
        startDetectCollision = false;
    }
    private void ApplyDamage(GameObject target, Vector3 hitPoint)
    {

        var damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(attackDamage, transform);
        }
    }
    public IEnumerator Scream()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("scream_trigger");
        animator.SetFloat("speed", 0);
        startAINaVi = true;
        StartCoroutine(EnemyNavi());
    }
    public IEnumerator EnemyNavi()
    {
        yield return new WaitForSeconds(1f);
        agent.enabled = true;
    }
    // 攻撃者へスムーズに振り向くコルーチン
    public IEnumerator SmoothLookAt(Vector3 targetPosition)
    {
        float elapsed = 0f;
        float duration = 0.15f;
        Quaternion startRotation = transform.rotation;
        // 水平方向の計算
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            while (elapsed < duration)
            {
                //平滑过渡，Quaternion.Slerp函数在两个旋转之间进行球面线性插值，返回一个新的旋转，第三个参数控制插值的程度，0返回startRotation，1返回targetRotation
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
                // 経過時間：durationに達するまで、elapsedの値を徐々に増加させる
                elapsed += Time.deltaTime;
                yield return null;
            }
            // 最終的な回転が目標の回転（targetRotation）になることを保証する
            transform.rotation = targetRotation;
        }
    }
    public IEnumerator EnemyStateRuntine()
    {
        while (true)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToPlayer <= 1.5f)
            {
                currentState = EnemyState.Attack;
            }
            else
            {
                currentState = EnemyState.Chasing;
            }
            EnemyStateSwitch();
            yield return new WaitForSeconds(0.2f);
        }
    }
    public IEnumerator AttackCoolDown(float n) 
    {
       yield return new WaitForSeconds(n);
       attackPermission = true;
        
    }
    public IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
