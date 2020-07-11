using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_NPC : MonoBehaviour
{
    public string[] ContentArray;
    public C_GameManager GameManager;
    private bool isTalked = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManager.SetDialogContent(ContentArray);
        }
    }
}