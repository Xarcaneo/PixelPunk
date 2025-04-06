# Basic Menus Sample

This sample demonstrates how to set up a basic menu system using MenuFlow.

## Setup Instructions

1. Create a new scene or use an existing one
2. Create a Canvas in your scene (right-click in Hierarchy > UI > Canvas)
3. Add a MenuManager component to an empty GameObject in your scene
4. Create menu definitions:
   - Right-click in Project window > Create > MenuFlow > Menu Definition
   - Create two: "MainMenu" and "SettingsMenu"
5. Create UI panels for each menu:
   - In your Canvas, create two empty GameObjects named "MainMenuPanel" and "SettingsMenuPanel"
   - Add a CanvasGroup component to each
   - Add the MenuPanel component to each
   - Design your UI within each panel (add buttons, text, etc.)
6. Set up navigation:
   - Add the MenuButton component to any button that should trigger navigation
   - For buttons that open the settings, assign the SettingsMenu definition
   - For back buttons, check the "Is Back Button" option

## Example Hierarchy

```
Scene
├── MenuManager
└── Canvas
    ├── MainMenuPanel (MenuPanel)
    │   ├── Title
    │   ├── Play Button
    │   └── Settings Button (MenuButton → SettingsMenu)
    └── SettingsPanel (MenuPanel)
        ├── Settings Title
        ├── Settings Options...
        └── Back Button (MenuButton → isBackButton=true)
```
