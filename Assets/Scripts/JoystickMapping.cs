using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickMapping : MonoBehaviour
{
    // 键位设置
    [Header("Joystick Settings")]
    public string SwitchGameMode = "Button7";
    public string MoveAxisX = "AxisX";
    public string MoveAxisY = "AxisY";
    public string CameraAxisX = "Axis4";
    public string CameraAxisY = "Axis5";
    public string PressToZoom = "Button9";
    public string ZooomAxis = "Axis5";
    public string PressToRun = "Button0";
    public string TriggerToJump = "Button3";
    public string TriggerToRoll = "Button1";
    public string TriggerToSlash = "Button2";
    public string TriggerToCloseGame = "Button6";
}
