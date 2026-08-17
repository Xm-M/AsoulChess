using System.Collections.Generic;

namespace AsoulChess.Game.Core.UI
{
    public sealed class UIService : IUIService
    {
        readonly Dictionary<string, UIView> _views;

        public UIService(Dictionary<string, UIView> views)
        {
            _views = views ?? new Dictionary<string, UIView>();
        }

        public IReadOnlyDictionary<string, UIView> Views => _views;

        public void InitializeShell()
        {
            foreach (var pair in _views)
            {
                if (pair.Value == null) continue;
                pair.Value.Init();
                pair.Value.Hide();
            }
        }

        public T GetView<T>() where T : UIView
        {
            foreach (var view in _views.Values)
            {
                if (view is T typed)
                    return typed;
            }

            return null;
        }

        public void Show<T>() where T : UIView
        {
            foreach (var view in _views.Values)
            {
                if (view is T)
                    view.Show();
            }
        }

        public void Close<T>() where T : UIView
        {
            foreach (var view in _views.Values)
            {
                if (view is T)
                    view.Hide();
            }
        }

        public void Show(string viewName)
        {
            if (_views.TryGetValue(viewName, out var view) && view != null)
                view.Show();
        }

        public void Close(string viewName)
        {
            if (_views.TryGetValue(viewName, out var view) && view != null)
                view.Hide();
        }
    }
}
