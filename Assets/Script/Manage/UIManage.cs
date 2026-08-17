using System.Collections.Generic;
using AsoulChess.Game.Core.Services;
using AsoulChess.Game.Core.UI;

public class UIManage
{
    public static Dictionary<string, View> viewsDic = new Dictionary<string, View>();

    public UIManage()
    {
        viewsDic = UIRoot.m_views;
        InitializeShell();
        InitializeGameFlow();
    }

    static void InitializeShell()
    {
        foreach (var view in viewsDic)
        {
            view.Value.Init();
            view.Value.Hide();
        }

        GameServices.Register(ui: new UIService(ConvertViews(viewsDic)));
    }

    /// <summary>AVZ 启动流程：存档、StartUI 等（不进框架包）。</summary>
    public static void InitializeGameFlow()
    {
        if (PlayerSaveSystem.HasSave())
        {
            PlayerSaveContext.LoadCurrent();
            PlayerSaveContext.ApplyPlayerChessToGame();
            PlayerSaveContext.ApplyPlayerPropsToGame();
            PlayerSaveContext.ApplyLevelClearStateToGame();
            PlayerSaveContext.ApplySettingsToGame();
        }
        else
        {
            PlayerSaveContext.CurrentData = null;
        }

        Show<StartUI>();
        bool isTestMode = GameManage.instance != null && GameManage.instance.mode == GameMode.Test;
        if (!isTestMode && !PlayerSaveSystem.HasSave())
        {
            var panel = GetView<LoadSaveDataPanel>();
            if (panel != null) panel.Show();
        }
    }

    static Dictionary<string, UIView> ConvertViews(Dictionary<string, View> source)
    {
        var result = new Dictionary<string, UIView>();
        foreach (var pair in source)
            result[pair.Key] = pair.Value;
        return result;
    }

    public static T GetView<T>() where T : View
    {
        foreach (var itemView in viewsDic)
        {
            if (itemView.Value is T tView)
                return tView;
        }

        return null;
    }

    public static void Show<T>() where T : View
    {
        foreach (var itemView in viewsDic)
        {
            if (itemView.Value is T)
                itemView.Value.Show();
        }
    }

    public static void Show(string name) => viewsDic[name].Show();

    public static void Close<T>() where T : View
    {
        foreach (var itemView in viewsDic)
        {
            if (itemView.Value is T)
                itemView.Value.Hide();
        }
    }

    public static void Close(string name) => viewsDic[name].Hide();
}
