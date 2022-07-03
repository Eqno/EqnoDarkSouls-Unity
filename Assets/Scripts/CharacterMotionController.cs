using UnityEngine;

public class CharacterMotionController : MonoBehaviour
{
    // 游戏对象
    [Header("Game Objects")]
    public GameObject CameraHandler;
    // 平滑推动过程    
    [Header("Smooth Duration")]
    public float Duration = 0.1f;
    public float TurnLerp = 0.05f;
    public float LayerLerp = 0.02f;
    public float CrouchLaryerLerp = 0.05f;
    public float ImpactLayerLerp = 0.1f;
    // 走路跑步参数
    [Header("Walk Run And Jump")]
    public float WalkSpeed = 1.4f;
    public float RunSpeed = 3.8f;
    public float JumpSpeed = 5;
    public float Gravity = 15;
    // 翻滚参数
    [Header("Roll And Jab")]
    public float RollSpeed = 3;
    public float JabSpeed = 6;
    public float FallToRollCritical = -10;
    public float DoubleTriggerCritical = 0.25f;
    // 切换Idle
    [Header("Switch Idle")]
    public float EachIdleTime = 10;
    public int IdleNum = 3;
    // 攻击
    [Header("Slash And Attack")]
    public float ListenNextSlashTime = 0.5f;

    // GameObjects
    private Animator Animator;
    private InputSystem InputSystem;
    private CharacterController Controller;
    private CameraRotationController cameraRotationController;

    // 摇杆锚点位置
    private float anchorFore, anchorRight;
    // 摇杆推动力度
    public float forceFore, forceRight;
    // Smooth过程变量
    private float _forceFore, _forceRight;

    // 动作幅度和移动速度
    private float unitMagnitude;
    private float anchRangeMagnitude, anchorRangeFore, anchorRangeRight, anchSpeed;
    private float motionRangeMagnitude, motionRangeFore, motionRangeRight, motionSpeed;
    private float _motionRangeMagnitude, _motionRangeFore, _motionRangeRight, _motionSpeed;

    // 下落
    private float velocity = 0;
    private Vector3 moveDirection;
    private float inAirTime = 0, inAirTotal = 0.3f;

    // 双击翻滚
    private float rollCount = 0, jabCount = 0;
    private bool rolling = false, jabbing = false;
    private int triggerButton = 0, readyButton = 0;
    private int triggerAxis = 0, triggerJab = 0;
    private int readyToRoll = 0, readyToJab = 0;

    // 攻击
    private bool slashing = false;
    private float attackLayerWeight = 0;
    private float movingAttackWeight = 0;
    private float slashTime = 0;
    
    // 切换Idle
    private int idleCount = 0;
    private float idleTime = 0;
    private float idleLayerWeight = 0;

    // 蹲下
    private bool crouching = false;
    private bool crouchDuration = false;
    private float crouchLayerWeight = 0;

    // 受击
    private bool impacting = false;
    private float impactLayerWeight = 0, movingImpactWeight = 0;

    void Awake()
    {
        moveDirection = Vector3.zero;
        Animator = GetComponent<Animator>();
        InputSystem = GetComponent<InputSystem>();
        Controller = GetComponent<CharacterController>();
        cameraRotationController = CameraHandler.GetComponent<CameraRotationController>();
    }
    void Update()
    {
        // 监听器
        if (! InputSystem.JoystickMode)
        {
            unitMagnitude = InputSystem.GetKeyAxisXY(
                ref anchorFore, ref anchorRight,
                ref forceFore, ref forceRight,
                ref _forceFore, ref _forceRight, Duration
            );
        }
        else unitMagnitude = InputSystem.GetJoyAxisXY(ref forceFore, ref forceRight);

        // 控制器
        CharacterMoveController();
        CharacterAttackController();
        CharacterImpactController();
    }

