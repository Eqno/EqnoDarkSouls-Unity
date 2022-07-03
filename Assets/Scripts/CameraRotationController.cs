using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    // 游戏对象
    [Header("Game Objects")]
    public GameObject Camera;
    public GameObject Character;
    public GameObject LockOnPoint;
    public GameObject CameraRotation;
    public GameObject TargetRotation;
    public GameObject CameraFollowRoot;
    // 平滑推动过程    
    [Header("Smooth Duration")]
    public float KeyDuration = 0.1f;
    public float CameraFollowLerp = 0.05f;
    public float CameraRotationDuration = 0.05f;
    public float CameraZoomDuration = 0.2f;
    public float MovingFollowLerp = 0.002f;
    public float LockOnFollowLerp = 0.01f;
    public float CameraHeightLerp = 0.05f;
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
    public float CameraRootHeight = 1.25f;
    public float CrouchRootHeight = 0.5f;
    // 跟踪方向参数
    [Header("Follow Direction")]
    public bool LockOn = false;
    public float LockOnTestLength = 1;
    public float LockOnTestWidth = 1;
    public float LockOnTestDistance = 8;
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
    // 锁定目标
    private Camera MainCamera;
    public GameObject LockTarget;
    private Vector3 LockOnTestBoxSize;
    private Collider[] lockOnColliders;
    private RectTransform LockOnPointRecTran;

    void Awake()
    {
        // 摄像机半径
        zoomAnchor = CameraRadius;
        // 锁定点
        LockOnPoint.SetActive(false);
        // 锁定鼠标
        Cursor.lockState = CursorLockMode.Locked;
        // 主摄像机
        MainCamera = Camera.GetComponent<Camera>();
        // 动画组件
        Animator = Character.GetComponent<Animator>();
        // 输入系统
        InputSystem = Character.GetComponent<InputSystem>();
        // 锁定点RecTran
        LockOnPointRecTran = LockOnPoint.GetComponent<RectTransform>();
        // 目标探测盒
        LockOnTestBoxSize = new Vector3(LockOnTestLength, LockOnTestWidth, LockOnTestDistance);
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
        if (! LockOn)
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
        }

        // 缩放
        zoomAnchor = Mathf.Clamp(
            zoomAnchor + InputSystem.GetZoom() * CameraZoomSensivity,
            MinCameraRadius, MaxCameraRadius
        );

        // 蹲下
        if (Animator.GetBool("OnCrouch"))
            CameraFollowRoot.transform.localPosition = Vector3.Lerp(CameraFollowRoot.transform.localPosition, new Vector3(0, CrouchRootHeight, 0), CameraHeightLerp);
        else CameraFollowRoot.transform.localPosition = Vector3.Lerp(CameraFollowRoot.transform.localPosition, new Vector3(0, CameraRootHeight, 0), CameraHeightLerp);

        // 平滑
        rotationX = Mathf.SmoothDamp(rotationX, rotAnchorX, ref _rotationX, CameraRotationDuration);
        rotationY = Mathf.SmoothDamp(rotationY, rotAnchorY, ref _rotationY, CameraRotationDuration);
        CameraRadius = Mathf.SmoothDamp(CameraRadius, zoomAnchor, ref _CameraRadius, CameraZoomDuration);

        // 应用
        Camera.transform.localPosition = new Vector3(-CameraOffsetX * CameraRadius, 0, CameraRadius);

        // 锁定
        bool newLockOn = LockOn;
        if (InputSystem.JoystickMode) newLockOn = InputSystem.GetLockOnPress();
        else if (InputSystem.GetLockOnDown()) newLockOn = !LockOn;
        if (LockOn != newLockOn)
        {
            LockOn = newLockOn;
            if (LockOn) FindLockOnTarget();
            else
            {
                LockTarget = null;
                LockOnPoint.SetActive(false);
            }
        }
        if (LockTarget != null)
        {
            Vector3 position = MainCamera.WorldToScreenPoint(LockTarget.transform.position);
            position -= new Vector3(Screen.width/2, Screen.height/2, 0);
            LockOnPointRecTran.anchoredPosition = position;
        }
        print(InputSystem.GetSwithLockOnTargetBy());

        // 摄像机方向跟踪
        if (InputSystem.JoystickMode)
        {
            if (Animator.GetFloat("Magnitude") > Config.EPS && movingTime <= MovingTimeBeforeFollow)
                movingTime += Time.deltaTime;
        }
        else movingTime = 0;
        float magnitude = Mathf.Sqrt(forceUp * forceUp + forceRight * forceRight);
        if (magnitude > Config.EPS) movingTime = 0;
        if (movingTime >= MovingTimeBeforeFollow || LockOn)
        {
            if (LockOn && LockTarget != null)
            {
                Vector3 forward = LockTarget.transform.position - transform.position;
                TargetRotation.transform.forward = forward;
                float tarAngleX = TargetRotation.transform.eulerAngles.x;
                if (360 - tarAngleX < 180) tarAngleX -= 360;
                FollowDirection(forward, tarAngleX, LockOnFollowLerp);
            }
            else FollowDirection(Character.transform.forward, 0, MovingFollowLerp);
        }
        else transform.eulerAngles = new Vector3(rotationY, rotationX, 0);
    }
    private void FindLockOnTarget()
    {
        Vector3 boxOrigin = transform.position + new Vector3(0, CameraRootHeight, 0);
        Vector3 boxCenter = boxOrigin + transform.forward * LockOnTestDistance;
        lockOnColliders = Physics.OverlapBox(boxCenter, LockOnTestBoxSize, transform.rotation, LayerMask.GetMask("Enemy"));
        if (lockOnColliders.Length > 0)
        {
            float minDis = (lockOnColliders[0].transform.position-Character.transform.position).magnitude;
            LockTarget = lockOnColliders[0].gameObject;
            foreach (var i in lockOnColliders)
            {
                float thisDis = (i.transform.position-Character.transform.position).magnitude;
                if (thisDis < minDis) { minDis = thisDis; LockTarget = i.gameObject; }
            }
            LockOnPoint.SetActive(true);
        }
        else LockOn = false;
    }
    private void FollowDirection(Vector3 target2D, float eulerX, float lerp) // forward
    {
        Vector3 direct = new Vector3(transform.forward.x, 0, transform.forward.z);
        Vector3 target = new Vector3(target2D.x, 0, target2D.z);
        direct = Vector3.Slerp(direct, target, lerp);

        CameraRotation.transform.forward = direct;
        rotAnchorX = CameraRotation.transform.eulerAngles.y;
        rotAnchorY = Mathf.Lerp(rotAnchorY, eulerX, lerp);
        CameraRotation.transform.eulerAngles = new Vector3(rotAnchorY, rotAnchorX, 0);

        transform.forward = CameraRotation.transform.forward;
    }
}