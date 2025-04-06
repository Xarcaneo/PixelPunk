using UnityEngine;
using MenuFlow.Core;
using MenuFlow.ScriptableObjects;

namespace MenuFlow.Components.Navigation
{
    /// <summary>
    /// Button that navigates to a specific menu panel
    /// </summary>
    public class NavigationButton : MenuButton
    {
        /// <summary>
        /// The target menu panel this button will navigate to when clicked
        /// </summary>
        [SerializeField]
        [Tooltip("The menu panel this button will navigate to")]
        private MenuDefinition targetMenu;

        /// <summary>
        /// Handles the button click by navigating to the target menu if one is assigned
        /// </summary>
        protected override async void OnClick()
        {
            if (MenuManager.Instance == null || targetMenu == null) return;

            await MenuManager.Instance.OpenMenu(targetMenu);
        }
    }
}
