using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_MonsterTrigger : MonoBehaviour
{
    public C_Enemy[] TriggerEnemys;
    private bool isTriggered;

    private void OnTriggerEnter(Collider other)
    {
        isTriggered = true;
        if (other.gameObject.CompareTag("Player"))
        {
            for (int i = 0; i < TriggerEnemys.Length; i++)
            {
                TriggerEnemys[i].gameObject.SetActive(true);
            }

            gameObject.SetActive(false);
        }
    }
}