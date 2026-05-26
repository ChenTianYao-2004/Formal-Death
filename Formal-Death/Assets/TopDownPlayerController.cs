using UnityEngine;

/// <summary>
/// 俯视角玩家控制器（游侠风格）
/// WASD = 世界坐标四方向移动，角色朝向自动平滑转向移动方向
/// 依赖：CharacterController、Animator（含 isWalking bool 参数）
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动速度（单位/秒）")]
    public float moveSpeed = 5f;

    [Header("转向设置")]
    [Tooltip("角色朝向跟随移动方向的旋转速度（度/秒），值越大转向越跟手")]
    public float rotationSpeed = 600f;

    [Header("重力设置")]
    public float gravity = -9.81f;

    // ── 私有字段 ──────────────────────────────────────────────
    private CharacterController _cc;
    private Animator _animator;
    private float _verticalVelocity;

    private static readonly int AnimIsWalking = Animator.StringToHash("isWalking");

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. 读取 WASD 输入，组成世界空间水平移动方向
        float h = Input.GetAxisRaw("Horizontal"); // A=-1, D=1
        float v = Input.GetAxisRaw("Vertical");   // S=-1, W=1

        Vector3 moveDir = new Vector3(h, 0f, v);
        bool isMoving = moveDir.sqrMagnitude > 0.001f;

        if (isMoving)
        {
            // 2. 归一化，避免斜向移动过快
            moveDir.Normalize();

            // 3. 角色朝向平滑转向移动方向（RotateTowards 有最大角速度，产生过渡感）
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // 4. 重力
        if (_cc.isGrounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;
        else
            _verticalVelocity += gravity * Time.deltaTime;

        // 5. 移动：沿角色当前朝向（transform.forward）走
        //    旋转到哪就走到哪，消除滑步瞬移感
        Vector3 velocity = (isMoving ? transform.forward : Vector3.zero) * moveSpeed;
        velocity.y = _verticalVelocity;
        _cc.Move(velocity * Time.deltaTime);

        // 6. 动画
        _animator.SetBool(AnimIsWalking, isMoving);
    }
}
