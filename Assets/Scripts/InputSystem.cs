using UnityEngine;

public class InputSystem : MonoBehaviour
{
    [Header("Game Mode")]
    public bool JoystickMode = true;

    // GameObjects
    private KeyboardMapping Keyboard;
    private JoystickMapping Joystick;

    // 缩放
    private bool zooming = false;
    private float joyZoomMuti = 150;

    // Get ... Down
    private bool leftSlash = false, leftAttack = false, rightSlash = false, rightAttack = false;

    void Awake()
    {
        Keyboard = GetComponent<KeyboardMapping>();
        Joystick = GetComponent<JoystickMapping>();
    }
    void Update()
    {
        if (Input.GetKeyDown(Keyboard.SwitchGameMode) || Input.GetButtonDown(Joystick.SwitchGameMode)) JoystickMode = !JoystickMode;
        if (Input.GetKeyDown(Keyboard.TriggerToCloseGame) || Input.GetButtonDown(Joystick.TriggerToCloseGame)) Application.Quit();
    }
    public float GetKeyAxisXY(
        ref float anchorFore, ref float anchorRight,
        ref float forceFore, ref float forceRight,
        ref float _forceFore, ref float _forceRight, float Duration
    )
    {
        // 摇杆锚点
        anchorFore = (Input.GetKey(Keyboard.MoveForward) ? 1 : 0) - (Input.GetKey(Keyboard.MoveBackword) ? 1 : 0);
        anchorRight = (Input.GetKey(Keyboard.MoveRightward) ? 1 : 0) - (Input.GetKey(Keyboard.MoveLeftward) ? 1 : 0);
        
        // 方转圆
        float unitFore = anchorFore * Mathf.Sqrt(1 - anchorRight * anchorRight / 2);
        float unitRight = anchorRight * Mathf.Sqrt(1 - anchorFore * anchorFore / 2);

        // 平滑推动
        forceFore = Mathf.SmoothDamp(forceFore, unitFore, ref _forceFore, Duration);
        forceRight = Mathf.SmoothDamp(forceRight, unitRight, ref _forceRight, Duration);

        return Mathf.Sqrt(forceFore * forceFore + forceRight * forceRight);
    }
    public float GetJoyAxisXY(ref float forceFore, ref float forceRight)
    {
        forceFore = Input.GetAxis(Joystick.MoveAxisY);
        forceRight = Input.GetAxis(Joystick.MoveAxisX);
        return Mathf.Sqrt(forceFore * forceFore + forceRight * forceRight);
    }
    public void GetKeyAxis45(
        ref float anchorUp, ref float anchorRight,
        ref float forceUp, ref float forceRight,
        ref float _forceUp, ref float _forceRight, float KeyDuration
    )
    {
        // 摇杆锚点
        anchorUp = (Input.GetKey(Keyboard.CameraUp) ? 1 : 0) - (Input.GetKey(Keyboard.CameraDown) ? 1 : 0);
        anchorRight = (Input.GetKey(Keyboard.CameraRight) ? 1 : 0) - (Input.GetKey(Keyboard.CameraLeft) ? 1 : 0);

        // 平滑推动
        forceUp = Mathf.SmoothDamp(forceUp, anchorUp, ref _forceUp, KeyDuration);
        forceRight = Mathf.SmoothDamp(forceRight, anchorRight, ref _forceRight, KeyDuration);
    }
    public void GetJoyAxis45(ref float forceUp, ref float forceRight)
    {
        if (! zooming) forceUp = Input.GetAxis(Joystick.CameraAxisY); else forceUp = 0;
        forceRight = Input.GetAxis(Joystick.CameraAxisX);
    }
    public float GetZoom()
    {
        float res = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetButton(Joystick.PressToZoom))
        {
            zooming = true;
            res -= Input.GetAxis(Joystick.ZooomAxis) / joyZoomMuti;
        }
        else zooming = false;
        return res;
    }
    public bool GetRun()
    { return Input.GetKey(Keyboard.PressToRun) || Input.GetButton(Joystick.PressToRun); }
    public bool GetJumpDown()
    { return Input.GetKeyDown(Keyboard.TriggerToJump) || Input.GetButtonDown(Joystick.TriggerToJump); }
    public bool GetJumpUp()
    { return Input.GetKeyUp(Keyboard.TriggerToJump) || Input.GetButtonUp(Joystick.TriggerToJump); }
    public bool GetRollDown()
    { return Input.GetKeyDown(Keyboard.TriggerToRoll) || Input.GetButtonDown(Joystick.TriggerToRoll); }
    public bool GetDefense()
    { return Input.GetKey(Keyboard.PressToDefense) || Input.GetButton(Joystick.PressToDefense); }
    public bool GetLeftSlash()
    { return Input.GetMouseButton(0) || Input.GetButton(Joystick.LeftSlash); }
    public bool GetLeftAttack()
    { return Input.GetKey(Keyboard.LeftAttack) || Input.GetAxisRaw(Joystick.LeftAttack) < -0.5f; }
    public bool GetRightSlash()
    { return Input.GetMouseButton(1) || Input.GetButton(Joystick.RightSlash); }
    public bool GetRightAttack()
    { return Input.GetKey(Keyboard.RightAttack) || Input.GetAxisRaw(Joystick.RightAttack) > 0.5f; }
    public bool GetLeftSlashDown()
    { if (! leftSlash && GetLeftSlash()) return leftSlash = true; else if (leftSlash && ! GetLeftSlash()) return leftSlash = false; return false; }
    public bool GetLeftAttackDown()
    { if (! leftAttack && GetLeftAttack()) return leftAttack = true; else if (leftAttack && ! GetLeftAttack()) return leftAttack = false; return false; }
    public bool GetRightSlashDown()
    { if (! rightSlash && GetRightSlash()) return rightSlash = true; else if (rightSlash && ! GetRightSlash()) return rightSlash = false; return false; }
    public bool GetRightAttackDown()
    { if (! rightAttack && GetRightAttack()) return rightAttack = true; else if (rightAttack && ! GetRightAttack()) return rightAttack = false; return false; }
    public bool GetCrouchDown()
    { return Input.GetKeyDown(Keyboard.TriggerToCrouch) || Input.GetButtonDown(Joystick.TriggerToCrouch); }
    public bool GetLockOnDown() { return Input.GetKeyDown(Keyboard.TriggerToLockOn) || Input.GetButtonDown(Joystick.DoubleTriggerToLockOn); }
    public bool GetLockOnUp() { return Input.GetButtonUp(Joystick.DoubleTriggerToLockOn); }
    public bool GetSwitchLockOnTargetDown() { return Input.GetKeyDown(Keyboard.TriggerToSwithTarget); }
    public int GetSwithLockOnTargetTo()
    {
        if (Input.GetAxis(Joystick.TriggerToSwithTargetFrontAndBack) <= -0.5f) return 1; // front
        if (Input.GetAxis(Joystick.TriggerToSwithTargetFrontAndBack) >= 0.5f) return 2; // back
        if (Input.GetAxis(Joystick.TriggerToSwithTargetLeftAndRight) >= 0.5f) return 3; // right
        if (Input.GetAxis(Joystick.TriggerToSwithTargetLeftAndRight) <= -0.5f) return 4; // left
        return 0;
    }
    public int TriggerAxis()
    {
        if (Input.GetKeyDown(Keyboard.MoveForward)) return 1;
        if (Input.GetKeyDown(Keyboard.MoveBackword)) return 2;
        if (Input.GetKeyDown(Keyboard.MoveRightward)) return 3;
        if (Input.GetKeyDown(Keyboard.MoveLeftward)) return 4;
        return 0;
    }
    public int ReleaseAxis()
    {
        if (Input.GetKeyUp(Keyboard.MoveForward)) return 1;
        if (Input.GetKeyUp(Keyboard.MoveBackword)) return 2;
        if (Input.GetKeyUp(Keyboard.MoveRightward)) return 3;
        if (Input.GetKeyUp(Keyboard.MoveLeftward)) return 4;
        return 0;
    }
}
