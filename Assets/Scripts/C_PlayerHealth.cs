using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class C_PlayerHealth : MonoBehaviour
{
    public float Health;    //玩家的生命值
    public AudioClip OnHitClip;
    public AudioClip OnDeathClip;
    public Slider PlayerHealthSlider;    //玩家的血条
    public bool isDead;
    private AudioSource audioSource;
    private Animator animator;

    public void onDamageGet(float damage)    //玩家受到攻击
    {
        if (!isDead)
        {
            Health -= damage;
            if (Health <= 0 && !isDead)
            {
                onDeath();
            }
            else
            {
                onHit();
            }
        }
    }
    
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        setUi();
    }

    private void onHit()    //玩家被击中
    {
        if (audioSource.clip != OnHitClip)
        {
            audioSource.clip = OnHitClip;
        }

        audioSource.Play();
        animator.SetTrigger("IsHit");
    }

    private void onDeath()    //玩家死亡
    {
        if (!isDead)
        {
            if (audioSource.clip != OnDeathClip)
            {
                audioSource.clip = OnDeathClip;
            }

            isDead = true;
            animator.SetTrigger("IsDead");
            audioSource.Play();
        }
    }

    private void setUi()    //设置玩家血条
    {
        PlayerHealthSlider.value = Health;
    }
}