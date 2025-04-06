using System;
using MenuFlow.ScriptableObjects;

namespace MenuFlow.Editor
{
    public interface IPanelTracker
    {
        MenuDefinition CurrentPanel { get; }
        event Action OnPanelChanged;
        void Initialize(MenuSystem menuSystem);
        void UpdateCurrentPanel(MenuDefinition panel);
    }
}
