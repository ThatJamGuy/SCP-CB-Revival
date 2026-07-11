---------------------------------------------------
EYE ADVANCED - Realistic Eye Shader for Unity

Copyright ©2021 Tanuki Digital
Version 1.1.0
---------------------------------------------------


---------------------------------------------
A NOTE ON SRP (SCRIPTABLE RENDER PIPELINES)
---------------------------------------------
In this folder are 3 .unitypackage files for each of the default Unity SRP versions.  

1. EyeAdvanced_ver1.1.0_STANDARD.unitypackage (default)
2. EyeAdvanced_ver1.1.0_SRP_UniversalRP.unitypackage
3. EyeAdvanced_ver1.1.0_SRP_HDRP.unitypackage

When you first install EyeAdvanced you will be using the "standard" rendering version by default.  If you are using either UniversalRP, or HDRP in your project, you will need to unpack the relevant package file before the sword textures and materials will work in your project.  Note that LWRP has been renamed to UniversalRP by Unity.  If you are using LWRP in your project then you should use the UniversalRP .unitypackage above.

When installing each .unitypackage, appropriate shaders will be added to the project and EyeAdvanced materials will be REPLACED for that pipeline.  Because of this make sure you have setup your project to use your target pipeline first, before installing the relevant .unitypackage file.


---------------------------------------------
SRP COMPATIBILITY
---------------------------------------------
Due to the quickly changing nature of Unity's Scriptable Render Pipeline System, and the differences between the myriad possible Unity version and SRP version configurations, these SRP packages may not work properly for all Unity configurations.  Base compatibility requirements are listed below.  In some cases different Unity or SRP versions will also work, but in some case there may be render-breaking bugs or changes in these pipelines as well, the below versions have been tested and are supported.


STANDARD
--------
Requires Unity 5.6.7f1 or higher.


Lightweight Render Pipeline (LWRP)
----------------------------------
LWRP has been renamed by Unity, see UniversalRP below.


Universal Render Pipeline (UniversalRP)
---------------------------------------
Requires Unity 2019.4.10f1 or higher.
Tested with LightweightRP 7.3.1.


High Definition Render Pipeline (HDRP)
---------------------------------------
Requires Unity 2019.4.10f1 or higher.
Tested with High Definition RP 7.3.1.