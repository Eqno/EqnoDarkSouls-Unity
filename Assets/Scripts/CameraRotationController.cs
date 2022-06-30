using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    // 游戏对象
    [Header("Game Objects")]
    public GameObject Camera;
    public GameObject Rotation;
    public GameObject Character;
    public GameObject CameraFollowRoot;
    // 平滑推动过程    
    [Header("Smooth Duration")]
    public float KeyDuration = 0.1f;
    public float CameraFollowLerp = 0.05f;
    public float CameraRotationDuration = 0.05f;
    public float CameraZoomDuration = 0.2f;
    public float MovingFollowLerp = 0.01f;
    // 摄像机缩放参数
    [Header("Camera Zoom")]
    public float CameraRadius = -2;
    public float CameraZoomSensivity = 4;
    public float MaxCameraRadius = -0.7f;
    public float MinCameraRadius = -4;
    // 相机旋转参数
    [Header("Camera Rotation")]
    public float KeyXSensitivity = 128;
    public float KeyYSensitivity = 80;
    public float MouseXSensitivity = 2;
    public float MouseYSensitivity = 2;
    public float MaxVerticalDegree = 70;
    public float MinVerticalDegree = -70;
    // 摄像机偏移参数
    [Header("Camera Offset")]
    public float CameraOffsetX = 0.28f;
    // 跟踪方向参数
    [Header("Follow Direction")]
    public float MovingTimeBeforeFollow = 3;

    // GameObjects
    private Animator Animator;
    private InputSystem InputSystem;

    // 摇杆锚点位置
    private float anchorUp, anchorRight;
    // 摇杆推动力度
    private float forceUp, forceRight;
    // Smooth过程变量
    private float _forceUp, _forceRight;

    // 相机缩放
    private float zoomAnchor, _CameraRadius;

    // 相机旋转
    private float rotationX = 0, rotationY = 0;
    private float rotAnchorX, rotAnchorY;
    private float _rotationX, _rotationY;
    private float movingTime = 0;

    void Start()
    {
        // 摄像机半径
        zoomAnchor = CameraRadius;
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        // 动画组件
        Animator = Character.GetComponent<Animator>();
        // 输入系统
        InputSystem = Character.GetComponent<InputSystem>();
    }
    void Update()
    {
        // 监听器
        if (! InputSystem.JoystickMode)
        {
            InputSystem.GetKeyAxis45(
                ref anchorUp, ref anchorRight,
                ref forceUp, ref forceRight,
                ref _forceUp, ref _forceRight, KeyDuration
            );
        }
        else InputSystem.GetJoyAxis45(ref forceUp, ref forceRight);

        //控制器
        CameraRotateController();
    }
    // 摄像机跟踪
    void LateUpdate()
    { transform.position = Vector3.Lerp(transform.position, CameraFollowRoot.transform.position, CameraFollowLerp); }
    private void CameraRotateController()
    {
        // 键盘
        rotAnchorY = Mathf.Clamp(
            rotAnchorY + forceUp * KeyYSensitivity * Time.deltaTime,
            MinVerticalDegree, MaxVerticalDegree
        );
        rotAnchorX += forceRight * KeyXSensitivity * Time.deltaTime;

        // 鼠标
        rotAnchorY = Mathf.Clamp(
            rotAnchorY + Input.GetAxis("Mouse Y") * MouseYSensitivity,
            MinVerticalDegree, MaxVerticalDegree
        );
        rotAnchorX += Input.GetAxis("Mouse X") * MouseXSensitivity;

        // 缩放
        zoomAnchor = Mathf.Clamp(
            zoomAnchor + InputSystem.GetZoom() * CameraZoomSensivity,
            MinCameraRadius, MaxCameraRadius
        );

        // 平滑
        rotationX = Mathf.SmoothDamp(rotationX, rotAnchorX, ref _rotationX, CameraRotationDuration);
        rotationY = Mathf.SmoothDamp(rotationY, rotAnchorY, ref _rotationY, CameraRotationDuration);
        CameraRadius = Mathf.SmoothDamp(CameraRadius, zoomAnchor, ref _CameraRadius, CameraZoomDuration);

        // 应用
        Camera.transform.localPosition = new Vector3(-CameraOffsetX * CameraRadius, 0, CameraRadius);

        // 摄像机方向跟踪
        if (InputSystem.JoystickMode)
        {
            if (Animator.GetFloat("Forward") > Config.EPS && movingTime <= MovingTimeBeforeFollow)
                movingTime += Time.deltaTime;
        }
        else movingTime = 0;
        float magnitude = Mathf.Sqrt(forceUp * forceUp + forceRight * forceRight);
        if (magnitude > Config.EPS) movingTime = 0;
        if (movingTime >= MovingTimeBeforeFollow)
        {
            Vector3 direction = new Vector3(transform.forward.x, 0, transform.forward.z);
            Vector3 target = new Vector3(Character.transform.forward.x, 0, Character.transform.forward.z);
            direction = Vector3.Slerp(direction, target, MovingFollowLerp);

            Rotation.transform.forward = direction;
            rotAnchorX = Rotation.transform.eulerAngles.y;
            rotAnchorY = Mathf.Lerp(rotAnchorY, Character.transform.eulerAngles.x, MovingFollowLerp);
            Rotation.transform.eulerAngles = new Vector3(rotAnchorY, rotAnchorX, 0);

            transform.forward = Rotation.transform.forward;
        }
        else transform.eulerAngles = new Vector3(rotationY, rotationX, 0);
    }
}