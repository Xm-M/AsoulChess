using UnityEngine;

/// <summary>
/// 放学后茶会羁绊。配置：detectMode=Tag, tag=放学后茶会, tierThresholds=[1]
/// 种植带该 tag 的植物时为其添加 Buff_HoukagoTeaTime（额外治疗 + 均摊参与条件）。
/// </summary>
public class HoukagoTeaTime : Fetter
{
    public const string PlantTag = "放学后茶会";

    [SerializeReference]
    public Buff_HoukagoTeaTime buffTemplate;

    public override void FetterEffect(int count, int tier)
    {
        base.FetterEffect(count, tier);
        EventController.Instance.AddListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    }

    public override void ResetFetter()
    {
        base.ResetFetter();
        EventController.Instance.RemoveListener<Chess>(EventName.WhenPlantChess.ToString(), OnPlantChess);
    }

    void OnPlantChess(Chess chess)
    {
        if (chess == null || !chess.CompareTag("Player")) return;
        if (chess.propertyController?.creator?.plantTags == null) return;
        if (!chess.propertyController.creator.plantTags.Contains(PlantTag)) return;
        if (buffTemplate == null) return;
        chess.buffController.AddBuff(buffTemplate.Clone());
    }
}
