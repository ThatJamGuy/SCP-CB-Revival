# Cubemaps Wizards - USER MANUAL

## Table of Contents
1. Introduction
2. Installation
3. Opening the Tool
4. Interface Overview
5. Step-by-Step Guide
6. Settings Explained
7. Tips & Best Practices
8. Troubleshooting
9. Support

---

## 1. INTRODUCTION

Cubemaps Wizards is a free Unity Editor tool designed to simplify the process
of creating cubemaps (360-degree environment textures) from any position in
your scene. Cubemaps are essential for:

- Realistic reflections on surfaces
- Skybox creation
- Image-Based Lighting (IBL)
- Environment mapping
- Reflection probe baking

This tool eliminates the need for manual cubemap creation, saving you
significant time and effort.

---

## 2. INSTALLATION

1. Import the package into your Unity project
2. The script will be automatically available in your Editor folder
3. No additional setup required - ready to use immediately!

**Requirements:**
- Unity 2022.3 or later
- Works with Built-in, URP, and HDRP pipelines

---

## 3. OPENING THE TOOL

**Method 1:** Menu Bar
- Click `Tools > Cubemaps Wizards` in Unity's top menu

**Method 2:** Search
- Press `Ctrl+K` (Windows) or `Cmd+K` (Mac)
- Type "Cubemaps Wizard" and press Enter

The Cubemaps Wizards window will open and can be docked anywhere in your
Unity Editor layout.

---

## 4. INTERFACE OVERVIEW

The interface is divided into clear sections:

**Render Positions**
- List of GameObjects from which cubemaps will be rendered
- Add Selected / Clear buttons for easy management

**Output Folder**
- Shows current save location
- Choose Folder button to change destination

**Cubemap Properties**
- Face Size: Resolution of each cubemap face (64-2048 pixels)
- Generate Mip Maps: Creates multiple resolution levels
- Linear: Color space selection (linear/sRGB)
- Readable: CPU access to texture data

**Render Settings**
- Near Clip Plane: Minimum render distance (default: 0.01)
- Far Clip Plane: Maximum render distance (default: 1000)
- Field of View: Camera FOV (default: 90°)
- Use HDR: High Dynamic Range rendering

**Face Settings**
- Individual toggles for each of the 6 cubemap faces
- Useful for optimization when not all faces are needed

**Actions**
- Create and Render Cubemaps button
- DevSite link (veerdna.ru)
- Status label showing current operation

---

## 5. STEP-BY-STEP GUIDE

### Basic Workflow

**Step 1: Prepare Your Scene**
- Open the scene where you want to create cubemaps
- Ensure your environment is properly lit and textured

**Step 2: Mark Render Positions**
- Select GameObjects in the scene hierarchy
- These objects' positions will be used as camera positions
- Tip: Create empty GameObjects at strategic locations

**Step 3: Open the Tool**
- Go to `Tools > Cubemaps Wizards`

**Step 4: Add Positions**
- With your GameObjects still selected, click "Add Selected"
- They will appear in the Render Positions list
- You can add more positions or clear the list as needed

**Step 5: Choose Output Location**
- Click "Choose Folder"
- Navigate to your desired Assets subfolder
- Click "Select Folder"
- The tool remembers this location for future use

**Step 6: Configure Settings (Optional)**
- Adjust Face Size based on your quality needs:
    - 64-128: Quick previews
    - 256-512: Standard quality
    - 1024-2048: High quality (larger files)
- Enable "Generate Mip Maps" for better quality (recommended)
- Choose "Linear" for physically-based rendering
- Enable "Use HDR" for high-quality reflections

**Step 7: Render**
- Click "Create and Render Cubemaps"
- Watch the status label for progress
- Cubemaps will be created with names like:
  `CBM_CubemapFrom_[ObjectName].asset`

**Step 8: Use Your Cubemaps**
- Find them in your output folder
- Drag and drop onto materials for reflections
- Assign to reflection probes
- Use as skybox textures

---

## 6. SETTINGS EXPLAINED

### Cubemap Properties

**Face Size (64-2048)**
- Determines the resolution of each cubemap face
- Higher = better quality but larger file size
- A 512px cubemap = 6 faces × 512×512 = ~1.5MB (LDR)
- Recommended: 512 for standard quality, 1024 for high quality

**Generate Mip Maps**
- Creates progressively smaller versions of the texture
- Improves visual quality at different viewing distances
- Slightly increases file size (~33%)
- Recommended: Enable (almost always)

**Linear**
- Enable: Uses linear color space (recommended for PBR)
- Disable: Uses sRGB/gamma color space (legacy)
- Should match your project's color space setting

**Readable**
- Enable: Allows CPU to read texture data
- Disable: GPU-only access (saves memory)
- Only enable if you need programmatic access to pixel data

### Render Settings

