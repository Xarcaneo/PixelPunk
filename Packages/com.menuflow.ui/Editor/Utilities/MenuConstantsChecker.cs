using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using MenuFlow.ScriptableObjects;

namespace MenuFlow.Editor.Utilities
{
    /// <summary>
    /// Utility class to check if MenuFlowConstants are in sync with the current MenuSystem.
    /// </summary>
    public static class MenuConstantsChecker
    {
        private const string CONSTANTS_PATH = "Packages/com.menuflow.ui/Runtime/Generated/MenuFlowConstants.cs";

        /// <summary>
        /// Checks if the generated constants match the current menu system.
        /// </summary>
        /// <param name="menuSystem">The menu system to check against</param>
        /// <returns>True if constants are up to date</returns>
        public static bool AreConstantsUpToDate(MenuSystem menuSystem)
        {
            if (menuSystem == null) return false;

            var currentMenus = GetCurrentMenuNames(menuSystem);
            var constantMenus = GetConstantMenuNames();

            return currentMenus.SetEquals(constantMenus);
        }

        private static HashSet<string> GetCurrentMenuNames(MenuSystem menuSystem)
        {
            var menus = new HashSet<string>();

            // Add initial panel
            if (menuSystem.initialPanel != null)
            {
                menus.Add(menuSystem.initialPanel.name);
            }

            // Add all scene panels
            foreach (var scene in menuSystem.scenes)
            {
                foreach (var panel in scene.panels)
                {
                    if (panel != null)
                    {
                        menus.Add(panel.name);
                    }
                }
            }

            return menus;
        }

        private static HashSet<string> GetConstantMenuNames()
        {
            var menus = new HashSet<string>();
            var fullPath = Path.GetFullPath(CONSTANTS_PATH);

            if (!File.Exists(fullPath))
                return menus;

            var content = File.ReadAllText(fullPath);
            var pattern = @"public const string (\w+) = ""([^""]+)"";";
            var matches = Regex.Matches(content, pattern);

            foreach (Match match in matches)
            {
                menus.Add(match.Groups[2].Value);
            }

            return menus;
        }
    }
}
