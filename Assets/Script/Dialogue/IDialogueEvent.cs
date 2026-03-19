/// <summary>
/// 对话事件：在 EnterMapPlugin_Dialogue 中统一注册/注销，用于处理对话触发的逻辑。
/// </summary>
public interface IDialogueEvent
{
    void Register(LevelController levelController);
    void Unregister();
}
