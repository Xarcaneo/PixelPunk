using MenuFlow.Core;

namespace MenuFlow.Components.Navigation
{
    /// <summary>
    /// Button that returns to the previous menu panel in the navigation stack
    /// </summary>
    public class ReturnButton : MenuButton
    {
        /// <summary>
        /// Handles the button click by returning to the previous menu panel
        /// </summary>
        protected override async void OnClick()
        {
            if (MenuManager.Instance == null) return;

            await MenuManager.Instance.GoBack();
        }
    }
}
