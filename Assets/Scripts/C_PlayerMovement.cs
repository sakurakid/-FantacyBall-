using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用于控制玩家角色移动的脚本
public class C_PlayerMovement : MonoBehaviour
{
    public float MaxSpeed; //玩家最高速度
    public float RotationTime = 0.1f; //玩家从一个角度移动到另一个角度需要的时间
    public Transform CameraTransform; //摄像机的位置
    public Joystick MovementJoyStick; //控制玩家移动的虚拟摇杆的位置

    private Animator Animator;
    private Rigidbody rigidbody;
    private C_PlayerHealth playerHealth;

    private string horizontalString = "Horizontal";
    private string verticalString = "Vertical";
    private float horizontalValue;
    private float verticalValue;
    private Vector3 moveVector;    //移动矩阵

    private void Start()
    {
        Animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        playerHealth = GetComponent<C_PlayerHealth>();
    }

    private void Update()
    {
//        getAxisValueFromKey();
        getAxisValueFromJoyStick();
        if (!playerHealth.isDead) //如果玩家死亡将禁止所有移动
        {
            RotateCharacter();
            moveCharacter();
        }
    }

    //从键盘获取玩家输入
    private void getAxisValueFromKey()
    {
        horizontalValue = Input.GetAxis(horizontalString);
        verticalValue = Input.GetAxis(verticalString);
        moveVector = new Vector3(horizontalValue, 0f, verticalValue);
    }

    //从虚拟摇杆获取玩家输入
    private void getAxisValueFromJoyStick()
    {
        horizontalValue = MovementJoyStick.Horizontal;
        verticalValue = MovementJoyStick.Vertical;
        moveVector = new Vector3(horizontalValue, 0f, verticalValue);
    }

    //玩家输入来移动角色并且配置移动动画
    private void moveCharacter()
    {
        float speedValue = moveVector.magnitude * MaxSpeed;
        Animator.SetFloat("Speed", speedValue);
        Vector3 fowardTransform = transform.forward * speedValue * Time.deltaTime;
        transform.position = rigidbody.position + fowardTransform;
    }

    //组合摄像机位置和玩家输入来决定角色移动方向
    private void RotateCharacter()
    {
        if (moveVector != Vector3.zero)
        {
            float rotationOffset = CameraTransform.eulerAngles.y;    //获取摄像机相对于角色在Y轴旋转的角度
            Quaternion targetQuaternion =
                Quaternion.Euler(0f, rotationOffset, 0f) * Quaternion.LookRotation(moveVector);    //将摄像机的角度组合上移动矩阵的角度
            transform.rotation = Quaternion.Slerp(transform.rotation, targetQuaternion, RotationTime);    //在一定时间内将角色转向指定位置
        }
    }
}