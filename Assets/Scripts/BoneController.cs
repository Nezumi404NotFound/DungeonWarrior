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
    protected float lastAttackTime = 0f;
    protected float attackCoolDown = 2f;
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
        //如果超出攻击索引则返回1，攻击索引对应攻击动画；或者如果距离上次攻击时间超过4秒也返回1
        if (attackIndex > n || Time.time - lastAttackTime > 4)
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
        //低射线检测
        RaycastHit hitLower;
        if (Physics.Raycast(stepCheakPoint, transform.forward, out hitLower, 0.5f))
        {
            //高射线检测
            RaycastHit hitupper;
            if (!Physics.Raycast(stepCheakPoint + new Vector3(0, stepHeight, 0), transform.forward, out hitupper, 1f))
            {
                rb.position += new Vector3(0, climbSmooth * Time.deltaTime, 0);
            }
        }
    }
    public void DetectCollision()
    {
        //射线检测敌人
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
    //状态机
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
                    if (Time.time > lastAttackTime + attackCoolDown)
                    {
                        lastAttackTime = Time.time;
                        animator.SetFloat("speed", 0);
                        hitTargets.Clear();
                        animator.SetTrigger(attackTriggerName);

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
    //平滑转向攻击者携程
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
    public IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
