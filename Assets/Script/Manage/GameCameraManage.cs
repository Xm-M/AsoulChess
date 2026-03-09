using UnityEngine;
using Sirenix.OdinInspector;
public class GameCameraManage : IManager
{
 
    private Camera cam;
    private Transform camTf;

    // 相机“正常位置”（不包含晃动），晃动是在此基础上叠加
    private Vector3 basePos;
    private bool active;

    // shake state
    private float shakeTimeLeft;
    private float shakeDuration;
    private float shakeStrength;

    /// <summary>
    /// 初始化：绑定相机并记录初始位置
    /// </summary>
    public void InitManage()
    {
        EventController.Instance.AddListener(EventName.GameStart.ToString(), OnGameStart);
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), OnGameOver);
    }

    public void OnGameStart()
    {
        cam = Camera.main;
        camTf = cam.transform;
        basePos = camTf.position;
        active = true;
        // 开局通常不需要抖，确保状态干净
        StopShake(true);
        // basePos 更新为当前（避免你外部改过相机位置）
        if (camTf != null) basePos = camTf.position;
    }

    public void OnGameOver()
    {
        active = false;
        StopShake(true);
    }

    /// <summary>
    /// 外部驱动更新（GameManage 每帧调用）
    /// </summary>
    public void Tick(float dt)
    {
        if (!LevelManage.instance.IfGameStart||!active || camTf == null) return;

        // 你如果有“相机正常移动/跟随”，请每帧先调用 SetBasePosition(...)
        // 这里只做晃动叠加
        Vector3 offset = GetShakeOffset(dt);
        camTf.position = basePos + offset;
        //Debug.Log(camTf.position);
    }

    // =======================
    // Public API
    // =======================

    /// <summary>
    /// 手动绑定相机（推荐 GameManage 在初始化时传入）
    /// </summary>
    public void SetCamera(Camera camera)
    {
        cam = camera;
        camTf = cam != null ? cam.transform : null;
        if (camTf != null) basePos = camTf.position;
    }

    /// <summary>
    /// 设置相机“正常位置”（不含晃动）。
    /// 如果你的相机是固定的，初始化后不调用也行。
    /// 如果你之后要做跟随/移动，请每帧更新它。
    /// </summary>
    public void SetBasePosition(Vector3 worldPos)
    {
        if (camTf == null) return;
        basePos = new Vector3(worldPos.x, worldPos.y, camTf.position.z);
    }

    /// <summary>
    /// 触发晃动（可叠加：取更强/更久）
    /// duration：秒；strength：世界单位(建议0.03~0.5)
    /// </summary>
    public void Shake(float duration, float strength)
    {
        Debug.Log("震动");
        if (duration <= 0f || strength <= 0f) return;

        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimeLeft = Mathf.Max(shakeTimeLeft, duration);
        shakeStrength = Mathf.Max(shakeStrength, strength);
    }

    public void StopShake(bool restoreImmediately)
    {
        shakeTimeLeft = 0f;
        shakeDuration = 0f;
        shakeStrength = 0f;

        if (restoreImmediately && camTf != null)
            camTf.position = basePos;
    }

    // =======================
    // Internal
    // =======================

    private Vector3 GetShakeOffset(float dt)
    {
        if (shakeTimeLeft <= 0f) return Vector3.zero;

        // 衰减（越接近结束越小）
        float t = (shakeDuration <= 0f) ? 0f : (shakeTimeLeft / shakeDuration);
        float current = shakeStrength * t;

        Vector2 rnd = Random.insideUnitCircle * current;

        shakeTimeLeft -= dt;
        if (shakeTimeLeft <= 0f)
        {
            shakeTimeLeft = 0f;
            shakeDuration = 0f;
            shakeStrength = 0f;
        }
        //Debug.Log(new Vector3(rnd.x, rnd.y, 0f));
        return new Vector3(rnd.x, rnd.y, 0f);
    }
}