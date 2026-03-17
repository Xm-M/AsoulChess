using UnityEngine;

/// <summary>
/// 僵尸死亡时 10% 概率掉落金币道具。银币 10（90%）、金币 100（9.9%）、钻石 1000（0.1%）。
/// 仅当商店已解锁时生效（Test 模式视为已解锁）。
/// </summary>
public class Buff_DeathCoin : Buff
{
    const float DropChance = 1f;
    const float SilverChance = 0.9f;   // 银币 10
    const float GoldChance = 0.099f;   // 金币 100
    // 钻石 1000: 0.1%

    public Buff_DeathCoin() { buffName = "死亡金币"; }

    public override void BuffEffect(Chess target)
    {
        base.BuffEffect(target);
        target.DeathEvent.AddListener(OnDeath);
    }

    void OnDeath(Chess c)
    {
        if (Random.Range(0f, 1f) >= DropChance) return;

        float r = Random.Range(0f, 1f);
        int amount = r < SilverChance ? 10 : r < SilverChance + GoldChance ? 100 : 1000;

        var item = UIManage.GetView<ItemPanel>()?.Create<Item_Coin>();
        if (item != null)
            item.InitCoin(amount, c.transform.position);
    }

    public override void BuffOver()
    {
        if (target != null)
            target.DeathEvent.RemoveListener(OnDeath);
        base.BuffOver();
    }
}
