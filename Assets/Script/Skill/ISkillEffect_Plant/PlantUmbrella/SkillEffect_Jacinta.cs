using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 嘉辛塔主动：动画 <c>UseSkill</c> 帧产阳；技能持续期间启用薇薇安同款挡弹，技能结束关闭。
/// </summary>
[Serializable]
public class SkillEffect_Jacinta : ISkillEffect
{
    [LabelText("阳光生成点")]
    public Transform sunLightPos;

    [LabelText("技能期挡弹")]
    [Tooltip("参数默认对齐薇薇安；仅技能窗口内生效")]
    public PassiveSkill_LettuceUmbrella umbrella = new PassiveSkill_LettuceUmbrella();

    bool _umbrellaActive;
    Chess _boundUser;

    public void SkillEffect(Chess user, SkillConfig config, List<Chess> targets)
    {
        SpawnSun(user, config);

        if (_umbrellaActive || user == null)
            return;

        _umbrellaActive = true;
        _boundUser = user;
        if (umbrella == null)
            umbrella = new PassiveSkill_LettuceUmbrella();
        umbrella.SkillEffect(user, config, targets);

        if (user.skillController != null)
            user.skillController.onSkillOver.AddListener(OnSkillOver);
        user.OnRemove.AddListener(OnUserRemove);
    }

    void SpawnSun(Chess user, SkillConfig config)
    {
        if (user?.moveController?.standTile == null)
            return;
        var panel = UIManage.GetView<ItemPanel>();
        if (panel == null)
            return;

        int amount = 20;
        if (config?.baseDamage != null && config.baseDamage.Count > 0)
            amount = Mathf.Max(0, (int)config.baseDamage[0]);

        Vector3 pos = sunLightPos != null ? sunLightPos.position : user.transform.position;
        var light = panel.Create<SunLight>() as SunLight;
        if (light != null)
            light.InitSunLight(user.moveController.standTile, amount, pos);
    }

    void OnSkillOver(Chess _) => EndUmbrellaSession();

    void OnUserRemove(Chess _) => EndUmbrellaSession();

    void EndUmbrellaSession()
    {
        if (!_umbrellaActive)
            return;
        _umbrellaActive = false;

        if (_boundUser != null)
        {
            if (_boundUser.skillController != null)
                _boundUser.skillController.onSkillOver.RemoveListener(OnSkillOver);
            _boundUser.OnRemove.RemoveListener(OnUserRemove);
        }

        umbrella?.StopUmbrella();
        _boundUser = null;
    }
}