    private void CharacterMoveController()
    {
        // 动作幅度
        anchorRangeFore = forceFore * (InputSystem.GetRunPress() ? 2 : 1);
        anchorRangeRight = forceRight * (InputSystem.GetRunPress() ? 2 : 1);
        anchRangeMagnitude = unitMagnitude * (InputSystem.GetRunPress() ? 2 : 1);
        if (! cameraRotationController.LockOn)
        {
            anchorRangeFore = anchRangeMagnitude;
            anchorRangeRight = 0;
        }
        motionRangeFore = Mathf.SmoothDamp(motionRangeFore, anchorRangeFore, ref _motionRangeFore, Duration);
        motionRangeRight = Mathf.SmoothDamp(motionRangeRight, anchorRangeRight, ref _motionRangeRight, Duration);
        motionRangeMagnitude = Mathf.SmoothDamp(motionRangeMagnitude, anchRangeMagnitude, ref _motionRangeMagnitude, Duration);
        Animator.SetFloat("Forward", motionRangeFore);
        Animator.SetFloat("Rightward", motionRangeRight);
        Animator.SetFloat("Magnitude", motionRangeMagnitude);

        // 转向
        if (motionRangeMagnitude > Config.EPS)
        {
            Vector3 anchor = Vector3.zero;
            if (cameraRotationController.LockOn)
            {
                Vector3 dir = cameraRotationController.LockTarget.transform.position - transform.position;
                anchor = new Vector3(dir.x, 0, dir.z);
            }
            else
            {
                anchor = new Vector3(
                    forceRight * CameraHandler.transform.forward.z + forceFore * CameraHandler.transform.forward.x, 0,
                    forceFore * CameraHandler.transform.forward.z - forceRight * CameraHandler.transform.forward.x
                );
            }
            transform.forward = Vector3.Slerp(transform.forward, anchor, TurnLerp);
        }

        // 移动
        Vector3 direction = Vector3.zero;
        anchSpeed = unitMagnitude * (InputSystem.GetRunPress() ? RunSpeed : WalkSpeed);
        motionSpeed = Mathf.SmoothDamp(motionSpeed, anchSpeed, ref _motionSpeed, Duration);
        if (cameraRotationController.LockOn)
        {
            direction = new Vector3(
                forceRight * transform.forward.z + forceFore * transform.forward.x, 0,
                forceFore * transform.forward.z - forceRight * transform.forward.x
            ) * motionSpeed;
        }
        else direction = transform.forward * motionSpeed;

        // 跳跃
        if (Controller.isGrounded)
        {
            velocity = inAirTime = 0;
            if (InputSystem.GetJumpDown() && !crouching)
            {
                Animator.SetTrigger("Jump");
                velocity = JumpSpeed;
            }
        }
        else inAirTime += Time.deltaTime;
        velocity -= Gravity * Time.deltaTime;
        Animator.SetBool("OnGround", inAirTime <= inAirTotal);

        // 翻滚
        if (velocity < FallToRollCritical) Animator.SetTrigger("Roll");
        if ((triggerButton = InputSystem.TriggerAxis()) != 0)
            triggerAxis = triggerButton;
        if (triggerAxis != 0)
        {
            if ((readyButton = InputSystem.ReleaseAxis()) != 0)
                readyToRoll = readyButton;
            if (readyToRoll != 0)
            {
                if (readyToRoll == triggerAxis)
                {
                    if (readyToRoll == InputSystem.TriggerAxis() && !slashing)
                        Animator.SetTrigger("Roll");
                }
                else rollCount = triggerAxis = readyToRoll = 0;
            }
            rollCount += Time.deltaTime;
            if (rollCount > DoubleTriggerCritical) 
                rollCount = triggerAxis = readyToRoll = 0;
        }
        if (InputSystem.GetRollDown() && !slashing) Animator.SetTrigger("Roll");
        if (rolling)
        {
            if (cameraRotationController.LockOn && Animator.GetFloat("Magnitude") > Config.EPS)
            {
                direction = new Vector3(
                    forceRight * transform.forward.z + forceFore * transform.forward.x, 0,
                    forceFore * transform.forward.z - forceRight * transform.forward.x
                );
                direction = direction / direction.magnitude * RollSpeed;
            }
            else direction = transform.forward * RollSpeed;
        }

        // 后跳
        if (InputSystem.GetJumpDown()) triggerJab = 1;
        if (triggerJab != 0)
        {
            if (InputSystem.GetJumpUp()) readyToJab = 1;
            if (readyToJab != 0)
            {
                if (InputSystem.GetJumpDown() && !crouching)
                    Animator.SetTrigger("Jab");
            }
            jabCount += Time.deltaTime;
            if (jabCount > DoubleTriggerCritical)
                jabCount = triggerJab = readyToJab = 0;
        }
        if (jabbing) direction = transform.forward * Animator.GetFloat("JabVelocity") * JabSpeed;

        // 重力
        direction.y = velocity;

        // 缓冲
        if (Animator.GetFloat("Magnitude") > Config.EPS) Controller.Move(direction * Time.deltaTime);
        else Controller.Move(moveDirection + direction * Time.deltaTime);
        moveDirection = Vector3.zero;

        // 切换Idle
        if (! CheckState("Ground") || ! CheckState("Idle", "Attack Layer") || ! CheckState("ShieldDown", "Defense Layer") || ! CheckState("Idle", "Crouch Layer") || Animator.GetFloat("Magnitude") > Config.EPS)
        {
            idleTime = 0;
            Animator.SetInteger("Idle", 0);
            AdjustLayerWeight(ref idleLayerWeight, "Idle Layer", 0, LayerLerp);
        }
        else idleTime += Time.deltaTime;
        if (idleTime >= EachIdleTime)
        {
            idleTime = 0;
            idleCount = (idleCount + 1) % IdleNum;
            if (idleCount != 0) Animator.SetTrigger("NextIdle");
            Animator.SetInteger("Idle", idleCount);
        }
        if (Animator.GetInteger("Idle") != 0)
            AdjustLayerWeight(ref idleLayerWeight, "Idle Layer", 1, LayerLerp);
        else AdjustLayerWeight(ref idleLayerWeight, "Idle Layer", 0, LayerLerp);

        // 蹲下
        if (InputSystem.GetCrouchDown())
        {
            crouching = !crouching;
            Animator.SetBool("Crouch", crouching);
        }
        if (crouching)
        {
            if (Animator.GetFloat("Magnitude") > Config.EPS || rolling)
            {
                if (rolling) Animator.SetBool("OnCrouch", true);
                else Animator.SetBool("OnCrouch", false);
                AdjustLayerWeight(ref crouchLayerWeight, "Crouch Layer", 0, CrouchLaryerLerp);
            }
            else
            {
                Animator.SetBool("OnCrouch", true);
                AdjustLayerWeight(ref crouchLayerWeight, "Crouch Layer", 1, CrouchLaryerLerp);
            }
        }
        else
        {
            Animator.SetBool("OnCrouch", false);
            AdjustLayerWeight(ref crouchLayerWeight, "Crouch Layer", 0, CrouchLaryerLerp);
        }
    }
    private void CharacterAttackController()
    {
        // 攻击
        if (InputSystem.GetSlashDown())
        {
            slashTime = 0;
            Animator.SetBool("Slash", true);
        }
        if (slashTime > ListenNextSlashTime)
        {
            Animator.SetBool("Slash", false);
        }
        else slashTime += Time.deltaTime;
        if (slashing)
        {
            if (Animator.GetFloat("Magnitude") > Config.EPS)
                AdjustLayerWeight(ref attackLayerWeight, "Attack Layer", 0, LayerLerp);
            else AdjustLayerWeight(ref attackLayerWeight, "Attack Layer", 1, LayerLerp);
            AdjustLayerWeight(ref movingAttackWeight, "Moving Attack", 1, LayerLerp);
        }
        else
        {
            slashTime = ListenNextSlashTime + 1;
            AdjustLayerWeight(ref attackLayerWeight, "Attack Layer", 0, LayerLerp);
            AdjustLayerWeight(ref movingAttackWeight, "Moving Attack", 0, LayerLerp);
        }

        // 防御
        Animator.SetBool("Defense", InputSystem.GetDefensePress());
    }
    private void CharacterImpactController()
    {
        // if (Input.GetKeyDown("1")) Animator.SetTrigger("ImpactDefense");
        // if (Input.GetKeyDown("2")) Animator.SetTrigger("ImpactFront");
        // if (Input.GetKeyDown("3")) Animator.SetTrigger("ImpactBack");
        if (impacting)
        {
            if (Animator.GetFloat("Magnitude") > Config.EPS)
                AdjustLayerWeight(ref impactLayerWeight, "Impact Layer", 0, ImpactLayerLerp);
            else AdjustLayerWeight(ref impactLayerWeight, "Impact Layer", 1, ImpactLayerLerp);
            AdjustLayerWeight(ref movingImpactWeight, "Moving Impact", 1, ImpactLayerLerp);
        }
        else
        {
            AdjustLayerWeight(ref impactLayerWeight, "Impact Layer", 0, ImpactLayerLerp);
            AdjustLayerWeight(ref movingImpactWeight, "Moving Impact", 0, ImpactLayerLerp);
        }
    }
    private void AdjustLayerWeight(ref float weight, string layerName, float target, float lerp)
    {
        weight = Mathf.Lerp(weight, target, lerp);
        Animator.SetLayerWeight(Animator.GetLayerIndex(layerName), weight);
    }
    private bool CheckState(string stateName, string layerName = "Base Layer")
    {
        int layerIndex = Animator.GetLayerIndex(layerName);
        return Animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName);
    }
    // 更新频率不同，必须缓冲
    void OnAnimatorMove() { if (slashing || crouchDuration || impacting) moveDirection += Animator.deltaPosition; }
    public void OnRollEnter() { rolling = true; }
    public void OnRollExit() { rolling = false; }
    public void OnJabEnter() { jabbing = true; }
    public void OnJabExit() { jabbing = false; }
    public void OnSlashEnter() { slashing = true; }
    public void OnSlashExit() { slashing = false; }
    public void OnCrouchDurationEnter() { crouchDuration = true; }
    public void OnCrouchDurationExit() { crouchDuration = false; }
    public void OnImpactEnter() { impacting = true; }
    public void OnImpactExit() { impacting = false; }
}