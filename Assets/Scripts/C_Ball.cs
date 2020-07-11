using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_Ball : MonoBehaviour
{
    public float Damage;    //球体击中伤害
    public ParticleSystem HitPartical;    //球体击中时的粒子效果
    public C_GameManager GameManager;
    public string[] ContentArray;    //对于球体的语言描述
    public bool isKnown;    //球体是否已知
    
    private Rigidbody rigidbody;
    private bool isHolding;
    private Transform targetPosition;
    private Vector3 currentSpeed;
    private bool isFired;    //球体是否被发射

    public void Fire(Vector3 force)    //将球体从当前位置发射
    {
        isHolding = false;
        rigidbody.isKinematic = false;
        isFired = true;
        targetPosition = null;
        rigidbody.AddForce(force);
        StartCoroutine(isFireReverse());
    }

    public void HoldBallToPosition(Transform targetPosition)    //将球体拾取到指定位置
    {
        isHolding = true;
        rigidbody.isKinematic = true;
        this.targetPosition = targetPosition;
    }

    public void DropBall()    //将球体抛下
    {
        isHolding = false;
        rigidbody.isKinematic = false;
        targetPosition = null;
    }

    private void Awake()
    {
        isHolding = false;
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isHolding)
        {
            transform.position =
                Vector3.SmoothDamp(transform.position, targetPosition.position, ref currentSpeed, 0.1f);
        }
    }

    private void OnCollisionEnter(Collision other)    //击中目标
    {
        if (isFired && other.gameObject.CompareTag("Enemy"))
        {
            Debug.Log(rigidbody.velocity.magnitude + "Hit");
            other.gameObject.GetComponent<C_Enemy>().StrikeDamage(Damage);
            if (HitPartical != null)
            {
                HitPartical.transform.parent = null;
                HitPartical.Play();
                HitPartical.GetComponent<AudioSource>().Play();
                C_ParticleAffectInterface particleAffect = HitPartical.GetComponent<C_ParticleAffectInterface>();
                if (particleAffect != null)
                {
                    particleAffect.TriggerAffect();    //触发球体自带的效果伤害
                }
                Destroy(HitPartical, HitPartical.duration);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)    //玩家进入介绍范围
    {
        if (!isKnown && other.gameObject.CompareTag("Player"))
        {
            isKnown = true;
            GameManager.SetDialogContent(ContentArray);
        }
    }

    private IEnumerator isFireReverse()    //发射一段时间后将无法继续攻击敌人
    {
        yield return new WaitForSeconds(6);
        isFired = false;
    }
}