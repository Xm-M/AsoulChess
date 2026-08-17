using UnityEngine;

namespace AsoulChess.Game.Core.UI
{
    public abstract class UIView : MonoBehaviour
    {
        public abstract void Init();
        public virtual void Hide() => gameObject.SetActive(false);
        public virtual void Show() => gameObject.SetActive(true);
    }
}
