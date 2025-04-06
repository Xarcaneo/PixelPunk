using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using MenuFlow.ScriptableObjects;

namespace MenuFlow.Editor.CodeGeneration
{
    /// <summary>
    /// Generates constants for all menus in the MenuFlow system.
    /// Use Tools > MenuFlow > Generate Menu Constants to update the constants.
    /// </summary>
    public class MenuConstantsGenerator
    {
        private const string CONSTANTS_PATH = "Packages/com.menuflow.ui/Runtime/Generated/MenuFlowConstants.cs";

        /// <summary>
        /// Force regeneration of constants from menu system.
        /// </summary>
        [MenuItem("Tools/MenuFlow/Generate Menu Constants")]
        public static void RegenerateConstants()
        {
            var menuSystem = FindMenuSystem();
            if (menuSystem == null)
            {
                Debug.LogWarning("MenuFlow: No MenuSystem asset found. Cannot generate constants.");
                return;
            }

            var constants = GenerateConstantsFile(menuSystem);
            var fullPath = Path.GetFullPath(CONSTANTS_PATH);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, constants);
            AssetDatabase.Refresh();
            Debug.Log("MenuFlow: Successfully generated menu constants.");
        }

        private static MenuSystem FindMenuSystem()
        {
            var guids = AssetDatabase.FindAssets("t:MenuSystem");
            if (guids.Length == 0) return null;
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<MenuSystem>(path);
        }

        private static string GenerateConstantsFile(MenuSystem menuSystem)
        {
            var sb = new StringBuilder();
            
            // File header
            sb.AppendLine("// This file is auto-generated. Do not modify it manually.");
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine("namespace MenuFlow");
            sb.AppendLine("{");
            sb.AppendLine("    /// <summary>");
            sb.AppendLine("    /// Auto-generated constants for MenuFlow system.");
            sb.AppendLine("    /// Contains type-safe references to all menus in the system.");
            sb.AppendLine("    /// </summary>");
            sb.AppendLine("    public static class MenuFlowConstants");
            sb.AppendLine("    {");
            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Constants for all menus in the MenuFlow system.");
            sb.AppendLine("        /// These are automatically generated from your MenuSystem asset.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        public static class Menus");
            sb.AppendLine("        {");

            // Generate constants for each menu
            var addedMenus = new System.Collections.Generic.HashSet<string>();
            
            // Add initial panel if it exists
            if (menuSystem.initialPanel != null)
            {
                AddMenuConstant(sb, menuSystem.initialPanel.name, addedMenus);
            }

            // Add all scene-specific panels
            foreach (var sceneEntry in menuSystem.scenes)
            {
                foreach (var panel in sceneEntry.panels)
                {
                    if (panel != null)
                    {
                        AddMenuConstant(sb, panel.name, addedMenus);
                    }
                }
            }

            // Close class definitions
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AddMenuConstant(StringBuilder sb, string menuName, System.Collections.Generic.HashSet<string> addedMenus)
        {
            if (string.IsNullOrEmpty(menuName) || !addedMenus.Add(menuName))
                return;

            var safeName = menuName.Replace(" ", "").Replace("-", "_");
            sb.AppendLine($"            /// <summary>Menu: {menuName}</summary>");
            sb.AppendLine($"            public const string {safeName} = \"{menuName}\";");
        }
    }
}
