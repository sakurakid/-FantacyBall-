using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Slider = UnityEngine.UI.Slider;

public class C_Enemy : MonoBehaviour
{
    public Transform target;    //需要寻找的目标(玩家)
    public Slider HealthSlider;    //自身血条
    public ParticleSystem DeathParticle;    //死亡时候的粒子效果(如果有的话)
    public float AngerRange;    //仇恨范围
    public float AttackDuration;    //攻击间隔
    public float Health;
    public float damage;
    public AudioClip OnHitClip;
    public AudioClip OnDeadClip;
    public GameObject[] DropBalls;
    public bool debug;    //测试模式开关
    [HideInInspector] public bool isDead;

    private Animator animator;
    private NavMeshAgent agent;
    private Vector3 lastPosition;
    private AudioSource audioSource;
    private bool isTargetSeen;
    private bool isAttack;
    private float attackTimer;
    private float healthVelocity;
    private float smoothDamageV;
    private float targetHealth;

    public void StrikeDamage(float damage)    //受到直接攻击
    {
        if (damage > 0)
        {
            Health -= damage;
            if (Health <= 0 && !isDead)
            {
                onDead();
            }
            else
            {
                onHit();
            }
        }
    }

    public void SmoothDamage(float damage)    //受到持续伤害
    {
        if (damage > 0)
        {
            Health -= damage;
        }

        if (Health <= 0 && !isDead)
        {
            onDead();
        }
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        attackTimer = AttackDuration;
    }

    private void Update()
    {
        isAttack = false;
        setUI();
        if (!isDead && !debug)
        {
            checkAngerRange();
            checkAttackRange();
            if (isAttack)
            {
                onAttack();
            }

            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private void onAttack()    //发动攻击
    {
        animator.SetTrigger("IsAttack");
        Debug.Log("Hit");
        target.GetComponent<C_PlayerHealth>().onDamageGet(damage);
    }

    private void onHit()    //受到攻击时的效果
    {
        if (audioSource.clip != OnHitClip)
        {
            audioSource.clip = OnHitClip;
        }

        audioSource.Play();
        animator.SetTrigger("IsHit");
    }

    private void onDead()    //死亡时的效果
    {
        isDead = true;
        agent.SetDestination(agent.nextPosition);    //如果死亡就设置目标地点为当前位置,
        if (audioSource.clip != OnDeadClip)
        {
            audioSource.clip = OnDeadClip;
        }

        audioSource.Play();
        animator.SetTrigger("IsDead");
        if (DeathParticle != null)
        {
            DeathParticle.transform.parent = null;
            DeathParticle.Play();
            Destroy(DeathParticle, DeathParticle.duration);
            gameObject.SetActive(false); //由粒子动画遮罩,直接销毁
        }

        GenerateBall();
        StartCoroutine(OnCompleteDeadAnimation()); //等待死亡动画播完再销毁
    }

    private void GenerateBall()    //死亡掉落
    {
        int i = Random.Range(0, DropBalls.Length);
        Rigidbody dropBall = Instantiate(DropBalls[i], transform.position + new Vector3(0f, 5f, 0f), transform.rotation)
            .GetComponent<Rigidbody>();
        float xForce = Random.Range(0, 500);
        float zForce = Random.Range(0, 500);
        dropBall.AddForce(xForce, 500f, zForce);
    }

    private void checkAttackRange()    //检查攻击范围内是否有玩家
    {
        if (isTargetSeen)
        {
            agent.SetDestination(target.position);
            if (canAttack())
            {
                isAttack = true;
            }
        }
        else
        {
            if (LostTarget())
            {
                //如果需要加入巡逻功能的话
            }
        }
    }

    private void checkAngerRange()    //检查仇恨范围内是否有玩家
    {
        if (Mathf.Abs((target.position - transform.position).magnitude) < AngerRange)
        {
            isTargetSeen = true;
            lastPosition = target.position;
        }
        else
        {
            isTargetSeen = false;
        }
    }

    private void setUI()
    {
        Mathf.SmoothDamp(HealthSlider.value, Health, ref healthVelocity, 0.1f);
        HealthSlider.value = Health;
    }

    private bool canAttack()    //检查是否可以攻击
    {
        bool canAttack = false;
        if (!debug)
        {
            if (agent.remainingDistance < agent.stoppingDistance)
            {
                if (attackTimer <= 0f)
                {
                    attackTimer = AttackDuration;
                    canAttack = true;
                }
            }

            if (attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
        }

        return canAttack;
    }

    private bool LostTarget()    //检查是否丢失目标
    {
        bool lost = transform.position == lastPosition;
        return lost;
    }

    private void OnDrawGizmos()    //测试用,绘制曲线
    {
        if (target != null)
        {
            Gizmos.DrawRay(transform.position, target.position - transform.position);
            Gizmos.DrawWireSphere(transform.position, AngerRange);
        }

    }
    IEnumerator OnCompleteDeadAnimation()    //死亡回收
    {
        yield return new WaitForSeconds(3);
        gameObject.SetActive(false);
    }
}