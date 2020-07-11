using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class C_CameraControl : MonoBehaviour
{
    public float RotationSpeed = 180;    //摄像机最大旋转速度
    public float CameraRotationX = 30;    //摄像机在X轴方向偏移的角度
    public Joystick CameraJoyStick;
    public Transform target;
    private float rotationY;
    private Vector3 offset;
    private string hString = "HorizontalCamera";
    private float horizontalValue;


    private void Start()
    {
        rotationY = transform.eulerAngles.y;
        offset = target.position - transform.position;
    }

    private void LateUpdate()
    {
        getInputFromJoyStick();
        if (horizontalValue != 0)
        {
            rotationY += horizontalValue * RotationSpeed * Time.deltaTime;
        }

        Quaternion rotation = Quaternion.Euler(0, rotationY, 0);
        transform.position = target.position - (rotation * offset);    //将摄像机移动到合适的位置
        transform.LookAt(target);    //让摄像机看向目标
        setXAxis();
    }

    private void setXAxis()    //设定摄像机相对于X轴旋转的偏移角
    {
        Quaternion rotationX = Quaternion.Euler(-CameraRotationX, 0f, 0f);
        transform.rotation *= rotationX;
    }

    private void getInputFromJoyStick()    //通过虚拟键盘获取输入
    {
        horizontalValue = CameraJoyStick.Horizontal;
    }

    private void getInputFromKey()
    {
        horizontalValue = Input.GetAxis(hString);
    }
}