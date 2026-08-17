using System.Collections.Generic;

namespace AsoulChess.Game.Core.UI
{
    public interface IUIService
    {
        IReadOnlyDictionary<string, UIView> Views { get; }

        T GetView<T>() where T : UIView;
        void Show<T>() where T : UIView;
        void Close<T>() where T : UIView;
        void Show(string viewName);
        void Close(string viewName);
        void InitializeShell();
    }
}
