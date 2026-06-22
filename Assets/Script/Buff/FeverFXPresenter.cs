using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 挂在 <c>Resources/Effect/FeverFX</c> 根物体上：每帧读取共享 <see cref="AveMujica.feverCurrent"/> / <see cref="AveMujica.feverMax"/>，
/// 刷新子物体 <c>ColdImage</c> 的 <see cref="Image.fillAmount"/>；满条时播放 <c>Particle</c>。
/// 每名 AveMujica 成员各挂一条（由 <see cref="Buff_AveMujica"/> 入场时生成），数值全羁绊共享。
/// </summary>
public class FeverFXPresenter : MonoBehaviour
{
    public const string ResourcePath = "Effect/FeverFX";
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
        if (_user == null || _user.IfDeath)
        {
            DestroySafe();
            return;
        }

        AveMujica inst = AveMujica.Instance;
        if (inst == null)
        {
            DestroySafe();
            return;
        }

        float p = inst.GetFeverProgress01();
        if (_coldFill != null)
            _coldFill.fillAmount = p;

        UpdateParticleByFull(p >= 1f - 1e-4f);
    }

    void UpdateParticleByFull(bool feverFull)
    {
        if (_particleRoot == null)
            return;

        if (feverFull)
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

    public void DestroySafe()
    {
        if (gameObject != null)
            Destroy(gameObject);
    }

    public static FeverFXPresenter TrySpawnUnderChess(Chess user)
    {
        if (user == null)
            return null;

        var existing = user.GetComponentInChildren<FeverFXPresenter>(true);
        if (existing != null)
            return existing;

        var prefab = Resources.Load<GameObject>(ResourcePath);
        if (prefab == null)
        {
            Debug.LogWarning($"[FeverFX] Resources.Load 失败: {ResourcePath}");
            return null;
        }

        var go = Object.Instantiate(prefab, user.transform, false);
        go.name = "FeverFX";
        var p = go.GetComponent<FeverFXPresenter>();
        if (p == null)
            p = go.AddComponent<FeverFXPresenter>();
        p.Initialize(user);
        return p;
    }

    public static void DestroyAll()
    {
        var presenters = Object.FindObjectsOfType<FeverFXPresenter>();
        for (int i = 0; i < presenters.Length; i++)
        {
            if (presenters[i] != null)
                presenters[i].DestroySafe();
        }
    }
}
