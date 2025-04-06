using UnityEngine;
using UnityEngine.UI;
using MenuFlow.Core;

namespace MenuFlow.Components.Navigation
{
    /// <summary>
    /// Abstract base class for menu navigation buttons.
    /// Provides common functionality for button initialization and cleanup.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public abstract class MenuButton : MonoBehaviour
    {
        /// <summary>
        /// The underlying Unity UI Button component
        /// </summary>
        protected Button button;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        protected virtual void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        /// <summary>
        /// Abstract method to be implemented by derived classes to define button behavior
        /// </summary>
        protected abstract void OnClick();
    }
}
