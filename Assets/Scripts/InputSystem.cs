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

    void Start()
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
        
        // 平滑推动
        forceFore = Mathf.SmoothDamp(forceFore, anchorFore, ref _forceFore, Duration);
        forceRight = Mathf.SmoothDamp(forceRight, anchorRight, ref _forceRight, Duration);

        // 方转圆
        float unitFore = forceFore * Mathf.Sqrt(1 - forceRight * forceRight / 2);
        float unitRight = forceRight * Mathf.Sqrt(1 - forceFore * forceFore / 2);
        return Mathf.Sqrt(unitFore * unitFore + unitRight * unitRight);
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