**Near Clip Plane (0.01)**
- Minimum distance from camera to render
- Too small: Z-fighting artifacts
- Too large: Near objects clipped
- Default of 0.01 works for most cases

**Far Clip Plane (1000)**
- Maximum distance from camera to render
- Should cover your entire visible scene
- Too small: Far objects missing
- Too large: Precision issues
- Adjust based on your scene size

**Field of View (90°)**
- Camera's field of view angle
- Standard cubemap uses 90° (do not change)
- Changing this creates distorted cubemaps

**Use HDR**
- Enable: RGBAFloat format (32-bit per channel)
- Disable: RGBA32 format (8-bit per channel)
- HDR provides better quality but 4× larger files
- Use HDR for: Reflections, IBL, high-quality rendering
- Use LDR for: Skyboxes, memory-constrained scenarios

### Face Settings

Each cubemap consists of 6 faces (±X, ±Y, ±Z):
- **Positive X**: Right
- **Negative X**: Left
- **Positive Y**: Up
- **Negative Y**: Down
- **Positive Z**: Forward
- **Negative Z**: Back

You can disable faces you don't need:
- Example: Interior room might not need Up/Down faces
- Saves rendering time
- Creates incomplete cubemap (use carefully)

---

## 7. TIPS & BEST PRACTICES

### Performance Tips

1. **Start Small**
    - Use 256px for testing, scale up when satisfied
    - Large cubemaps take longer to render

2. **Batch Processing**
    - Add multiple positions at once
    - Tool processes them automatically

3. **Strategic Positioning**
    - Place render positions where reflections matter most
    - Avoid redundant positions (too close together)

### Quality Tips

1. **HDR for Reflections**
    - Always use HDR for reflection probes
    - Provides more realistic lighting information

2. **Resolution Matching**
    - Match cubemap resolution to usage:
    - Reflection probes: 128-512
    - Skyboxes: 512-1024
    - Hero objects: 1024-2048 (very big files!)

3. **Mipmap Generation**
    - Keep enabled unless you have specific reasons not to
    - Improves visual quality significantly

### Workflow Tips

1. **Naming Convention**
    - Name your position objects descriptively
    - Example: "ReflectionProbe_MainHall"
    - Resulting cubemap: "CBM_CubemapFrom_ReflectionProbe_MainHall"

2. **Organization**
    - Create a dedicated "Cubemaps" folder
    - Organize by scene or area
    - Keep HDR and LDR versions separate

3. **Iteration**
    - Render low-res versions first (64-128)
    - Verify positioning and results
    - Then render final high-res versions

---

## 8. TROUBLESHOOTING

**Problem:** Cubemap is black/empty
- Solution: Check that objects are within Far Clip Plane distance
- Solution: Ensure lights are present in the scene
- Solution: Verify Use HDR matches your scene setup

**Problem:** Cubemap is too bright/dark
- Solution: Adjust exposure settings in your scene
- Solution: Use HDR format for better dynamic range
- Solution: Check post-processing settings

**Problem:** Can't select output folder
- Solution: Folder must be inside the Assets directory
- Solution: Create folder in Project window first

**Problem:** Render button disabled
- Solution: Add at least one render position
- Solution: Wait for current process to complete

**Problem:** Cubemap has seams/artifacts
- Solution: Increase face size resolution
- Solution: Ensure mipmap generation is enabled
- Solution: Check for edge-bleeding materials

**Problem:** File size too large
- Solution: Reduce face size resolution
- Solution: Use LDR instead of HDR
- Solution: Disable mipmap generation

---

## 9. SUPPORT

**Documentation:** Included with the asset

**Developer Website:** https://veerdna.ru

**Unity Asset Store:** Leave feedback and questions on the asset page, site chat or email dim@veerdna.ru

**Common Resources:**
- Unity Manual: Cubemaps
- Unity Manual: Reflection Probes
- Unity Manual: Image-Based Lighting

---

## APPENDIX: TECHNICAL SPECIFICATIONS

**File Format:** Unity Cubemap Asset (.asset)

**Supported Texture Formats:**
- HDR: RGBAFloat (32-bit per channel)
- LDR: RGBA32 (8-bit per channel)

**Resolution Range:** 64 to 2048 pixels per face

**Color Space:** Linear or sRGB (gamma)

**Compression:** Controlled by Unity's texture import settings

**Memory Usage (approximate):**
| Resolution | LDR Size | HDR Size |
|------------|----------|----------|
| 64px       | 128 KB    | 512 KB   |
| 2048px     | 128 MB    | 512 MB   |

(Sizes include mipmaps, actual size may vary based on compression)

---

**Version:** 1.0
**Last Updated:** 2025
**License:** Free to use

---

Thank you for using Cubemaps Wizards!
Visit https://veerdna.ru for more tools and resources.