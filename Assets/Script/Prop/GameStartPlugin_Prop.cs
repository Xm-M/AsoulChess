/// <summary>开局激活本局道具并显示 <see cref="PropPanel"/>。</summary>
public class GameStartPlugin_Prop : ILevelPlugin
{
    public void StadgeEffect(LevelController levelController)
    {
        if (GameManage.instance?.propManage == null) return;
        GameManage.instance.propManage.CheckProps();
        UIManage.Show<PropPanel>();
    }

    public void OverPlugin(LevelController levelController)
    {
        GameManage.instance?.propManage?.ClearProps();
        UIManage.Close<PropPanel>();
    }
}
