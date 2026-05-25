/// <summary>肉鸽地图节点房间类型（与 <see cref="LevelData"/> 解耦，进入节点时再解析具体关卡）。</summary>
public enum MapRoomType
{
    Start,
    Normal,
    Elite,
    Boss,
    Rest,
    Shop,
    Event,
}
