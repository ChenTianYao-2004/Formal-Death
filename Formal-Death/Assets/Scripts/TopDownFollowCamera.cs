using UnityEngine;

/// <summary>
/// 俯视角平滑跟随摄像机
/// 使用方法：将此脚本挂载到 Main Camera，在 Inspector 中将玩家拖入 Target 字段。
/// </summary>
public class TopDownFollowCamera : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("玩家 Transform")]
    public Transform target;

    [Header("摄像机位置")]
    [Tooltip("摄像机在目标正上方的高度")]
    public float height = 15f;

    [Tooltip("摄像机沿目标背后的水平偏移（0 = 正上方，正值 = 略偏后方）")]
    public float backOffset = 3f;

    [Header("俯视角度")]
    [Tooltip("摄像机俯视角度（度），90 = 完全垂直俯视，60~80 为常见俯视角")]
    [Range(30f, 90f)]
    public float pitchAngle = 75f;

    [Header("平滑参数")]
    [Tooltip("位置跟随平滑速度，值越大跟随越紧")]
    public float positionSmoothSpeed = 8f;

    [Tooltip("旋转跟随平滑速度（仅在跟随玩家朝向时生效）")]
    public float rotationSmoothSpeed = 5f;

    [Tooltip("是否跟随玩家的水平朝向（Y 轴旋转）")]
    public bool followPlayerYaw = false;

    // 当前跟随的 Y 轴旋转角
    private float _currentYaw;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogWarning("[TopDownFollowCamera] 未设置 Target，请在 Inspector 中指定玩家。");
            return;
        }
        // 初始化 Yaw 为目标当前朝向，避免游戏开始时镜头跳变
        _currentYaw = target.eulerAngles.y;
        SnapToTarget(); // 第一帧直接到位，不做插值
    }

    private void LateUpdate()
    {
        if (target == null) return;
        SmoothFollow();

        if (Input.GetKeyDown(KeyCode.LeftBracket)) { height -= 0.25f; backOffset -= 0.25f; }
        if (Input.GetKeyDown(KeyCode.RightBracket)) { height += 0.25f; backOffset += 0.25f; }
    }

    // ── 核心跟随逻辑 ─────────────────────────────────────────

    private void SmoothFollow()
    {
        // 1. 计算目标 Yaw（水平朝向）
        float targetYaw = followPlayerYaw ? target.eulerAngles.y : _currentYaw;
        _currentYaw = followPlayerYaw
            ? Mathf.LerpAngle(_currentYaw, targetYaw, rotationSmoothSpeed * Time.deltaTime)
            : 0f; // 不跟随时固定朝向北方（Z+）

        float yaw = followPlayerYaw ? _currentYaw : 0f;

        // 2. 计算摄像机期望位置
        //    以目标为原点，沿 yaw 方向的反方向偏移 backOffset，再抬高 height
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 backDir = yawRotation * Vector3.back; // 目标背后方向
        Vector3 desiredPos = target.position
                                 + backDir * backOffset
                                 + Vector3.up * height;

        // 3. 平滑插值位置
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPos,
            positionSmoothSpeed * Time.deltaTime
        );

        // 4. 始终看向目标（稍微看向目标前方让画面更舒适）
        Vector3 lookTarget = target.position;
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(lookTarget - transform.position, Vector3.up),
            positionSmoothSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// 第一帧直接吸附到目标位置，避免摄像机从场景原点飞过来。
    /// </summary>
    private void SnapToTarget()
    {
        float yaw = followPlayerYaw ? target.eulerAngles.y : 0f;
        Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
        Vector3 backDir = yawRot * Vector3.back;
        transform.position = target.position + backDir * backOffset + Vector3.up * height;
        transform.LookAt(target.position);
    }

    // ── 编辑器辅助 ───────────────────────────────────────────
#if UNITY_EDITOR
    /// <summary>在 Scene 视图中绘制摄像机到目标的连线，方便调试。</summary>
    private void OnDrawGizmosSelected()
    {
        if (target == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.3f);
    }
#endif
}