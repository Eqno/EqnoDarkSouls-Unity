using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardMapping : MonoBehaviour
{
    // 键位设置
    [Header("Keyboard Settings")]
    public string SwitchGameMode = "\\";
    public string CameraUp = "up";
    public string CameraDown = "down";
    public string CameraLeft = "left";
    public string CameraRight = "right";
    public string MoveForward = "w";
    public string MoveBackword = "s";
    public string MoveLeftward = "a";
    public string MoveRightward = "d";
    public string PressToRun = "left shift";
    public string TriggerToJump = "space";
    public string TriggerToRoll = "left ctrl";
    public string PressToDefense = "f";
    public string RightAttack = "e";
    public string LeftAttack = "q";
    public string TriggerToCrouch = "c";
    public string TriggerToLockOn = "t";
    public string TriggerToSwithTarget = "tab";
    public string TriggerToCloseGame = "escape";
}
