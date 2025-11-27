# Easy Tooltip v1.1 by Ahmed Benlakhdhar

A simple, zero-setup tooltip system for Unity UI, configurable from the Inspector or entirely from code.


## Quick Start

The system is designed to "just work." You can add tooltips in two ways:

1.  **From the Inspector (Recommended):*
    - Add the "TooltipTrigger" component to any UI element.
    - Fill in the Title and Content fields.

2.  **From Code:**
    - Call the static method from any script:  
      `TooltipTrigger.AddTooltip(myGameObject, "My content");`

Done. The manager is created automatically.
(See the Demo Scene and Documentation for more examples).


## Key Features

- Zero Setup Required (Manager is auto-created)
- Inspector & C# API for Full Control
- Rich Content (Title, Content, Icon)
- Custom Styles (Title & Icon Colors)
- Smart Text Wrapping with Max Width
- Outline Aware Positioning (Stays on-screen)
- Smooth Fade Animations


## Configuration

To configure global settings (Max Width, Fade Speed, etc.), edit the "TooltipManager" prefab located at:
`Assets/Easy Tooltip/Resources/TooltipManager.prefab`


## Support

For the full manual, see the Documentation folder. For support:
- LinkedIn: https://www.linkedin.com/in/ahmedbenlakhdhar
- ArtStation: https://www.artstation.com/ahmedbenlakhdhar