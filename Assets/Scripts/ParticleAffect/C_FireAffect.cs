using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_FireAffect : C_ParticleAffectInterface
{
    public float Damage = 50;
    public float BlastRange = 10;

    public override void TriggerAffect()
    {
        Collider[] Enemies = Physics.OverlapSphere(transform.position, BlastRange, LayerMask.GetMask("Enemy"));
        for (int i = 0; i < Enemies.Length; i++)
        {
            C_Enemy enemy = Enemies[i].GetComponent<C_Enemy>();
            enemy.StrikeDamage(Damage);
        }
    }
}