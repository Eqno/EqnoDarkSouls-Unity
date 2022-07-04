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
    public string PressToDefense = "Button2";
    public string LeftSlash = "Button4";
    public string RightSlash = "Button5";
    public string LeftAttack = "Axis3";
    public string RightAttack = "Axis3";
    public string TriggerToCrouch = "Button8";
    public string DoubleTriggerToLockOn = "Button9";
    public string TriggerToSwithTargetFrontAndBack = "Axis5";
    public string TriggerToSwithTargetLeftAndRight = "Axis4";
    public string TriggerToCloseGame = "Button6";
}
