using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_IceAffect : C_ParticleAffectInterface
{
    public float Times;
    public float DamagePerTime;
    public float TimeDuration;
    public float IceRange;

    public override void TriggerAffect()
    {
        StartCoroutine(IceDamage());
    }

    private IEnumerator IceDamage()
    {
        while (Times > 0)
        {
            Collider[] Enemies =
                Physics.OverlapSphere(transform.position, IceRange, LayerMask.GetMask("Enemy"));
            for (int i = 0; i < Enemies.Length; i++)
            {
                C_Enemy enemy = Enemies[i].GetComponent<C_Enemy>();
                enemy.SmoothDamage(DamagePerTime);
            }
            Times--;
            yield return new WaitForSeconds(0.5f);
        }
    }
}