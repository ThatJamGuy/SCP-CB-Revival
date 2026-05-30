# Easy Tooltip

### User Manual & Documentation

**Version 2.0.1**
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

Thank you for choosing Easy Tooltip! This asset is a lightweight and easy-to-use solution for adding professional tooltips to your Unity project.

**Key Features:**
*   **Zero Setup Required:** Works out of the box for both Inspector and code-based tooltips.
*   **Inspector & C# API:** Create and customize tooltips visually or entirely from code.
*   **Flexible Positioning:** Choose between "Follow Mouse" or "Fixed" modes (e.g., Top-Left, Bottom-Center) with smart screen flipping.
*   **Visual Styling:** Override background colors, outline colors, and toggles per tooltip.
*   **Event System:** UnityEvents (OnShow/OnHide) allow you to trigger game logic easily.
*   **Rich Content:** Supports titles, main content, and icons.
*   **Multi-Canvas Support:** Works seamlessly across Screen Space (Overlay/Camera) and World Space UIs.
*   **Smart Clamping:** Automatically keeps tooltips on-screen, accounting for Scale, Pivot, and UI Outlines.

---

### **2. Quick Start Guide**

The system is designed to "just work" in seconds. You can add tooltips in two ways:

#### **Method 1: Using the Inspector (Recommended for Designers)**

1.  **Add the `TooltipTrigger` Component:**
    Select any UI GameObject and add the `TooltipTrigger` component.

2.  **Add Content:**
    Fill in the fields in the Inspector.

3.  **Customize (Optional):**
    Use the checkboxes (Override Defaults) to change positioning, styling, or timing for this specific trigger.

**Done!** The `TooltipManager` is created automatically.

#### **Method 2: Using Code (Recommended for Programmers)**

You can add and customize tooltips entirely from your own scripts with a single static method.

**Example:**
```csharp
// Get a reference to your button's GameObject
public GameObject myButton;

// Add a simple tooltip in one line
TooltipTrigger.AddTooltip(myButton, "This is a procedural tooltip.");

// Or, add a complex tooltip and customize styles/positioning
var trigger = TooltipTrigger.AddTooltip(myButton, "Stats and info here.", "Magic Sword");

if (trigger != null)
{
    // Custom Style
    trigger.BackgroundColor = Color.black;
    trigger.ShowOutline = true;
    trigger.OutlineColor = Color.cyan;

    // Fixed Position (Top Right of the target)
    trigger.PositionMode = TooltipPositionMode.Fixed;
    trigger.AnchorPosition = TooltipAnchor.TopRight;
}
```

*(See the Demo Scene in `Assets/Easy Tooltip/Demo` for live examples of both methods.)*

---

### **3. Core Components**

*   **`TooltipTrigger`:** The main component you add to your UI elements. It holds content, style overrides, position settings, and event hooks.
*   **`TooltipManager`:** The "brain" of the system. It handles instantiation, global defaults, smart positioning, and animation logic automatically.
*   **`Tooltip` Prefab:** The visual prefab for the tooltip. You can edit it to change the default fonts, padding, or sprite slicing. It is located in `Assets/Easy Tooltip/Prefabs/`.

---

### **4. Configuration**

You can configure global settings (Max Width, Fade Speed, Default Colors) in two ways:

**1. Global Settings (Recommended):**
Edit the **`TooltipManager` prefab** directly. This changes the defaults for your whole project.
Prefab Path: `Assets/Easy Tooltip/Resources/TooltipManager.prefab`

**2. Per-Scene Overrides (Optional):**
Drag the `TooltipManager` prefab into a scene's hierarchy to use different settings for that scene only.

---

### **5. FAQ & Support**

**Q: My tooltip isn't showing up when I hover. Why?**
A: Ensure the UI element with the `TooltipTrigger` has an `Image` or `Text` component with **Raycast Target** checked. Also, make sure no other invisible UI elements are blocking the raycast in front of it.

**Q: Does it work with the New Input System?**
A: Yes. The asset uses preprocessor directives to automatically detect and support both the Legacy Input Manager and the New Input System package. No setup is required.

**Q: Does this work with multiple Canvases or World Space UI?**
A: Yes. The system automatically detects which Canvas the hovered object belongs to and ensures the tooltip is rendered on the correct layer and coordinate space.

**Q: How does "Fixed" positioning handle screen edges?**
A: The manager uses "Smart Flipping." If a tooltip anchored to the "Top" goes off-screen, it will automatically try to flip to the "Bottom." If it still doesn't fit, it clamps to the screen edge.

⭐⭐⭐⭐⭐ **Leave a Rating**

If Easy Tooltip saves you time and helps your project, please consider leaving a 5-star review on the [Asset Store page](https://assetstore.unity.com/packages/tools/gui/easy-tooltip-329113#reviews). It helps the asset grow immensely.

**Need Support?**  
Email is the fastest way to reach me. If you encounter any bugs, need help, or have feature requests, please contact me directly *before* leaving a review so I can resolve it for you immediately:
*   **Email:** ahmedbenlakhdhar [at] gmail [dot] com  
*(Please include "[Easy Tooltip]" in the email subject line so it doesn't get caught in spam.)*