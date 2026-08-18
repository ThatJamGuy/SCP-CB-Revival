# Easy Tooltip v3.0.0 by Ahmed Benlakhdhar

A high-performance, zero-setup tooltip system for Unity UI. Designed for artists (via a robust custom inspector) and programmers (via a clean 1-line C# API). Features smart screen clamping, optimized object pooling, and dual-tooltip support.

## Quick Start

The system is designed to "just work." You can add tooltips in two ways:

1.  **From the Inspector (For Artists):**
    - Add the `TooltipTrigger` component to any UI element.
    - Fill in the Title and Content fields.
    - Click **Preview Tooltip** to view and edit your changes without entering Play Mode.
    - *(Optional)* Customize Positioning, Timing, and Styles using the override toggles.

2.  **From Code (For Programmers):**
    - Call the static method from any script:  
      ```csharp
      TooltipTrigger.AddTooltip(myGameObject, "My content", "My Title");
      ```

Done. The global manager is created automatically behind the scenes.
*(See the Game-Ready Showcase and Sandbox Demo scenes for advanced examples).*

## Key Features

- **One-Line C# API** (Instantly populate dynamic UI inventories via code)
- **Zero-Code Workflow** (Build complex stat cards entirely in the custom Inspector)
- **Live Editor Previews** (View and edit tooltips dynamically without entering Play Mode)
- **Optimized Object Pooling** (High performance, recycled instances with zero garbage collection spikes)
- **Smart Screen Constraints** (Auto-clamping and smart anchor flipping at screen edges)
- **Dual-Tooltip Support** (Decoupled `TooltipHintTrigger` for cursor-following prompts like "[RMB] Equip")
- **Multi-Block Auto-Separators** (Automatically injects and scales visual dividers between text blocks)
- **Custom Prefab Overrides** (Swap the global tooltip layout for massive RPG stat cards per-item)
- **Target Overrides** (Hover one UI element, spawn the tooltip on a remote target)
- **12-Point Anchoring & Continuous Tracking** (Complete spatial control)
- **Visual Style Overrides** (Local control over colors, sprites, outlines, and separator heights)
- **Safe Event Hooks** (`onTooltipShow` / `onTooltipHide` to trigger sounds or external logic)
- **Native VR Support** (Fixed positioning tested and optimized for Meta Quest 2)
- **Two Interactive Demo Scenes Included** (Game-Ready Showcase & Feature Sandbox)

## Configuration

To configure global settings (Default Styles, Max Width, Fade Speed, Global Screen Clamping), edit the `TooltipManager` prefab located at:
`Assets/Easy Tooltip/Resources/TooltipManager.prefab`

## Support

For the full manual, see the Documentation folder.

⭐⭐⭐⭐⭐ **Leave a Rating**

If Easy Tooltip saves you time and helps your project, please consider leaving a 5-star review on the [Asset Store page](https://assetstore.unity.com/packages/tools/gui/easy-tooltip-329113#reviews). It helps the asset grow immensely.

**Need Support?**  
Email is the fastest way to reach me. If you encounter any bugs, need help, or have feature requests, please contact me directly *before* leaving a review so I can resolve it for you immediately:
*   **Email:** [pixeladderdev@gmail.com](mailto:pixeladderdev@gmail.com) 
*(Please include "[Easy Tooltip]" in the email subject line so it doesn't get caught in spam.)*