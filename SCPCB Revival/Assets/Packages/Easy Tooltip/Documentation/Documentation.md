# Easy Tooltip

### User Manual & Documentation

**Version 1.1**
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
*   **Rich Content:** Supports titles, main content, and icons.
*   **Fully Stylable:** Set custom colors for the title and icon.
*   **Smart Text Wrapping:** Automatically wraps long text to respect a 'Max Tooltip Width'.
*   **Outline Aware Positioning:** Automatically adjusts to keep tooltips on-screen, even when using a UI Outline.
*   **Smooth Animations:** Fades tooltips in and out for a professional feel.

---

### **2. Quick Start Guide**

The system is designed to "just work" in seconds. You can add tooltips in two ways:

#### **Method 1: Using the Inspector (Recommended for Designers)**

1.  **Add the `TooltipTrigger` Component:**
    Select any UI GameObject and add the `TooltipTrigger` component.

2.  **Add Content:**
    Fill in the fields in the Inspector.

**Done!** The `TooltipManager` is created automatically.

#### **Method 2: Using Code (Recommended for Programmers)**

You can add and customize tooltips entirely from your own scripts with a single static method.

**Example:**
```csharp
// Get a reference to your button's GameObject
public GameObject myButton;

// Add a simple tooltip in one line
TooltipTrigger.AddTooltip(myButton, "This is a procedural tooltip.");

// Or, add a complex tooltip and customize it
var trigger = TooltipTrigger.AddTooltip(myButton, "Stats and info here.", "Magic Sword");
trigger.TitleColor = Color.cyan;
trigger.HoverDelay = 1.0f;
```

*(See the Demo Scene in `Assets/Easy Tooltip/Demo` for live examples of both methods.)*

---

### **3. Core Components**

*   **`TooltipTrigger`:** The main component you add to your UI elements. It holds the content for a tooltip.
*   **`TooltipManager`:** The "brain" of the system. It is created automatically from the prefab in your `Resources` folder.
*   **`Tooltip` Prefab:** The visual prefab for the tooltip. You can edit it to change the default fonts, background, etc. It is located in `Assets/Easy Tooltip/Prefabs/`.

---

### **4. Configuration**

You can configure global settings (Max Width, Fade Speed, etc.) in two ways:

**1. Global Settings (Recommended):**
Edit the **`TooltipManager` prefab** directly. This changes the defaults for your whole project.
Prefab Path: `Assets/Easy Tooltip/Resources/TooltipManager.prefab`

**2. Per-Scene Overrides (Optional):**
Drag the `TooltipManager` prefab into a scene's hierarchy to use different settings for that scene only.

---

### **5. FAQ & Support**

**Q: My tooltip is appearing behind my other UI!**
A: The system automatically places the tooltip on top of all other UI on the same Canvas. If you are using multiple Canvases, ensure the Canvas the tooltip is on has a higher "Sort Order" in its Inspector.

**Q: My tooltip's positioning is slightly off when I use a UI Outline.**
A: This is handled automatically! The positioning system detects if the tooltip prefab uses an `Outline` component and adjusts its on-screen clamping to make sure the outline is never cut off.

**Contact for support or feature requests:**
*   LinkedIn: https://www.linkedin.com/in/ahmedbenlakhdhar
*   ArtStation: https://www.artstation.com/ahmedbenlakhdhar
