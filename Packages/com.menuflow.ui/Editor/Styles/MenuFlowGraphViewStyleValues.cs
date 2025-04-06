using UnityEngine.UIElements;
using UnityEditor;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Provides access to MenuFlowGraphView style values defined in USS.
    /// </summary>
    public static class MenuFlowGraphViewStyleValues
    {
        /// <summary>
        /// Gets the horizontal spacing between nodes.
        /// </summary>
        public static float HorizontalSpacing => 100f;

        /// <summary>
        /// Gets the vertical spacing between nodes.
        /// </summary>
        public static float VerticalSpacing => 80f;

        /// <summary>
        /// Gets the root node X offset.
        /// </summary>
        public static float RootOffsetX => 20f;

        /// <summary>
        /// Gets the root node Y offset.
        /// </summary>
        public static float RootOffsetY => 20f;

        /// <summary>
        /// Gets the width of a node.
        /// </summary>
        public static float NodeWidth => 150f;

        /// <summary>
        /// Gets the height of a node.
        /// </summary>
        public static float NodeHeight => 80f;

        /// <summary>
        /// Applies the USS stylesheet to the specified element.
        /// </summary>
        public static void ApplyStyles(VisualElement element)
        {
            var stylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.menuflow.ui/Editor/Styles/MenuFlowGraphViewStyles.uss");
            if (stylesheet != null)
            {
                element.styleSheets.Add(stylesheet);
            }
        }
    }
}
