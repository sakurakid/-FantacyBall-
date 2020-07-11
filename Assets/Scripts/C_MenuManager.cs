using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class C_MenuManager : MonoBehaviour
{

    public void LoadNextScene()
    {
        SceneManager.LoadSceneAsync("FirstScene");
    }
}