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
    public float LayerLerp = 0.1f;
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

    // 摇杆锚点位置
    private float anchorFore, anchorRight;
    // 摇杆推动力度
    private float forceFore, forceRight;
    // Smooth过程变量
    private float _forceFore, _forceRight;

    // 动作幅度和移动速度
    private float unitMagnitude;
    private float anchRange, anchSpeed;
    private float motionRange, motionSpeed;
    private float _motionRange, _motionSpeed;

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

    void Start()
    {
        moveDirection = Vector3.zero;
        Animator = GetComponent<Animator>();
        InputSystem = GetComponent<InputSystem>();
        Controller = GetComponent<CharacterController>();
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
    }
    private void CharacterMoveController()
    {
        // 动作幅度
        anchRange = unitMagnitude * (InputSystem.GetRunPress() ? 2 : 1);
        motionRange = Mathf.SmoothDamp(motionRange, anchRange, ref _motionRange, Duration);
        Animator.SetFloat("Forward", motionRange);

        // 转向
        if (motionRange > Config.EPS)
        {
            Vector3 anchor = new Vector3(
                forceRight * CameraHandler.transform.forward.z + forceFore * CameraHandler.transform.forward.x, 0,
                forceFore * CameraHandler.transform.forward.z - forceRight * CameraHandler.transform.forward.x
            );
            transform.forward = Vector3.Slerp(transform.forward, anchor, TurnLerp);
        }

        // 移动
        anchSpeed = unitMagnitude * (InputSystem.GetRunPress() ? RunSpeed : WalkSpeed);
        motionSpeed = Mathf.SmoothDamp(motionSpeed, anchSpeed, ref _motionSpeed, Duration);
        Vector3 direction = transform.forward * motionSpeed;

        // 跳跃
        if (Controller.isGrounded)
        {
            velocity = inAirTime = 0;
            if (InputSystem.GetJumpDown())
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
                    if (readyToRoll == InputSystem.TriggerAxis())
                        Animator.SetTrigger("Roll");
                }
                else rollCount = triggerAxis = readyToRoll = 0;
            }
            rollCount += Time.deltaTime;
            if (rollCount > DoubleTriggerCritical) 
                rollCount = triggerAxis = readyToRoll = 0;
        }
        if (InputSystem.GetRollDown()) Animator.SetTrigger("Roll");

        // 后跳
        if (InputSystem.GetJumpDown()) triggerJab = 1;
        if (triggerJab != 0)
        {
            if (InputSystem.GetJumpUp()) readyToJab = 1;
            if (readyToJab != 0)
            {
                if (InputSystem.GetJumpDown())
                    Animator.SetTrigger("Jab");
            }
            jabCount += Time.deltaTime;
            if (jabCount > DoubleTriggerCritical)
                jabCount = triggerJab = readyToJab = 0;
        }
        if (rolling) direction = transform.forward * RollSpeed;
        if (jabbing) direction = transform.forward * Animator.GetFloat("JabVelocity") * JabSpeed;

        // 重力
        direction.y = velocity;

        // 缓冲
        if (Animator.GetFloat("Forward") > Config.EPS) Controller.Move(direction * Time.deltaTime);
        else Controller.Move(moveDirection + direction * Time.deltaTime);
        moveDirection = Vector3.zero;

        // 切换Idle
        if (! CheckState("Ground") || ! CheckState("Idle", "Attack Layer") || Animator.GetFloat("Forward") > Config.EPS)
        {
            idleTime = 0;
            Animator.SetInteger("Idle", 0);
            AdjustLayerWeight(ref idleLayerWeight, "Idle Layer", 0);
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
            AdjustLayerWeight(ref idleLayerWeight, "Idle Layer", 1);
        else AdjustLayerWeight(ref idleLayerWeight, "Idle Layer", 0);
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
            if (Animator.GetFloat("Forward") > Config.EPS)
                AdjustLayerWeight(ref attackLayerWeight, "Attack Layer", 0);
            else AdjustLayerWeight(ref attackLayerWeight, "Attack Layer", 1);
            AdjustLayerWeight(ref movingAttackWeight, "Moving Attack", 1);
        }
        else
        {
            slashTime = ListenNextSlashTime + 1;
            AdjustLayerWeight(ref attackLayerWeight, "Attack Layer", 0);
            AdjustLayerWeight(ref movingAttackWeight, "Moving Attack", 0);
        }
    }
    private void AdjustLayerWeight(ref float weight, string layerName, float target)
    {
        weight = Mathf.Lerp(weight, target, LayerLerp);
        Animator.SetLayerWeight(Animator.GetLayerIndex(layerName), weight);
    }
    private bool CheckState(string stateName, string layerName = "Base Layer")
    {
        int layerIndex = Animator.GetLayerIndex(layerName);
        return Animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(stateName);
    }
    // 更新频率不同，必须缓冲
    void OnAnimatorMove() { if (slashing) moveDirection += Animator.deltaPosition; }
    public void OnRollEnter() { rolling = true; }
    public void OnRollExit() { rolling = false; }
    public void OnJabEnter() { jabbing = true; }
    public void OnJabExit() { jabbing = false; }
    public void OnSlashEnter() { slashing = true; }
    public void OnSlashExit() { slashing = false; }
}