using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 铲子：圆圈检测植物，按 Support &gt; Main 优先级选中；有其他植物时不可铲花盆。
/// </summary>
public class ShovelPanel : BaseHandPanel
{
    const int HitBufferSize = 32;
    static readonly Collider2D[] HitBuffer = new Collider2D[HitBufferSize];
    static readonly List<Chess> CandidateBuffer = new List<Chess>(16);

    Chess _hoverTarget;

    public override IEnumerator Plants(UnityAction CancelPlant, UnityAction<Chess> Plant, PrePlantImage_Data data)
    {
        _hoverTarget = null;
        int plantLayer = LayerMask.GetMask("Player");
        while (true)
        {
            Camera cam = Camera.main;
            Vector2 worldPos = cam != null
                ? (Vector2)cam.ScreenToWorldPoint(Input.mousePosition)
                : Vector2.zero;
            float radius = PrePlantImage.instance != null
                ? PrePlantImage.instance.ShovelDetectRadius
                : 1f;

            _hoverTarget = ResolveShovelTarget(worldPos, radius, plantLayer);
            UpdateHoverName(_hoverTarget);

            if (Input.GetMouseButtonDown(1))
            {
                ClearHover();
                CancelPlant?.Invoke();
                break;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (_hoverTarget != null && !_hoverTarget.IfDeath)
                {
                    Chess target = _hoverTarget;
                    ClearHover();
                    target.Death();
                    Plant?.Invoke(target);
                    break;
                }

                ClearHover();
                CancelPlant?.Invoke();
                break;
            }

            yield return null;
        }
    }

    public override void CancleUse()
    {
        ClearHover();
    }

    static void UpdateHoverName(Chess target)
    {
        if (PrePlantImage.instance == null) return;
        if (target == null)
        {
            PrePlantImage.instance.ClearShovelTargetName();
            return;
        }

        string name = target.propertyController?.creator?.chessName;
        PrePlantImage.instance.SetShovelTargetName(string.IsNullOrEmpty(name) ? target.name : name);
    }

    void ClearHover()
    {
        _hoverTarget = null;
        PrePlantImage.instance?.ClearShovelTargetName();
    }

    /// <summary>
    /// 圆内候选：排除 Consume；有非花盆时可铲目标时剔除花盆；Support &gt; Main &gt; 仅花盆；同级取最近。
    /// UnSelectable 因不在 Player 层，自然排除。
    /// </summary>
    public static Chess ResolveShovelTarget(Vector2 worldPos, float radius, int plantLayerMask)
    {
        CandidateBuffer.Clear();
        int count = Physics2D.OverlapCircleNonAlloc(worldPos, radius, HitBuffer, plantLayerMask);
        for (int i = 0; i < count; i++)
        {
            Collider2D col = HitBuffer[i];
            if (col == null) continue;
            Chess chess = col.GetComponent<Chess>() ?? col.GetComponentInParent<Chess>();
            if (!IsShovelCandidate(chess)) continue;
            if (!CandidateBuffer.Contains(chess))
                CandidateBuffer.Add(chess);
        }

        if (CandidateBuffer.Count == 0) return null;

        bool hasNonPot = false;
        for (int i = 0; i < CandidateBuffer.Count; i++)
        {
            if (!IsPotPlant(CandidateBuffer[i]))
            {
                hasNonPot = true;
                break;
            }
        }

        if (hasNonPot)
        {
            for (int i = CandidateBuffer.Count - 1; i >= 0; i--)
            {
                if (IsPotPlant(CandidateBuffer[i]))
                    CandidateBuffer.RemoveAt(i);
            }
        }

        if (CandidateBuffer.Count == 0) return null;

        Chess best = null;
        int bestPriority = int.MaxValue;
        float bestDistSq = float.MaxValue;
        for (int i = 0; i < CandidateBuffer.Count; i++)
        {
            Chess c = CandidateBuffer[i];
            int priority = GetShovelPriority(c);
            float distSq = ((Vector2)c.transform.position - worldPos).sqrMagnitude;
            if (priority < bestPriority || (priority == bestPriority && distSq < bestDistSq))
            {
                best = c;
                bestPriority = priority;
                bestDistSq = distSq;
            }
        }

        return best;
    }

    static bool IsShovelCandidate(Chess chess)
    {
        if (chess == null || chess.IfDeath) return false;
        var creator = chess.propertyController?.creator;
        if (creator == null) return false;
        PlantType type = creator.plantType;
        if ((type & PlantType.Consume) != 0) return false;
        // 仅考虑主/辅/花盆（可带 LimitType 等 flag）
        if ((type & (PlantType.MainPlant | PlantType.SupportPlant | PlantType.PotPlant)) == 0)
            return false;
        return true;
    }

    static bool IsPotPlant(Chess chess)
    {
        var type = chess.propertyController.creator.plantType;
        return (type & PlantType.PotPlant) != 0;
    }

    /// <summary>数值越小优先级越高：Support=0, Main=1, Pot=2。</summary>
    static int GetShovelPriority(Chess chess)
    {
        PlantType type = chess.propertyController.creator.plantType;
        if ((type & PlantType.SupportPlant) != 0) return 0;
        if ((type & PlantType.MainPlant) != 0) return 1;
        if ((type & PlantType.PotPlant) != 0) return 2;
        return 99;
    }
}
