using UnityEditor;
using UnityEngine;
using MenuFlow.Components.Navigation;

namespace MenuFlow.Editor
{
    /// <summary>
    /// Custom editor for MenuButton components.
    /// Provides a specialized inspector interface for configuring menu button properties.
    /// </summary>
    [CustomEditor(typeof(MenuButton))]
    public class MenuButtonEditor : UnityEditor.Editor
    {
        private SerializedProperty targetMenuProperty;
        private SerializedProperty isBackButtonProperty;

        /// <summary>
        /// Initializes the editor by finding required serialized properties.
        /// </summary>
        private void OnEnable()
        {
            targetMenuProperty = serializedObject.FindProperty("targetMenu");
            isBackButtonProperty = serializedObject.FindProperty("isBackButton");
        }

        /// <summary>
        /// Draws the custom inspector GUI.
        /// Shows/hides the target menu field based on whether the button is a back button.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(isBackButtonProperty);

            // Only show target menu if not a back button
            if (!isBackButtonProperty.boolValue)
            {
                EditorGUILayout.PropertyField(targetMenuProperty);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
