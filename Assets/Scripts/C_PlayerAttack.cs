using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class C_PlayerAttack : MonoBehaviour
{
    public Button PickButton;
    public Button FireButton;
    public Transform BallPosition; //获取球之后球漂浮的位置
    public float HoldingRange; //获取球的最小半径
    public float FireForce; //发射球体时的力量
    public AudioClip[] FireAudios;

    private Animator animator;
    private AudioSource audioSource;
    private Text PickText;
    private C_Ball holdingBall; //拾取到的球
    private Collider[] colliders;
    private bool isPickEnabled;
    private bool isHolding;

    public void getHoldingBall() //从范围内拾取最近的球
    {
        if (isHolding)
        {
            DropBall();
        }
        else
        {
            if (colliders.Length > 0)
            {
                Collider target = colliders[0];
                float distance = (colliders[0].transform.position - transform.position).sqrMagnitude;
                for (int i = 1; i < colliders.Length; i++)
                {
                    float tempDistance = (colliders[i].transform.position - transform.position).sqrMagnitude;
                    if (tempDistance < distance)
                    {
                        distance = tempDistance;
                        target = colliders[i];
                    }
                }

                isHolding = true;
                holdingBall = target.GetComponent<C_Ball>();
                holdingBall.HoldBallToPosition(BallPosition);
            }
        }
    }

    public void Fire() //将手中的球体发射出去
    {
        if (isHolding)
        {
            isHolding = false;
            if (FireAudios.Length > 0)
            {
                audioSource.clip = FireAudios[Random.Range(0, FireAudios.Length)];
                audioSource.Play();
            }
            animator.SetTrigger("IsPushing");
            holdingBall.Fire(transform.forward * FireForce);
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        PickText = PickButton.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        CollidersCheck();
        setUI();

        if (Input.GetButtonDown("BallPick") && isPickEnabled)
        {
            getHoldingBall();
        }
        else if (Input.GetButton("BallFire") && isHolding)
        {
            Fire();
        }
    }

    private void setUI()
    {
        if (isPickEnabled)
        {
            PickButton.gameObject.SetActive(true);
        }
        else
        {
            PickButton.gameObject.SetActive(false);
        }

        if (isHolding)
        {
            FireButton.gameObject.SetActive(true);
            PickText.text = "抛下";
        }
        else
        {
            FireButton.gameObject.SetActive(false);
            PickText.text = "拾取";
        }
    }

    private void CollidersCheck() //进行碰撞检查范围内是否可以拾取球体
    {
        isPickEnabled = false;
        colliders = Physics.OverlapSphere(transform.position, HoldingRange,
            LayerMask.GetMask("Ball")); //在"Ball"这个Layer下检查是否有球体在范围内
        if (colliders.Length > 0)
        {
            isPickEnabled = true;
        }
    }

    private void DropBall() //抛下球体
    {
        isHolding = false;
        holdingBall.DropBall();
        holdingBall = null;
    }

    private void OnDrawGizmos() //测试用,绘制拾取半径
    {
        Gizmos.DrawWireSphere(transform.position, HoldingRange);
    }
}