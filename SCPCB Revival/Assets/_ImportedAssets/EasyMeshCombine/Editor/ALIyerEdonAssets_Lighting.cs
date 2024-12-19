// ALIyerEdon@gmail.com - Writed at July 2021
// All rights reserved

using UnityEditor;
using UnityEngine;

public class ALIyerEdonAssets_Lighting : EditorWindow
{
    [MenuItem("Window/Lighting Tools")]
    public static void ShowWindow()
    {
        GetWindow<ALIyerEdonAssets_Lighting>(false, "Lighting Tools", true);
    }
    public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }

    private const int windowWidth = 610;
    private const int windowHeight = 500;
    Vector2 _scrollPosition;
    public bool dontShow;

    void OnEnable()
    {
        titleContent = new GUIContent("Lighting Tools and Assets");
        maxSize = new Vector2(windowWidth, windowHeight);
        minSize = maxSize;

        if (EditorPrefs.GetInt("dontShow_easyMeshCombineTool") == 3)
            dontShow = true;
        if (EditorPrefs.GetInt("dontShow_easyMeshCombineTool") != 3)
            dontShow = false;
                        
    }

    private void OnGUI()
    {
        
        Texture2D border = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/border.psd") as Texture2D;
        Texture2D ad1 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad1.psd") as Texture2D;
        Texture2D ad2 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad2.psd") as Texture2D;
        Texture2D ad3 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad3.psd") as Texture2D;
        Texture2D ad4 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad4.psd") as Texture2D;
        Texture2D ad5 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad5.psd") as Texture2D;
        Texture2D ad6 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad6.psd") as Texture2D;
        Texture2D ad7 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad7.psd") as Texture2D;
        Texture2D ad8 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad8.psd") as Texture2D;
        Texture2D ad9 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad9.psd") as Texture2D;
        Texture2D ad10 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad10.psd") as Texture2D;
        Texture2D ad11 = EditorGUIUtility.Load("Assets/EasyMeshCombine/Editor/Textures/UI/Ads/ad11.psd") as Texture2D;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Buy My Lighting Assets", MessageType.None);
        EditorGUILayout.Space();


        var dontShowRef = dontShow;

        dontShow = EditorGUILayout.Toggle("Don't show this page", dontShow);

        if (dontShowRef != dontShow)
        {
            if (dontShow == true)            
                EditorPrefs.SetInt("dontShow_easyMeshCombineTool", 3); // 3 == true
            if (dontShow == false)
                EditorPrefs.SetInt("dontShow_easyMeshCombineTool", 0); // 0 = false
        }

        


        EditorGUILayout.Space();

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition,
                     false,
                     false,
                     GUILayout.Width(windowWidth),
                     GUILayout.Height(windowHeight-20));        //---------Ad 1-------------------------------------------------
                                                                //  GUILayout.BeginVertical("Box");

        //_scrollPosition = EditorGUILayout.BeginScrollView(scrollViewRect, _scrollPosition, new Rect(0, 0, 2000, 2000));
       
        if (GUILayout.Button(border, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/complete-games-bundle-2022-116482");
        }

        if (GUILayout.Button(ad1, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/tutorials/truck-parking-kit-81767");
        }

        if (GUILayout.Button(ad2, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/2d-racing-game-2022-76989");
        }

        if (GUILayout.Button(ad3, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/traffic-ride-template-86956");
        }

        if (GUILayout.Button(ad4, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/off-road-truck-template-2022-76990");
        }

        if (GUILayout.Button(ad5, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/physics/real-drift-manager-78510");
        }

        if (GUILayout.Button(ad6, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/traffic-race-crash-template-2022-83204");
        }

        if (GUILayout.Button(ad7, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/car-parking-kit-2-2-71914");
        }

        if (GUILayout.Button(ad8, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/car-parking-template-3-106716");
        }

        if (GUILayout.Button(ad9, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/3d/characters/fps-game-template-2022-86364");
        }

        if (GUILayout.Button(ad10, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("https://assetstore.unity.com/packages/templates/packs/city-tower-defense-210511");
        }

        if (GUILayout.Button(ad11, "", GUILayout.Width(600), GUILayout.Height(130)))
        {
            Application.OpenURL("www");
        }
        EditorGUILayout.EndScrollView();

    }
}


[InitializeOnLoad]
public class Startup
{
    static Startup()
    {
        EditorPrefs.SetInt("showCounts_easyMeshCombineTool", EditorPrefs.GetInt("showCounts_easyMeshCombineTool") + 1);
        if (EditorPrefs.GetInt("showCounts_easyMeshCombineTool") == 10)
        { 

            EditorApplication.ExecuteMenuItem("Window/Lighting Tools");
        }        
    }
} 
