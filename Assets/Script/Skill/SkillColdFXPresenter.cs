using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在 <c>Resources/Effect/SkillColdFX</c> 根物体上：每帧根据当前 <see cref="SkillController.activeSkill"/> 刷新子物体 <c>ColdImage</c> 的 <see cref="Image.fillAmount"/>；
/// 冷却完成（进度 ≥ 1）时开启子物体 <c>Particle</c> 上的 <see cref="ParticleSystem"/> 并播放，未完成则关闭并停止（由 <see cref="SkillReady_MouseDown"/> 入场时生成本组件）。
/// </summary>
public class SkillColdFXPresenter : MonoBehaviour
{
    public const string ResourcePath = "Effect/SkillColdFX";
    const string ColdImageName = "ColdImage";
    const string ParticleChildName = "Particle";

    Chess _user;
    Image _coldFill;
    GameObject _particleRoot;
    ParticleSystem _particlePs;

    public void Initialize(Chess user)
    {
        _user = user;
        var canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
            canvas.worldCamera = Camera.main;
        var tr = transform.Find(ColdImageName);
        if (tr != null)
            _coldFill = tr.GetComponent<Image>();

        var particleTr = transform.Find(ParticleChildName);
        if (particleTr != null)
        {
            _particleRoot = particleTr.gameObject;
            _particlePs = particleTr.GetComponent<ParticleSystem>();
            if (_particlePs == null)
                _particlePs = particleTr.GetComponentInChildren<ParticleSystem>(true);
            _particleRoot.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (_user == null)
        {
            DestroySafe();
            return;
        }
        if (_user.IfDeath)
        {
            DestroySafe();
            return;
        }
        var sc = _user.skillController;
        if (sc == null)
        {
            DestroySafe();
            return;
        }

        float p = SkillCooldownProgress.Resolve(sc.activeSkill);
        if (_coldFill != null)
            _coldFill.fillAmount = Mathf.Clamp01(p);

        UpdateParticleByCooldown(p >= 1f - 1e-4f);
    }

    void UpdateParticleByCooldown(bool cooldownComplete)
    {
        if (_particleRoot == null)
            return;

        if (cooldownComplete)
        {
            if (!_particleRoot.activeSelf)
                _particleRoot.SetActive(true);
            if (_particlePs != null && !_particlePs.isPlaying)
                _particlePs.Play();
        }
        else
        {
            if (_particlePs != null && _particlePs.isPlaying)
                _particlePs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (_particleRoot.activeSelf)
                _particleRoot.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (_user != null && _user.skillController != null && _user.skillController.skillColdFx == this)
            _user.skillController.skillColdFx = null;
    }

    public void DestroySafe()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }

    public static SkillColdFXPresenter TrySpawnUnderChess(Chess user)
    {
        if (user == null || user.skillController == null)
            return null;
        if (user.skillController.skillColdFx != null)
            return user.skillController.skillColdFx;

        var prefab = Resources.Load<GameObject>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"[SkillColdFX] Resources.Load 失败: {ResourcePath}");
            return null;
        }

        var go = Object.Instantiate(prefab, user.transform, false);
        go.name = "SkillColdFX";
        var p = go.GetComponent<SkillColdFXPresenter>();
        if (p == null)
            p = go.AddComponent<SkillColdFXPresenter>();
        p.Initialize(user);
        user.skillController.skillColdFx = p;
        return p;
    }
}

/// <summary>从 <see cref="ISkill"/> 具体类型解析冷却条进度。</summary>
public static class SkillCooldownProgress
{
    public static float Resolve(ISkill skill)
    {
        if (skill == null)
            return 1f;

        if (skill is ReplaceSkill replace && replace.CurrentSubSkill != null)
            return Resolve(replace.CurrentSubSkill);

        if (skill is MultySkill_Mio mio)
            return Mathf.Clamp01(mio.GetCooldownProgress01());

        if (skill is MultySkill multy)
            return ResolveMulty(multy);

        if (skill is ISkillCooldownProgress p)
            return Mathf.Clamp01(p.GetCooldownProgress01());

        return 1f;
    }

    static float ResolveMulty(MultySkill multy)
    {
        if (multy.skills == null || multy.skills.Count == 0)
            return 1f;
        float max = 0f;
        for (int i = 0; i < multy.skills.Count; i++)
        {
            var s = multy.skills[i];
            if (s is ISkillCooldownProgress p)
                max = Mathf.Max(max, Mathf.Clamp01(p.GetCooldownProgress01()));
        }
        return max;
    }
}
