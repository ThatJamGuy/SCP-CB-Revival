# Easy Tooltip

### User Manual & Documentation

**Version 3.0.0**  
**Created by Ahmed Benlakhdhar**

---

### **Table of Contents**
1.  Introduction
2.  Quick Start Guide
3.  Core Components
4.  Configuration
5.  FAQ & Support

---

### **1. Introduction**

Thank you for choosing Easy Tooltip! This asset is a high-performance, robust solution for adding professional tooltips to your Unity UI. 

Designed for both artists and programmers, Version 3.0.0 introduces a massive architectural upgrade featuring optimized object pooling, live editor previews, and dual-tooltip support.

**Key Features:**
*   **Zero Setup Required:** The global manager is auto-generated behind the scenes.
*   **Live Editor Previews:** View and edit tooltips directly in the Inspector without entering Play Mode.
*   **Optimized Object Pooling:** Automatically recycles tooltips and text blocks for maximum performance with zero lag spikes.
*   **Flexible Positioning:** 12-point "Fixed" anchors, static "Follow Mouse", continuous cursor tracking, and remote Target Overrides.
*   **Smart Constraints:** Auto-clamping prevents off-screen clipping, while auto-flipping mirrors tooltips that hit screen edges.
*   **Multi-Block Auto-Separators:** Dynamically injects scalable dividers between arrays of text.
*   **Dual-Tooltips:** Includes a decoupled `TooltipHintTrigger` for secondary cursor-following prompts (e.g., "[RMB] Equip").
*   **Extensive Styling:** Swap entire UI prefabs per item, or use local overrides for colors, sprites, and outlines.
*   **Event System:** UnityEvents (`onTooltipShow` / `onTooltipHide`) to easily trigger game logic or sounds.

---

### **2. Quick Start Guide**

The system is designed to "just work" in seconds. You can add tooltips in two ways:

#### **Method 1: Using the Inspector (For Artists/Designers)**

1.  **Add the Component:** Select any UI GameObject and add the `TooltipTrigger` component.
2.  **Add Content:** Fill in the Title and Content fields. (Use *Advanced Content* to automatically generate separated text blocks).
3.  **Preview It:** Click the **Preview Tooltip** button in the Inspector to see your tooltip instantly in the Scene/Game view. 
4.  **Customize:** Use the toggle switches (e.g., *Override Global Style*) to assign custom prefabs, change colors, or adjust anchor positions.

#### **Method 2: Using Code (For Programmers)**

You can add and customize tooltips entirely from your own scripts with a single static method.

**Example:**
```csharp
// Get a reference to your button's GameObject
public GameObject mySlot;

// 1. Add a simple tooltip in one line:
TooltipTrigger.AddTooltip(mySlot, "Restores 50 HP.", "Health Potion");

// 2. Add a complex multi-block tooltip with auto-separators:
List<string> stats = new List<string> { "Damage: 120", "Durability: 100/100" };
var trigger = TooltipTrigger.AddTooltip(mySlot, stats, "Epic Sword");

// 3. Customize settings via properties:
if (trigger != null)
{
    trigger.PositionMode = TooltipPositionMode.Fixed;
    trigger.AnchorPosition = TooltipAnchor.TopRight;
    trigger.PanelColor = Color.black;
}
```

*(See the included **Game-Ready Showcase** and **Feature Sandbox** scenes for live examples of these setups).*

---

### **3. Core Components**

*   **`TooltipTrigger`:** The main component you attach to your UI elements. It holds content, style overrides, position settings, and event hooks.
*   **`TooltipHintTrigger`:** A lightweight, decoupled trigger for secondary mouse-following hints (perfect for displaying hotkeys next to the cursor while the main tooltip anchors elsewhere).
*   **`TooltipManager`:** The "brain" of the system. It handles instantiation, optimized object pooling, smart screen clamping, and animation logic automatically.
*   **`Tooltip` Prefab:** The visual prefab. The package includes default, Sci-Fi, Tactical, and Fantasy examples. You can easily duplicate and edit these to create your own custom RPG layouts. Located in `Assets/Easy Tooltip/Prefabs/`.

---

### **4. Configuration**

You can configure your global project settings (Max Width, Fade Speeds, Global Clamping, and Default Colors) in two ways:

**1. Global Settings (Recommended):**
Edit the **`TooltipManager`** prefab directly. This changes the defaults for your whole project, saving you from setting up colors on every single trigger.
Prefab Path: `Assets/Easy Tooltip/Resources/TooltipManager.prefab`

**2. Per-Scene Overrides (Optional):**
Drag the `TooltipManager` prefab into a scene's hierarchy. The system will use this instance and its specific settings for that scene only.

---

### **5. FAQ & Support**

**Q: My tooltip isn't showing up when I hover. Why?**
A: Ensure the UI element with the `TooltipTrigger` has an `Image` or `Text` component with **Raycast Target** checked. Also, make sure no other invisible UI elements are blocking the raycast in front of it.

**Q: Does it work with the New Input System?**
A: Yes. The asset uses preprocessor directives to automatically detect and support both the Legacy Input Manager and the New Input System package. No setup is required.

**Q: Does this work in VR?**
A: Yes. However, because VR uses 3D laser raycasters instead of a screen-space mouse, you must set your triggers to **Fixed Position** (anchoring to the UI element). *Follow Mouse* positioning relies on 2D screen coordinates and is not recommended for VR UX. (Tested successfully on Meta Quest 2).

**Q: Does this work with multiple Canvases or World Space UI?**
A: Yes. The system automatically detects which Canvas the hovered object belongs to and ensures the tooltip is rendered on the correct layer and coordinate space.

**Q: How does "Smart Flipping" handle screen edges?**
A: If a tooltip is set to Fixed Positioning and anchored to the "Right", but moving it there pushes it off-screen, the system will automatically mirror the anchor to the "Left". If it still doesn't fit, it physically clamps to the screen bounds.

⭐⭐⭐⭐⭐ **Leave a Rating**

If Easy Tooltip saves you time and helps your project, please consider leaving a 5-star review on the [Asset Store page](https://assetstore.unity.com/packages/tools/gui/easy-tooltip-329113#reviews). It helps the asset grow immensely.

**Need Support?**  
Email is the fastest way to reach me. If you encounter any bugs, need help, or have feature requests, please contact me directly *before* leaving a review so I can resolve it for you immediately:
*   **Email:** [pixeladderdev@gmail.com](mailto:pixeladderdev@gmail.com) 
*(Please include "[Easy Tooltip]" in the email subject line so it doesn't get caught in spam.)*