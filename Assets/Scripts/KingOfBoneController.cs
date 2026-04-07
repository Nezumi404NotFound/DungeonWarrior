using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class KingOfBoneController : MonoBehaviour, IDamageable
{
    private Rigidbody rb;
    private float speed = 2.0f;
    public Animator animator;
    private bool startAINavi;
    public GameObject stepCheck;
    protected GameObject target;
    public NavMeshAgent agent;
    protected LayerMask targetMask;
    protected bool startDetectCollision = false;
    protected int attackDamage = 0;
    protected enum EnemyState
    {
        Chasing,
        Attack,
        Dead
    }
    protected EnemyState currentState = EnemyState.Chasing;
    protected float lastAttackTime = 0f;
    protected float attackCoolDown = 4f;
    protected GameObject player;
    protected Animator playerAnimator;
    protected AudioSource playerAudioSource;
    protected AudioSource enemytAudioSource;
    public AudioClip screamClip;
    public AudioClip deadClip;
    public AudioClip fireClip;
    protected float maxHP;
    protected float currentHP;
    public Slider slider;
    public GameObject fireBall;
    public Transform firePosition;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startAINavi = false;
        target = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        targetMask = LayerMask.GetMask("Player", "Shield");
        attackDamage = 10;
        player = GameObject.FindGameObjectWithTag("Player");
        playerAnimator = player.GetComponent<Animator>();
        playerAudioSource = player.GetComponent<AudioSource>();
        enemytAudioSource = GetComponent<AudioSource>();
        maxHP = 30;
        currentHP = maxHP;
        slider.maxValue = maxHP;
        slider.value = currentHP;
        StartCoroutine(Scream());
        StartCoroutine(EnemyStateRuntine());
    }

    // Update is called once per frame
    void Update()
    {
        StepClimb();
    }
    private void FixedUpdate()
    {
        if (startAINavi == false) 
        {
            Vector3 rbVelocity = transform.forward * speed;
            rb.linearVelocity = new Vector3(rbVelocity.x, rb.linearVelocity.y, rbVelocity.z);
            animator.SetFloat("speed", rbVelocity.z);
        }
    }
    public void StepClimb()
    {
        Vector3 stepCheakPoint = stepCheck.transform.position;
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
                        animator.SetTrigger("attack_trigger");
                        StartCoroutine(SmoothLookAt(player.transform.position));
                    }
                    break;
                case EnemyState.Dead:
                    agent.enabled = false;
                    animator.SetFloat("speed", 0);
                    break;
            }

        }
    }
    public void PlayClip()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("KOBScream"))
        {
            enemytAudioSource.PlayOneShot(screamClip, 0.5f);
        }
        else if (animator.GetCurrentAnimatorStateInfo(0).IsName("KOBAttack"))
        {
            enemytAudioSource.PlayOneShot(fireClip);
        }
        ;
    }
    public void Fire() 
    {
        Vector3 targetPosition = player.transform.position + new Vector3(0,1,0);
        Vector3 fireDirection = (targetPosition - firePosition.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(fireDirection);
        GameObject newFireBall = Instantiate(fireBall, firePosition.position, targetRotation);
        Rigidbody rb = newFireBall.GetComponent<Rigidbody>();
        rb.AddForce(fireDirection * 200, ForceMode.Force);
    }
    public void Dead() 
    {
        animator.SetTrigger("death_trigger");
        StartCoroutine(DestroyAfterDeath());
    }
    public IEnumerator Scream()
    {
        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("scream_trigger");
        animator.SetFloat("speed", 0);
        startAINavi = true;
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
            if (distanceToPlayer <= 5f)
            {
                currentState = EnemyState.Attack;
            }
            else if (distanceToPlayer > 5.5f)
            {
                currentState = EnemyState.Chasing;
            }
            EnemyStateSwitch();
            yield return new WaitForSeconds(0.1f);
        }
    }
    public IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
        
