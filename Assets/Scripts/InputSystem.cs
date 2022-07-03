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
    
    // 切换锁定
    private bool pressedDown = false;

    void Awake()
    {
        Keyboard = GetComponent<KeyboardMapping>();
        Joystick = GetComponent<JoystickMapping>();
    }
    void Update()
    {
        if (Input.GetKeyDown(Keyboard.SwitchGameMode) || Input.GetButtonDown(Joystick.SwitchGameMode))
            JoystickMode = !JoystickMode;
        // print(Input.GetButton(Joystick.TriggerToCloseGame));
        if (Input.GetKeyDown(Keyboard.TriggerToCloseGame) || Input.GetButtonDown(Joystick.TriggerToCloseGame))
            Application.Quit();
    }
    public float GetKeyAxisXY(
        ref float anchorFore, ref float anchorRight,
        ref float forceFore, ref float forceRight,
        ref float _forceFore, ref float _forceRight, float Duration)
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
        ref float _forceUp, ref float _forceRight, float KeyDuration)
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
    public bool GetRunPress()
    { return Input.GetKey(Keyboard.PressToRun) || Input.GetButton(Joystick.PressToRun); }
    public bool GetJumpDown()
    { return Input.GetKeyDown(Keyboard.TriggerToJump) || Input.GetButtonDown(Joystick.TriggerToJump); }
    public bool GetJumpUp()
    { return Input.GetKeyUp(Keyboard.TriggerToJump) || Input.GetButtonUp(Joystick.TriggerToJump); }
    public bool GetRollDown()
    { return Input.GetKeyDown(Keyboard.TriggerToRoll) || Input.GetButtonDown(Joystick.TriggerToRoll); }
    public bool GetSlashDown() 
    {
        return Input.GetMouseButtonDown(0)
        || Input.GetKeyDown(Keyboard.TriggerToSlash)
        || Input.GetButtonDown(Joystick.TriggerToSlash);
    }
    public bool GetDefensePress()
    { return Input.GetKey(Keyboard.PressToDefense) || Input.GetButton(Joystick.PressToDefense); }
    public bool GetCrouchDown()
    { return Input.GetKeyDown(Keyboard.TriggerToCrouch) || Input.GetButtonDown(Joystick.TriggerToCrouch); }
    public bool GetLockOnDown() { return Input.GetKeyDown(Keyboard.TriggerToLockOn); }
    public bool GetLockOnPress() { return Input.GetAxis(Joystick.PressToLockOn) >= 1-Config.EPS; }
    public bool GetSwitchLockOnTargetDown() { return Input.GetKeyDown(Keyboard.TriggerToSwithTarget); }
    public int GetSwithLockOnTargetBy()
    {
        if (Input.GetAxis(Joystick.TriggerToSwithTargetFrontAndBack) >= 1-Config.EPS) return 1;
        if (Input.GetAxis(Joystick.TriggerToSwithTargetFrontAndBack) <= -1+Config.EPS) return 2;
        if (Input.GetAxis(Joystick.TriggerToSwithTargetLeftAndRight) >= 1-Config.EPS) return 3;
        if (Input.GetAxis(Joystick.TriggerToSwithTargetLeftAndRight) <= -1+Config.EPS) return 4;
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
