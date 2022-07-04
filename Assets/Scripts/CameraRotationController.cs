using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CameraRotationController : MonoBehaviour
{
    // 游戏对象
    [Header("Game Objects")]
    public GameObject Camera;
    public GameObject Character;
    public GameObject LockOnPoint;
    public GameObject CameraFollowRoot;
    // 平滑推动过程    
    [Header("Smooth Duration")]
    public float KeyDuration = 0.1f;
    public float CameraFollowLerp = 0.05f;
    public float MovingFollowLerp = 0.002f;
    public float LockOnFollowLerp = 0.01f;
    public float CameraHeightLerp = 0.05f;
    public float CameraZoomDuration = 0.2f;
    public float CameraRotationDuration = 0.05f;
    // 摄像机缩放参数
    [Header("Camera Zoom")]
    public float CameraRadius = -2;
    public float MaxCameraRadius = -0.7f;
    public float MinCameraRadius = -4;
    public float CameraZoomSensivity = 4;
    // 相机旋转参数
    [Header("Camera Rotation")]
    public bool FlipVertical = true;
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
    public GameObject LockTarget;
    public float LockOnTestWidth = 10;
    public float LockOnTestLength = 10;
    public float LockOnTestDistance = 10;
    public float DoubleTriggerCritical = 0.4f;
    public float MovingTimeBeforeFollow = 1.5f;
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
    private float movingTime = 0;
    public float rotationX, rotationY;
    private float rotAnchorX, rotAnchorY;
    private float _rotationX, _rotationY;
    // 锁定目标
    private Camera MainCamera;
    private float lockCount = 0;
    private int lastSwitchSignal = 0;
    private Vector3 LockOnTestBoxSize;
    private Collider[] lockOnColliders;
    private RectTransform LockOnPointRecTran;
    private Dictionary<int, int> lockedOnTimes;
    private bool triggerLock = false, readyToLock = false;

    void Awake()
    {
        zoomAnchor = CameraRadius;
        LockOnPoint.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        lockedOnTimes = new Dictionary<int, int>();
        MainCamera = Camera.GetComponent<Camera>();
        Animator = Character.GetComponent<Animator>();
        InputSystem = Character.GetComponent<InputSystem>();
        LockOnPointRecTran = LockOnPoint.GetComponent<RectTransform>();
        LockOnTestBoxSize = new Vector3(LockOnTestLength, LockOnTestWidth, LockOnTestDistance);
    }
    void Update()
    {
        // 监听器
        if (! InputSystem.JoystickMode) InputSystem.GetKeyAxis45(ref anchorUp, ref anchorRight, ref forceUp, ref forceRight, ref _forceUp, ref _forceRight, KeyDuration);
        else InputSystem.GetJoyAxis45(ref forceUp, ref forceRight);

        //控制器
        CameraRotateController();
    }
    // 摄像机跟踪
    void LateUpdate() { transform.position = Vector3.Lerp(transform.position, CameraFollowRoot.transform.position, CameraFollowLerp); }
    private void CameraRotateController()
    {
        // 键盘
        rotAnchorY = Mathf.Clamp(rotAnchorY + forceUp * KeyYSensitivity * Time.deltaTime, MinVerticalDegree, MaxVerticalDegree);
        rotAnchorX += forceRight * KeyXSensitivity * Time.deltaTime;
        
        // 鼠标
        rotAnchorY = Mathf.Clamp(rotAnchorY + Input.GetAxis("Mouse Y") * MouseYSensitivity * (FlipVertical ? -1 : 1), MinVerticalDegree, MaxVerticalDegree);
        rotAnchorX += Input.GetAxis("Mouse X") * MouseXSensitivity;

        // 缩放
        zoomAnchor = Mathf.Clamp(zoomAnchor + InputSystem.GetZoom() * CameraZoomSensivity, MinCameraRadius, MaxCameraRadius);

        // 平滑
        rotationX = Mathf.SmoothDamp(rotationX, rotAnchorX, ref _rotationX, CameraRotationDuration);
        rotationY = Mathf.SmoothDamp(rotationY, rotAnchorY, ref _rotationY, CameraRotationDuration);
        CameraRadius = Mathf.SmoothDamp(CameraRadius, zoomAnchor, ref _CameraRadius, CameraZoomDuration);

        // 蹲下
        float rootHeight = Animator.GetBool("OnCrouch") ? CrouchRootHeight : CameraRootHeight;
        CameraFollowRoot.transform.localPosition = Vector3.Lerp(CameraFollowRoot.transform.localPosition, new Vector3(0, rootHeight, 0), CameraHeightLerp);

        // 锁定
        bool newLockOn = LockOn;
        if (InputSystem.JoystickMode)
        {
            if (InputSystem.GetLockOnDown()) triggerLock = true;
            if (triggerLock)
            {
                if (InputSystem.GetLockOnUp()) readyToLock = true;
                if (readyToLock && InputSystem.GetLockOnDown()) newLockOn = !LockOn;
                lockCount += Time.deltaTime;
                if (lockCount > DoubleTriggerCritical)
                {
                    lockCount = 0;
                    triggerLock = readyToLock = false;
                }
            }
        }
        else if (InputSystem.GetLockOnDown()) newLockOn = !LockOn;
        if (LockOn != newLockOn)
        {
            LockOn = newLockOn;
            if (LockOn) NearestLockOnTarget();
            else
            {
                LockTarget = null;
                lockedOnTimes.Clear();
                LockOnPoint.SetActive(false);
            }
        }
        if (LockTarget != null)
        {
            SwitchLockOnTarget();
            FollowDirection(LockTarget.transform.position - transform.position, LockOnFollowLerp);
            if (Vector3.Distance(LockTarget.transform.position, Character.transform.position) > 2*LockOnTestDistance) NearestLockOnTarget();
            LockOnPointRecTran.anchoredPosition = MainCamera.WorldToScreenPoint(LockTarget.transform.position) - new Vector3(Screen.width/2, Screen.height/2, 0);
        }

        // 摄像机方向跟踪
        if (InputSystem.JoystickMode && Animator.GetFloat("Magnitude") > Config.EPS && movingTime <= MovingTimeBeforeFollow) movingTime += Time.deltaTime;
        if (Mathf.Sqrt(forceUp * forceUp + forceRight * forceRight) > Config.EPS || LockTarget != null) movingTime = 0;
        if (movingTime >= MovingTimeBeforeFollow) FollowDirection(Character.transform.forward, MovingFollowLerp);

        // 应用
        transform.eulerAngles = new Vector3(rotationY, rotationX, 0);
        Camera.transform.localPosition = new Vector3(-CameraOffsetX * CameraRadius, 0, CameraRadius);
    }
    private void FindLockOnTarget()
    {
        Vector3 boxOrigin = transform.position + new Vector3(0, CameraRootHeight, 0);
        Vector3 boxCenter = boxOrigin + transform.forward * LockOnTestDistance;
        lockOnColliders = Physics.OverlapBox(boxCenter, LockOnTestBoxSize, transform.rotation, LayerMask.GetMask("Enemy"));
    }
    private void NearestLockOnTarget()
    {
        FindLockOnTarget();
        LockTarget = null;
        if (lockOnColliders.Length > 0)
        {
            System.Array.Sort(lockOnColliders, (a, b) => Vector3.Distance(a.transform.position, Character.transform.position).CompareTo(Vector3.Distance(b.transform.position, Character.transform.position)));
            LockTarget = lockOnColliders[0].gameObject;

            if (! lockedOnTimes.ContainsKey(LockTarget.GetInstanceID())) lockedOnTimes.Add(LockTarget.GetInstanceID(), 0);
            lockedOnTimes[LockTarget.GetInstanceID()] ++;
            LockOnPoint.SetActive(true);
        }
    }
    private void FollowDirection(Vector3 target, float lerp) // forward
    {
        Vector3 dir2D = Vector3.Slerp(new Vector3(transform.forward.x, 0, transform.forward.z), new Vector3(target.x, 0, target.z), lerp);
        transform.forward = new Vector3(dir2D.x, Mathf.Lerp(transform.forward.y, target.y, lerp), dir2D.z);

        rotAnchorX = rotationX = transform.eulerAngles.y;
        rotAnchorY = rotationY = 360 - transform.eulerAngles.x < 180 ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
    }
    private void SwitchLockOnTarget()
    {
        if (InputSystem.JoystickMode)
        {
            int swithSignal = InputSystem.GetSwithLockOnTargetTo();
            if (swithSignal != lastSwitchSignal)
            {
                lastSwitchSignal = swithSignal;
                if (swithSignal == 0) return;
                
                FindLockOnTarget();
                switch (swithSignal)
                {
                    case 1: SwitchTargetToLeftOrRight(0, 1); break; // front
                    case 2: SwitchTargetToLeftOrRight(0, -1); break; // back
                    case 3: SwitchTargetToLeftOrRight(1, 1); break; // right
                    case 4: SwitchTargetToLeftOrRight(1, -1); break; // left
                    default: break;
                }
            }
        }
        else
        {
            if (InputSystem.GetSwitchLockOnTargetDown())
            {
                FindLockOnTarget();
                if (lockOnColliders.Length > 0)
                {
                    System.Array.Sort(lockOnColliders, (a, b) => {
                        int aid = a.gameObject.GetInstanceID();
                        int bid = b.gameObject.GetInstanceID();
                        if (! lockedOnTimes.ContainsKey(aid)) lockedOnTimes.Add(aid, 0);
                        if (! lockedOnTimes.ContainsKey(bid)) lockedOnTimes.Add(bid, 0);
                        if (lockedOnTimes[aid] != lockedOnTimes[bid]) return lockedOnTimes[aid].CompareTo(lockedOnTimes[bid]);
                        return Vector3.Distance(a.transform.position, Character.transform.position).CompareTo(Vector3.Distance(b.transform.position, Character.transform.position));
                    });
                    LockTarget = lockOnColliders[0].gameObject;
                    lockedOnTimes[LockTarget.GetInstanceID()] ++;
                    LockOnPoint.SetActive(true);
                }
            }
        }
    }
    private void SwitchTargetToLeftOrRight(int dir1, int dir2) // dir1: 0->front or back, 1->left or right; dir2: 1->front, -1->back or -1->left, 1->right
    {
        float minDis = 0;
        bool flag = false;
        GameObject newTarget = null;
        Vector3 normal = LockTarget.transform.position - transform.position;
        foreach (var i in lockOnColliders)
        {
            float mul = 0, thisDis = 0;
            Vector3 direction = Vector3.zero;
            if (dir1 == 0)
            {
                direction = i.transform.position - LockTarget.transform.position;
                mul = Vector3.Dot(normal, direction);
            }
            else
            {
                direction = i.transform.position - transform.position;
                mul = Vector3.Cross(normal, direction).y;
            }
            if (i.gameObject.GetInstanceID() != LockTarget.GetInstanceID() && dir2 * mul >= 0)
            {
                if (flag)
                {
                    thisDis = Vector3.Distance(i.transform.position, LockTarget.transform.position);
                    if (thisDis < minDis) { minDis = thisDis; newTarget = i.gameObject; }
                }
                else
                {
                    flag = true;
                    newTarget = i.gameObject;
                    thisDis = minDis = Vector3.Distance(i.transform.position, LockTarget.transform.position);
                }
            }
        }
        if (newTarget != null) LockTarget = newTarget;
    }
}