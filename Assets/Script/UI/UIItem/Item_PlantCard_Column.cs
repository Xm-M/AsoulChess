/// <summary>传送带列种植卡：种植时整列落子（逐格 IfCanPlant）。</summary>
public class Item_PlantCard_Column : Item_PlantCard
{
    protected override HandItemType PlantHandType => HandItemType.ColumnPlants;

    protected override void RecycleToPool()
    {
        UIManage.GetView<ItemPanel>().Recycle<Item_PlantCard_Column>(this);
    }
}
