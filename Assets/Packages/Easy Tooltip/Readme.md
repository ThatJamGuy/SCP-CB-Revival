# Easy Tooltip v2.0.1 by Ahmed Benlakhdhar

A robust, zero-setup tooltip system for Unity UI, configurable from the Inspector or entirely from code. Now supports fixed positioning, visual styling, and event hooks.


## Quick Start

The system is designed to "just work." You can add tooltips in two ways:

1.  **From the Inspector (Recommended):**
    - Add the "TooltipTrigger" component to any UI element.
    - Fill in the Title and Content fields.
    - (Optional) Customize Positioning and Styles using the override toggles.

2.  **From Code:**
    - Call the static method from any script:  
      `TooltipTrigger.AddTooltip(myGameObject, "My content");`

Done. The manager is created automatically.
(See the Demo Scene and Documentation for more examples).


## Key Features

- Zero Setup Required (Manager is auto-created)
- Inspector & C# API for Full Control
- Fixed & Relative Positioning (Anchors)
- Visual Style Overrides (Background, Outline, Colors)
- Unity Events (OnShow/OnHide)
- Rich Content (Title, Content, Icon)
- Smart Text Wrapping with Max Width
- Multi-Canvas Support (Overlay, Camera, World Space)
- Smart Screen Clamping & Flipping
- Smooth Fade Animations


## Configuration

To configure global settings (Max Width, Fade Speed, Default Styles), edit the "TooltipManager" prefab located at:
`Assets/Easy Tooltip/Resources/TooltipManager.prefab`


## Support

For the full manual, see the Documentation folder.

⭐⭐⭐⭐⭐ **Leave a Rating**

If Easy Tooltip saves you time and helps your project, please consider leaving a 5-star review on the [Asset Store page](https://assetstore.unity.com/packages/tools/gui/easy-tooltip-329113#reviews). It helps the asset grow immensely.

**Need Support?**  
Email is the fastest way to reach me. If you encounter any bugs, need help, or have feature requests, please contact me directly *before* leaving a review so I can resolve it for you immediately:
*   **Email:** ahmedbenlakhdhar [at] gmail [dot] com  
*(Please include "[Easy Tooltip]" in the email subject line so it doesn't get caught in spam.)*