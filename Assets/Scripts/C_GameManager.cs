using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class C_GameManager : MonoBehaviour
{
    public Button Dialog;    //对话框
    public AudioClip DialogClip;    //对话框音效
    public Joystick MovementJoyStick;    //控制移动的摇杆
    public Joystick CameraJoyStick;    //控制摄像机的摇杆
    public string ThisSceneName;
    public string NextSceneName;
    public float TextDuration = 0.02f;    //对话框出字速度
    public C_PlayerHealth PlayerHealth;
    public C_Enemy[] Enemys;

    public AudioSource MusicSource;
    public AudioSource UISource;

    public string[] StartContent;

    private Text dialogText;
    private bool isDialogShowing;
    private bool isFinish;

    private bool isSkip;


    public void SetDialogContent(string[] contents)    //向对话框中展示内容
    {
        StartCoroutine(ShowDialog(contents));
    }

    public void ClickDialog()    //点击对话框
    {
        if (!isFinish)
        {
            isFinish = true;
        }
        else if (isFinish && !isSkip)
        {
            isSkip = true;
        }
    }

    private void Start()
    {
        dialogText = Dialog.GetComponentInChildren<Text>();
        setDialogVisibility(false);
        SetDialogContent(StartContent);
        StartCoroutine(gameLoop());
    }

    private IEnumerator gameLoop()    //游戏主循环
    {
        yield return StartCoroutine(roundPlaying());
        yield return new WaitForSeconds(3);
        if (PlayerHealth.isDead)    //如果玩家死亡
        {
            Debug.Log("ReLoadScene");
            SceneManager.LoadScene(ThisSceneName);
        }
        else
        {
            SceneManager.LoadScene(NextSceneName);
        }
    }

    private IEnumerator roundPlaying()    //当前关卡运行中
    {
        while (!enemyCleared() && !playerDead())
        {
            yield return null;
        }
    }

    private bool playerDead()
    {
        return PlayerHealth.isDead;
    }

    private bool enemyCleared()    //判断敌人是否被清除
    {
        int enemyLeft = 0;
        for (int i = 0; i < Enemys.Length; i++)
        {
            if (!Enemys[i].isDead)
            {
                enemyLeft++;
            }
        }

        return enemyLeft == 0;
    }

    private IEnumerator playDialogSound()    //控制对话框音效
    {
        if (UISource.clip != DialogClip)
        {
            UISource.clip = DialogClip;
        }

        while (!isFinish)
        {
            yield return new WaitForSeconds(0.1f);
            if (UISource.isPlaying)
            {
                UISource.Stop();
            }

            UISource.Play();
        }
    }

    private IEnumerator ShowDialog(string[] contents)    //控制对话框出字
    {
        setJoyStickVisibility(false);
        setDialogVisibility(true);
        StringBuilder stringBuilder = new StringBuilder();
        string dialogContent;
        for (int j = 0; j < contents.Length; j++)    //循环所有字符串
        {
            isSkip = false;
            isFinish = false;
            StartCoroutine(playDialogSound());    //开启多线程播放音效
            dialogContent = contents[j];
            for (int i = 0; i < dialogContent.Length; i++)    //逐字显示
            {
                stringBuilder.Append(dialogContent[i]);
                yield return new WaitForSeconds(TextDuration);
                dialogText.text = stringBuilder.ToString();
                if (isFinish)
                {
                    break;
                }
            }

            isFinish = true;
            dialogText.text = dialogContent;
            stringBuilder.Clear();
            yield return StartCoroutine(DialogClicked());    //显示完成一句之后等待玩家点击对话框
        }

        dialogText.text = "";
        setDialogVisibility(false);
        setJoyStickVisibility(true);
    }

    private void setDialogVisibility(bool visible)
    {
        Dialog.gameObject.SetActive(visible);
    }

    private void setJoyStickVisibility(bool visible)
    {
        MovementJoyStick.gameObject.SetActive(visible);
        CameraJoyStick.gameObject.SetActive(visible);
    }

    private IEnumerator DialogClicked()
    {
        while (!isSkip)
        {
            yield return null;
        }
    }    //点击对话框
}