using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using TMPro;

public class DamagePanel : View
{
    [FoldoutGroup("Color")]
    public Color Physics;
    [FoldoutGroup("Color")]
    public Color Magic;
    [FoldoutGroup("Color")]
    public Color Real;
    [FoldoutGroup("Heal")]
    public Color Heal;
    public GameObject damageText;
    public GameObject chineseText;
    public GameObject healPrefab;
    public bool showDamage;

    [FoldoutGroup("Stack")]
    [LabelText("垂直层间距（屏幕像素）")]
    public float stackSpacing = 18f;
    [FoldoutGroup("Stack")]
    [LabelText("水平抖动（屏幕像素）")]
    public float horizontalJitter = 12f;
    [FoldoutGroup("Stack")]
    [LabelText("基础下移（屏幕像素）")]
    public float baseYOffset = 16f;
    [FoldoutGroup("Stack")]
    [LabelText("垂直抖动向下（屏幕像素）")]
    public float verticalJitterDown = 14f;
    [FoldoutGroup("Stack")]
    [LabelText("垂直抖动向上（屏幕像素）")]
    public float verticalJitterUp = 4f;
    [FoldoutGroup("Stack")]
    [LabelText("世界锚点高度偏移")]
    public float worldAnchorUp = 0.55f;
    [FoldoutGroup("Stack")]
    [LabelText("飘字时长（与 DamageCanver 动画一致）")]
    public float floatingLifetime = 0.75f;

    readonly Dictionary<int, int> _activeStacks = new Dictionary<int, int>();

    public override void Init()
    {
        EventController.Instance.AddListener(EventName.WhenLeaveLevel.ToString(), ClearStacks);
    }

    void OnDestroy()
    {
        EventController.Instance?.RemoveListener(EventName.WhenLeaveLevel.ToString(), ClearStacks);
    }

    void ClearStacks() => _activeStacks.Clear();

    public void ShowDamageMes(DamageMessege dm)
    {
        if (dm == null || dm.suppressFloatingDamage) return;
        if (!showDamage || dm.damage < 1.5f) return;
        if (dm.damageTo == null || Camera.main == null) return;

        int slot = AcquireStackSlot(dm.damageTo);
        GameObject text = SpawnFloatingText(dm.damageTo, damageText, slot);
        if (text == null) return;

        TMP_Text t = text.GetComponentInChildren<TMP_Text>();
        if (t == null)
        {
            ReleaseStackSlot(dm.damageTo);
            ObjectPool.instance.Recycle(text);
            return;
        }

        if (!dm.ifCrit)
        {
            text.transform.localScale = Vector3.one;
            t.text = ((int)dm.damage).ToString();
        }
        else
        {
            text.transform.localScale = Vector3.one * 1.25f;
            t.text = ((int)dm.damage).ToString() + "!!";
        }

        switch (dm.damageType)
        {
            case DamageType.Physical: t.color = Physics; break;
            case DamageType.Magic: t.color = Magic; break;
            case DamageType.Real: t.color = Real; break;
            default: t.color = Color.white; break;
        }

        ScheduleStackRelease(dm.damageTo);
    }

    public void ShowMiss(DamageMessege dm)
    {
        if (dm == null || dm.damageTo == null || Camera.main == null) return;

        int slot = AcquireStackSlot(dm.damageTo);
        GameObject text = SpawnFloatingText(dm.damageTo, damageText, slot);
        if (text == null) return;

        TMP_Text t = text.GetComponentInChildren<TMP_Text>();
        if (t == null)
        {
            ReleaseStackSlot(dm.damageTo);
            ObjectPool.instance.Recycle(text);
            return;
        }

        text.transform.localScale = Vector3.one;
        t.text = "Miss";
        t.color = Color.white;
        ScheduleStackRelease(dm.damageTo);
    }

    public void ShowText(DamageMessege dm, string mes, Color color)
    {
        if (dm == null || dm.damageTo == null || Camera.main == null) return;

        int slot = AcquireStackSlot(dm.damageTo);
        GameObject text = SpawnFloatingText(dm.damageTo, chineseText, slot);
        if (text == null) return;

        TMP_Text t = text.GetComponentInChildren<TMP_Text>();
        if (t == null)
        {
            ReleaseStackSlot(dm.damageTo);
            ObjectPool.instance.Recycle(text);
            return;
        }

        text.transform.localScale = Vector3.one;
        t.text = mes;
        t.color = color;
        ScheduleStackRelease(dm.damageTo);
    }

    public void ShowHeal(float heal, Chess target)
    {
        if (!showDamage || target == null || Camera.main == null) return;

        int slot = AcquireStackSlot(target);
        GameObject text = SpawnFloatingText(target, damageText, slot);
        if (text == null) return;

        text.transform.localScale = Vector3.one;
        TMP_Text t = text.GetComponentInChildren<TMP_Text>();
        if (t == null)
        {
            ReleaseStackSlot(target);
            ObjectPool.instance.Recycle(text);
            return;
        }

        t.text = "+" + ((int)heal).ToString();
        t.color = Heal;
        ScheduleStackRelease(target);

        if (healPrefab != null)
        {
            GameObject healeffect = ObjectPool.instance.Create(healPrefab);
            healeffect.transform.SetParent(target.transform);
            healeffect.transform.localPosition = Vector3.zero;
        }
    }

    int AcquireStackSlot(Chess target)
    {
        int id = target.GetInstanceID();
        if (!_activeStacks.TryGetValue(id, out int count))
            count = 0;

        int slot = count;
        _activeStacks[id] = count + 1;
        return slot;
    }

    void ReleaseStackSlot(Chess target)
    {
        if (target == null) return;
        ReleaseStackSlot(target.GetInstanceID());
    }

    void ReleaseStackSlot(int targetId)
    {
        if (!_activeStacks.TryGetValue(targetId, out int count))
            return;

        count--;
        if (count <= 0)
            _activeStacks.Remove(targetId);
        else
            _activeStacks[targetId] = count;
    }

    GameObject SpawnFloatingText(Chess target, GameObject prefab, int slot)
    {
        if (prefab == null || target == null)
        {
            ReleaseStackSlot(target);
            return null;
        }

        GameObject text = ObjectPool.instance.Create(prefab);
        text.transform.SetParent(transform, false);
        text.transform.position = GetStackedScreenPosition(target, slot);
        return text;
    }

    void ScheduleStackRelease(Chess target)
    {
        if (target != null)
            StartCoroutine(ReleaseStackSlotAfter(target.GetInstanceID(), floatingLifetime));
    }

    Vector3 GetStackedScreenPosition(Chess target, int slot)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position + Vector3.up * worldAnchorUp);
        if (horizontalJitter > 0f)
            screenPos.x += Random.Range(-horizontalJitter, horizontalJitter);

        screenPos.y -= baseYOffset;
        if (verticalJitterDown > 0f || verticalJitterUp > 0f)
            screenPos.y += Random.Range(-verticalJitterDown, verticalJitterUp);
        screenPos.y += slot * stackSpacing;
        return screenPos;
    }

    IEnumerator ReleaseStackSlotAfter(int targetId, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseStackSlot(targetId);
    }
}
