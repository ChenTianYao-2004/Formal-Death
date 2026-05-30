using UnityEngine;

/// <summary>
/// 俯视角玩家控制器（游侠风格）
/// WASD = 世界坐标四方向移动，角色朝向自动平滑转向移动方向
/// Shift = 按住加速跑步
/// 依赖：CharacterController、Animator（含 Float 参数 speed）
///   speed == 0   → 待机 Idle
///   0 < speed ≤ walkSpeed  → 走路 Walk
///   speed > walkSpeed      → 跑步 Run
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("步行速度（单位/秒）")]
    public float walkSpeed = 5f;

    [Tooltip("跑步速度（单位/秒）")]
    public float runSpeed = 9f;

    [Header("转向设置")]
    [Tooltip("角色朝向跟随移动方向的旋转速度（度/秒），值越大转向越跟手")]
    public float rotationSpeed = 600f;

    [Header("重力设置")]
    public float gravity = -9.81f;

    [Header("动画设置")]
    [Tooltip("Animator 中 Float 参数的名称")]
    public string speedParamName = "speed";

    [Tooltip("动画 speed 参数的平滑阻尼时间（秒），0 = 无过渡")]
    public float animDampTime = 0.1f;

    // ── 私有字段 ──────────────────────────────────────────────
    private CharacterController _cc;
    private Animator _animator;
    private float _verticalVelocity;
    private int _animSpeedHash;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _animSpeedHash = Animator.StringToHash(speedParamName);
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. 读取 WASD 输入
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(h, 0f, v);
        bool isMoving = moveDir.sqrMagnitude > 0.001f;

        // 2. 判断是否按住 Shift 跑步
        bool isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isMoving
            ? (isSprinting ? runSpeed : walkSpeed)
            : 0f;

        if (isMoving)
        {
            // 3. 归一化，避免斜向过快
            moveDir.Normalize();

            // 4. 朝向平滑转向移动方向
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // 5. 重力
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        // 6. 移动：沿角色当前朝向走，速度由 walk/run 决定
        Vector3 velocity = (isMoving ? transform.forward : Vector3.zero) * targetSpeed;
        velocity.y = _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);

        // 7. 动画：用带阻尼的 SetFloat 让过渡更顺滑
        _animator.SetFloat(_animSpeedHash, targetSpeed, animDampTime, Time.deltaTime);
    }
}