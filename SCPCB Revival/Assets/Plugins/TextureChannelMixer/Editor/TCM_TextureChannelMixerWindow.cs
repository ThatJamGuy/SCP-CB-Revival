// Don't look at my code, it's messy :(

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;
using System.IO;

namespace TCM_TextureChannelMixer
{

    public enum EColorChannel
    {
        None,
        Red,
        Green,
        Blue,
        Alpha
    }

    public class ConnectionNodeData
    {
        public Dictionary<EColorChannel, Rect> channelRects;

        public ConnectionNodeData()
        {
            channelRects = new Dictionary<EColorChannel, Rect>();
            channelRects.Add(EColorChannel.Red, Rect.zero);
            channelRects.Add(EColorChannel.Green, Rect.zero);
            channelRects.Add(EColorChannel.Blue, Rect.zero);
            channelRects.Add(EColorChannel.Alpha, Rect.zero);
        }
    }

    [Serializable]
    public class ResultConnectionModifier
    {
        public IResultModifier modifier;
        public string type;
        public Vector4 data;

        public ResultConnectionModifier(IResultModifier modifier, Type modifierType)
        {
            this.modifier = modifier;
            type = modifierType.FullName;
            data = Vector4.zero;
        }

        public void GatherData()
        {
            modifier.GetShaderData(out int ignore, out data);
        }
    }

    [Serializable]
    public class ResultConnectionData
    {
        public EColorChannel connectedChannel;
        public int connectedReferenceIndex;
        public float defaultValue;
        public List<ResultConnectionModifier> modifiers = new List<ResultConnectionModifier>();

        public ResultConnectionData(int connectedReferenceIndex = -1, EColorChannel connectedChannel = EColorChannel.None, float defaultValue = 0)
        {
            this.connectedChannel = connectedChannel;
            this.connectedReferenceIndex = connectedReferenceIndex;
            this.defaultValue = defaultValue;
        }
    }

    [Serializable]
    public class ConfigPreset
    {
        public ResultConnectionData[] connections;
        public string[] referenceTextureNames;
        public bool autoSize;
        public int exportWidth;
        public int exportHeight;

        public ConfigPreset(ResultConnectionData[] connections, string[] referenceTextureNames, bool autoSize, int exportWidth, int exportHeight)
        {
            this.connections = connections;
            this.referenceTextureNames = referenceTextureNames;
            this.autoSize = autoSize;
            this.exportWidth = exportWidth;
            this.exportHeight = exportHeight;
        }
    }

    public class TextureChannelMixerWindow : EditorWindow
    {
        static List<Texture2D> referenceTextures = new List<Texture2D>();
        static List<List<Texture2D>> referenceBulkTextures = new List<List<Texture2D>>();
        static List<string> referenceTextureNames = new List<string>();
        static float imageSize = 128;
        static Vector2 scrollPosition;
        static bool autoUpdate = true;
        static bool autoSize = true;
        static int resultWidth = 512;
        static int resultHeight = 512;

        Event e;

        Texture2D addImageImage;
        Texture2D nodeConnectionImage;
        Texture2D nodeConnectionSelectedImage;
        Texture2D arrowUpImage;
        Texture2D arrowDownImage;
        Texture2D mouseLeftImage;
        Texture2D mouseRightImage;
        Texture2D saveIconImage;
        Texture2D loadIconImage;
        Texture2D clearIconImage;
        Texture2D transparencyImage;

        GenericMenu helpMenu;
        EColorChannel lastModifierColorChannel;
        GenericMenu modifierMenu;

        bool connectingNodes;
        bool startedConnectionFromReference;
        EColorChannel startingConnectionChannel;
        int startingConnectionReferenceIndex;
        static int referenceModeIndex;

        static int highestBulkAmount;

        static RenderTexture resultTexture;

        static Dictionary<int, ConnectionNodeData> connectionNodeDatas = new Dictionary<int, ConnectionNodeData>();
        static Dictionary<EColorChannel, ResultConnectionData> currentConnections = new Dictionary<EColorChannel, ResultConnectionData>();

        GUIContent addImageContent;
        GUIContent saveContent;
        GUIContent loadContent;
        GUIContent clearContent;
        GUIContent removePresetButton;
        GUIContent removeReferenceImageButton;
        GUIContent removeModifierButton;
        GUIContent updatePreviewButton;
        GUIContent exportButton;
        GUIContent bulkExportButton;
        GUIContent addModifierButton;
        GUIContent helpButton;

        GUIStyle centerStyle;
        GUIStyle helpButtonStyle;
        GUIStyle footerTextStyle;
        GUIStyle reorderButtonStyle;
        GUIContent[] referenceModeContents;

        Shader mixChannelShader;
        static Material mixChannelMaterial;

        List<string> presetStrings = new List<string>();
        static string exportPath;

        static string bulkPrefix;
        static int bulkLeftChop;
        static EColorChannel bulkNameChannel = EColorChannel.Red;
        static int bulkRightChop;
        static string bulkPostfix;
        static bool showBulkExportNames;

        [MenuItem("Window/Texture Channel Mixer")]
        public static void OpenWindow()
        {
            TextureChannelMixerWindow window = GetWindow<TextureChannelMixerWindow>("Texture Channel Mixer");
            window.Show();
        }

        void OnEnable()
        {
            addImageImage = Resources.Load<Texture2D>("TCM_AddImage");
            nodeConnectionImage = Resources.Load<Texture2D>("TCM_NodeConnection");
            nodeConnectionSelectedImage = Resources.Load<Texture2D>("TCM_NodeConnectionSelected");
            arrowUpImage = Resources.Load<Texture2D>("TCM_ArrowUp");
            arrowDownImage = Resources.Load<Texture2D>("TCM_ArrowDown");
            mouseLeftImage = Resources.Load<Texture2D>("TCM_MouseLeft");
            mouseRightImage = Resources.Load<Texture2D>("TCM_MouseRight");
            saveIconImage = Resources.Load<Texture2D>("TCM_SaveIcon");
            loadIconImage = Resources.Load<Texture2D>("TCM_LoadIcon");
            clearIconImage = Resources.Load<Texture2D>("TCM_ClearIcon");
            transparencyImage = Resources.Load<Texture2D>("TCM_Transparency");
            resultTexture = new RenderTexture(resultWidth, resultHeight, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(addImageImage, resultTexture);


            if (!connectionNodeDatas.ContainsKey(-1))
            {
                connectionNodeDatas.Add(-1, new ConnectionNodeData());
                connectionNodeDatas.Add(-2, new ConnectionNodeData());
                connectionNodeDatas.Add(-3, new ConnectionNodeData());
            }

            if (!currentConnections.ContainsKey(EColorChannel.Red))
            {
                currentConnections.Add(EColorChannel.Red, new ResultConnectionData(-1, EColorChannel.Red, 0));
                currentConnections.Add(EColorChannel.Green, new ResultConnectionData(-1, EColorChannel.Green, 0));
                currentConnections.Add(EColorChannel.Blue, new ResultConnectionData(-1, EColorChannel.Blue, 0));
                currentConnections.Add(EColorChannel.Alpha, new ResultConnectionData(-1, EColorChannel.Alpha, 1));
            }

            addImageContent = new GUIContent(addImageImage, "Adds an image for channel mixing");
            saveContent = new GUIContent(" Save as Preset", saveIconImage, "Saves the current setup as a preset");
            loadContent = new GUIContent(" Load external Preset", loadIconImage, "Loads an external preset to use");
            clearContent = new GUIContent(" Clear", clearIconImage, "Clears the current setup");
            removePresetButton = new GUIContent("X", "Removes this preset");
            removeReferenceImageButton = new GUIContent("X", "Removes this reference image");
            removeModifierButton = new GUIContent("X", "Removes this modifier");
            updatePreviewButton = new GUIContent("Update Preview", "Updates the preview manually");
            exportButton = new GUIContent("Export", "Exports the image using the selected settings");
            bulkExportButton = new GUIContent("Bulk Export", "Exports the images using the selected settings");
            addModifierButton = new GUIContent("Add Modifier", "Adds a modifier to adjust the input");
            helpButton = new GUIContent("Help");

            mixChannelShader = Resources.Load<Shader>("TCM_MixChannels");
            mixChannelMaterial = new Material(mixChannelShader);

            if (exportPath == "")
                exportPath = Application.dataPath;

            centerStyle = new GUIStyle();
            centerStyle.alignment = TextAnchor.MiddleCenter;
            centerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            helpButtonStyle = new GUIStyle();
            helpButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            helpButtonStyle.normal.textColor = Color.gray;
            footerTextStyle = new GUIStyle();
            footerTextStyle.alignment = TextAnchor.MiddleLeft;
            footerTextStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            reorderButtonStyle = new GUIStyle();
            reorderButtonStyle.padding = new RectOffset(2, 2, 2, 2);
            referenceModeContents = new[] { new GUIContent("Single", "Do the operation for a single image per reference"), new GUIContent("Bulk", "Do the operation for multiple images per reference") };

            helpMenu = new GenericMenu();
            helpMenu.AddItem(new GUIContent("Rate this asset"), false, () =>
            {
                System.Diagnostics.Process.Start("https://assetstore.unity.com/packages/slug/180690");
            });
            helpMenu.AddItem(new GUIContent("Send feedback or suggestions"), false, () =>
            {
                System.Diagnostics.Process.Start("mailto:help.davidlawrence@gmail.com?subject=TCM feedback/suggestion");
            });

            modifierMenu = new GenericMenu();
            Type baseType = typeof(IResultModifier);
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes().Where(t => baseType.IsAssignableFrom(t) && t != baseType);
            foreach (var index in types)
            {
                ModifierHiddenAttribute attribute = ((ModifierHiddenAttribute)index.GetCustomAttribute(typeof(ModifierHiddenAttribute)));
                if (attribute != null && attribute.hideAttribute == true) continue;
                GUIContent content = new GUIContent();
                content.text = ((ModifierNameAttribute)index.GetCustomAttribute(typeof(ModifierNameAttribute)))?.name ?? "No name";
                modifierMenu.AddItem(content, false, () =>
                {
                    IResultModifier modifier = (IResultModifier)Activator.CreateInstance(index);
                    modifier.Initialize();
                    modifier.ChannelColor = Vector4.Max(GetColorFromColorChannel(lastModifierColorChannel), Vector4.one * 0.5f);
                    currentConnections[lastModifierColorChannel].modifiers.Add(new ResultConnectionModifier(modifier, index));
                    if (autoUpdate)
                        UpdatePreview();
                });
            }

            UpdatePresetButtons();
            UpdatePreview();
        }

        void UpdatePresetButtons()
        {
            presetStrings.Clear();
            string path = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
            path = Path.Combine(path, "Presets");
            string[] files = Directory.GetFiles(path);
            foreach (var index in files)
            {
                if (index.EndsWith(".meta")) continue;
                presetStrings.Add(Path.GetFileNameWithoutExtension(index));
            }
        }

        private void OnGUI()
        {
            e = Event.current;
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.BeginVertical("Box");
                {
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(saveContent))
                        {
                            SavePreset();
                        }
                        if (GUILayout.Button(loadContent))
                        {
                            LoadExternalPreset();
                        }
                        if (GUILayout.Button(clearContent))
                        {
                            ClearAll();
                        }
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(helpButton, helpButtonStyle))
                        {
                            helpMenu.ShowAsContext();
                        }
                        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("Presets");
                        foreach (var index in presetStrings)
                        {
                            if (GUILayout.Button(index))
                            {
                                LoadPreset(index);
                            }
                            Color originalColor = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                            GUILayout.Space(-8);
                            if (GUILayout.Button(removePresetButton, GUILayout.Width(20)))
                            {
                                RemovePreset(index);
                                UpdatePresetButtons();
                                return;
                            }
                            GUI.backgroundColor = originalColor;
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(EditorGUIUtility.standardVerticalSpacing * 2);
                }
                GUILayout.EndVertical();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    referenceModeIndex = GUILayout.Toolbar(referenceModeIndex, referenceModeContents);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (referenceTextures.Count > 0)
                    {
                        GUILayout.Space(imageSize + EditorGUIUtility.standardVerticalSpacing * 2);
                    }

                    for (int i = 0; i < referenceTextures.Count; ++i)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.MaxWidth(imageSize));
                        {
                            GUILayout.Label(referenceModeIndex == 0 ? (referenceTextures[i] != null ? referenceTextures[i].width + "x" + referenceTextures[i].height : "") : "");
                            EditorGUILayout.BeginHorizontal();
                            {
                                referenceTextureNames[i] = EditorGUILayout.TextField(referenceTextureNames[i], GUILayout.MaxWidth(9999));
                                GUILayout.FlexibleSpace();
                                Color originalColor = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                                if (GUILayout.Button(removeReferenceImageButton, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                                {
                                    RemoveReferenceImage(i);
                                    return;
                                }
                                GUI.backgroundColor = originalColor;
                            }
                            EditorGUILayout.EndHorizontal();
                            if (referenceModeIndex == 0)
                                referenceTextures[i] = EditorGUILayout.ObjectField(referenceTextures[i], typeof(Texture2D), false, GUILayout.Height(imageSize), GUILayout.Width(imageSize)) as Texture2D;
                            else if(referenceModeIndex == 1)
                            {
                                GUILayout.BeginVertical("box");
                                {
                                    GUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true), GUILayout.Width(imageSize * 2.0f));
                                    {
                                        GUILayout.Label("Drop textures here");
                                        if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
                                        {
                                            referenceBulkTextures[i].Clear();
                                            UpdateHighestBulkAmount();
                                        }
                                    }
                                    GUILayout.EndHorizontal();
                                    Rect r = GUILayoutUtility.GetLastRect();
                                    Event e = Event.current;
                                    EventType eType = e.type;
                                    if (r.Contains(e.mousePosition) && (eType == EventType.DragUpdated || eType == EventType.DragPerform))
                                    {
                                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                                        if (eType == EventType.DragPerform)
                                        {
                                            DragAndDrop.AcceptDrag();
                                            foreach (var obj in DragAndDrop.objectReferences)
                                            {
                                                Texture2D tex = (Texture2D)obj;
                                                if (tex != null)
                                                    referenceBulkTextures[i].Add(tex);
                                            }
                                            UpdateHighestBulkAmount();
                                            Event.current.Use();
                                        }
                                    }
                                    for (int bulkIndex = 0; bulkIndex < referenceBulkTextures[i].Count; ++bulkIndex)
                                    {
                                        GUILayout.BeginHorizontal();
                                        {
                                            referenceBulkTextures[i][bulkIndex] = EditorGUILayout.ObjectField(referenceBulkTextures[i][bulkIndex], typeof(Texture2D), false) as Texture2D;
                                            Color originalColor = GUI.backgroundColor;
                                            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                                            if (GUILayout.Button(removeReferenceImageButton, GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                                            {
                                                RemoveBulkReferenceImage(i, bulkIndex);
                                                return;
                                            }
                                            GUI.backgroundColor = originalColor;
                                        }
                                        GUILayout.EndHorizontal();
                                    }
                                    float spaceHeight = (highestBulkAmount - referenceBulkTextures[i].Count) * (EditorGUIUtility.singleLineHeight + 2);
                                    GUILayout.Space(spaceHeight);
                                }
                                GUILayout.EndVertical();
                            }
                            DrawConnectionNodes(i, imageSize * (referenceModeIndex == 1 ? 2.0f : 1.0f) + 8);
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.BeginVertical(GUILayout.MaxWidth(imageSize));
                    {
                        GUILayout.Space(EditorGUIUtility.singleLineHeight + 1);
                        GUILayout.Label("Add Reference", centerStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                        //GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
                        Color originalColor = GUI.color;
                        GUI.color = new Color(0.75f, 0.75f, 0.75f, 1.0f);
                        if (GUILayout.Button(addImageContent, GUILayout.Height(imageSize), GUILayout.Width(imageSize)))
                        {
                            AddReferenceImage();
                        }
                        GUI.color = originalColor;
                    }
                    EditorGUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(imageSize / 2.0f);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawResultConnectionNodes();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical(GUILayout.Width(imageSize));
                    {
                        GUILayout.Space(imageSize / 4.0f);
                        GUI.enabled = false;
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        DrawConnectionNodes(-3, imageSize + 16);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUI.enabled = true;
                        if (referenceModeIndex == 0)
                        {
                            GUILayout.BeginVertical("Box");
                            {
                                Rect r = GUILayoutUtility.GetRect(imageSize, referenceModeIndex == 0 ? imageSize : 0.0f);
                                GUI.DrawTexture(r, transparencyImage, ScaleMode.ScaleToFit);
                                GUI.DrawTexture(r, resultTexture, ScaleMode.ScaleToFit);
                                if (GUILayout.Button(updatePreviewButton))
                                {
                                    UpdatePreview();
                                }
                            }
                            GUILayout.EndVertical();
                        }
                        GUILayout.BeginVertical("Box");
                        GUILayout.Label("Export Dimensions", centerStyle);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            autoSize = GUILayout.Toggle(autoSize, "Auto size");
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUI.enabled = !autoSize;
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            GUILayout.BeginHorizontal(GUILayout.Width(125));
                            {
                                resultWidth = EditorGUILayout.IntField(resultWidth);
                                GUILayout.FlexibleSpace();
                                GUILayout.Label("x");
                                GUILayout.FlexibleSpace();
                                resultHeight = EditorGUILayout.IntField(resultHeight);
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.FlexibleSpace();
                        }
                        GUILayout.EndHorizontal();
                        GUI.enabled = resultWidth > 0 && resultHeight > 0;
                        Color originalColor = GUI.backgroundColor;
                        if (referenceModeIndex == 0)
                        {
                            GUI.backgroundColor = new Color(0.5f, 1.0f, 0.5f, 1.0f);
                            if (GUILayout.Button(exportButton, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2.0f)))
                            {
                                Export();
                            }
                        }
                        else if (referenceModeIndex == 1)
                        {
                            GUILayout.Space(20);
                            GUILayout.Label("Export names", centerStyle);
                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.Label("Prefix", centerStyle, GUILayout.Width(103));
                                GUILayout.Label("Left Remove", centerStyle, GUILayout.Width(103));
                                GUILayout.Label("Name Column", centerStyle, GUILayout.Width(103));
                                GUILayout.Label("Right Remove", centerStyle, GUILayout.Width(103));
                                GUILayout.Label("Postfix", centerStyle, GUILayout.Width(100));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            {
                                bulkPrefix = GUILayout.TextField(bulkPrefix, GUILayout.Width(100));
                                bulkLeftChop = EditorGUILayout.IntField(bulkLeftChop, GUILayout.Width(100));
                                bulkNameChannel = (EColorChannel)EditorGUILayout.EnumPopup(bulkNameChannel, GUILayout.Width(100));
                                bulkRightChop = EditorGUILayout.IntField(bulkRightChop, GUILayout.Width(100));
                                bulkPostfix = GUILayout.TextField(bulkPostfix, GUILayout.Width(100));
                            }
                            GUILayout.EndHorizontal();
                            try
                            {
                                int exportAmount = GetExportAmount();
                                if (exportAmount > 0)
                                {
                                    showBulkExportNames = EditorGUILayout.Foldout(showBulkExportNames, "Output names", true);

                                    if (showBulkExportNames)
                                    {
                                        for (int i = 0; i < exportAmount; ++i)
                                        {
                                            GUILayout.Label(i + ": \t" + GetBulkExportName(i));
                                        }
                                    }
                                }
                                GUI.backgroundColor = new Color(0.5f, 1.0f, 0.5f, 1.0f);
                                if (GUILayout.Button(bulkExportButton, GUILayout.Height(EditorGUIUtility.singleLineHeight * 2.0f)))
                                {
                                    BulkExport();
                                }
                            }
                            catch (Exception)
                            {
                                // Hides occasional error
                            };
                        }
                        GUI.backgroundColor = originalColor;
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    if (autoUpdate)
                        UpdatePreview();
                }
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal("Box");
            {
                Color originalColor = GUI.color;
                GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                GUILayout.Label(mouseLeftImage, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));
                GUILayout.Space(-8);
                GUILayout.Label("+ Drag on Node: Connect nodes", footerTextStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));
                GUILayout.Space(20);
                GUILayout.Label(mouseRightImage, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));
                GUI.color = originalColor;
                GUILayout.Space(-8);
                GUILayout.Label("on Node: Remove connections from node", footerTextStyle, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Space(EditorGUIUtility.singleLineHeight * 0.333f);
                GUILayout.BeginHorizontal();
                autoUpdate = GUILayout.Toggle(autoUpdate, "Auto Update");
                GUILayout.Space(20);
                imageSize = GUILayout.HorizontalSlider(imageSize, 128, 1024, GUILayout.Width(100), GUILayout.Height(EditorGUIUtility.singleLineHeight));
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            Handles.BeginGUI();
            if (connectingNodes)
            {
                Color bezierColor = GetColorFromColorChannel(startingConnectionChannel);

                Vector2 nodeLocation = connectionNodeDatas[startingConnectionReferenceIndex].channelRects[startingConnectionChannel].center;
                Vector3 startLocation = (startedConnectionFromReference ? nodeLocation - scrollPosition : e.mousePosition);
                Vector3 endLocation = (startedConnectionFromReference ? e.mousePosition : nodeLocation - scrollPosition);
                Handles.DrawBezier(startLocation, endLocation, startLocation - Vector3.down * (endLocation.y - startLocation.y) * 0.75f, endLocation + Vector3.down * (endLocation.y - startLocation.y) * 0.75f, bezierColor, null, 3);

                if (e.type == EventType.MouseUp && e.button == 0)
                    EndConnection(EColorChannel.None);
            }

            DrawCurrentConnection(EColorChannel.Red);
            DrawCurrentConnection(EColorChannel.Green);
            DrawCurrentConnection(EColorChannel.Blue);
            DrawCurrentConnection(EColorChannel.Alpha);
            DrawResultConnection(EColorChannel.Red);
            DrawResultConnection(EColorChannel.Green);
            DrawResultConnection(EColorChannel.Blue);
            DrawResultConnection(EColorChannel.Alpha);

            Handles.EndGUI();

            Repaint();
        }

        void DrawCurrentConnection(EColorChannel colorChannel)
        {
            if (currentConnections[colorChannel].connectedReferenceIndex >= 0)
            {
                Color bezierColor = GetColorFromColorChannel(colorChannel);
                Vector2 startingNodeCenter = connectionNodeDatas[currentConnections[colorChannel].connectedReferenceIndex].channelRects[currentConnections[colorChannel].connectedChannel].center - scrollPosition;
                Vector2 endingNodeCenter = connectionNodeDatas[-1].channelRects[colorChannel].center - scrollPosition;
                Handles.DrawBezier(startingNodeCenter, endingNodeCenter, startingNodeCenter - Vector2.down * (endingNodeCenter.y - startingNodeCenter.y) * 0.75f, endingNodeCenter + Vector2.down * (endingNodeCenter.y - startingNodeCenter.y) * 0.75f, bezierColor, null, 3);
            }
        }

        void DrawResultConnection(EColorChannel colorChannel)
        {
            Color bezierColor = GetColorFromColorChannel(colorChannel);
            bezierColor.a = 0.5f;
            Vector2 startingNodeCenter = connectionNodeDatas[-2].channelRects[colorChannel].center - scrollPosition;
            Vector2 endingNodeCenter = connectionNodeDatas[-3].channelRects[colorChannel].center - scrollPosition;
            Vector2 bottomBezierOffset = colorChannel == EColorChannel.Green || colorChannel == EColorChannel.Blue ? Vector2.down * (endingNodeCenter.y - startingNodeCenter.y) * 0.75f : -Vector2.right * (endingNodeCenter.x - startingNodeCenter.x) * 0.75f;
            Handles.DrawBezier(startingNodeCenter, endingNodeCenter, startingNodeCenter - Vector2.down * (endingNodeCenter.y - startingNodeCenter.y) * 0.75f, endingNodeCenter + bottomBezierOffset, bezierColor, null, 3);
        }

        Color GetColorFromColorChannel(EColorChannel colorChannel)
        {
            switch (colorChannel)
            {
                case EColorChannel.Red:
                    return Color.red;
                case EColorChannel.Green:
                    return Color.green;
                case EColorChannel.Blue:
                    return Color.blue;
                case EColorChannel.Alpha:
                    return EditorGUIUtility.isProSkin ? Color.white : Color.black;
            }
            return Color.black;
        }

        void DrawConnectionNodes(int referenceIndex, float width)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(width));
            {
                GUI.enabled = (!connectingNodes || !startedConnectionFromReference) && referenceIndex >= -1;
                Color originalColor = GUI.color;
                GUILayout.FlexibleSpace();

                GUILayout.FlexibleSpace();
                GUI.color = Color.red;
                DrawNodeConnection(EColorChannel.Red, referenceIndex);

                GUILayout.FlexibleSpace();
                GUI.color = Color.green;
                DrawNodeConnection(EColorChannel.Green, referenceIndex);

                GUILayout.FlexibleSpace();
                GUI.color = Color.blue;
                DrawNodeConnection(EColorChannel.Blue, referenceIndex);

                GUILayout.FlexibleSpace();
                GUI.color = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                DrawNodeConnection(EColorChannel.Alpha, referenceIndex);

                GUILayout.FlexibleSpace();
                GUILayout.FlexibleSpace();
                GUI.color = originalColor;
                GUI.enabled = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawResultConnectionNodes()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(imageSize));
            {
                GUILayout.FlexibleSpace();
                DrawResultNodeConnection(EColorChannel.Red);
                DrawResultNodeConnection(EColorChannel.Green);
                DrawResultNodeConnection(EColorChannel.Blue);
                DrawResultNodeConnection(EColorChannel.Alpha);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawNodeConnection(EColorChannel colorChannel, int referenceIndex)
        {
            GUILayout.Label(nodeConnectionImage, GUILayout.MaxWidth(Mathf.Min(imageSize / 4.0f, EditorGUIUtility.singleLineHeight)), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            if (lastRect.Contains(e.mousePosition) && referenceIndex >= -1)
            {
                if (e.type == EventType.MouseDown && e.button == 0)
                    StartConnection(colorChannel, referenceIndex >= 0, referenceIndex);
                if (e.type == EventType.MouseUp && e.button == 0)
                    EndConnection(colorChannel, referenceIndex >= 0, referenceIndex);
                if (e.type == EventType.MouseDown && e.button == 1)
                    DisconnectNodes(colorChannel, referenceIndex);
                if (GUI.enabled)
                    GUI.DrawTexture(lastRect, nodeConnectionSelectedImage);
            }
            if (connectionNodeDatas.ContainsKey(referenceIndex))
                connectionNodeDatas[referenceIndex].channelRects[colorChannel] = lastRect;
        }

        void DrawResultNodeConnection(EColorChannel colorChannel)
        {
            GUILayout.BeginVertical();
            {
                Color originalColor = GUI.color;
                GUILayout.BeginVertical();
                if (currentConnections[colorChannel].connectedReferenceIndex < 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    currentConnections[colorChannel].defaultValue = EditorGUILayout.FloatField(currentConnections[colorChannel].defaultValue, GUILayout.Width(40));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Space(EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2);
                }
                GUI.enabled = !connectingNodes || startedConnectionFromReference;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUI.color = GetColorFromColorChannel(colorChannel);
                DrawNodeConnection(colorChannel, -1);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUI.color = originalColor;
                GUI.enabled = true;
                if (currentConnections[colorChannel].modifiers.Count > 0)
                {
                    Color originalBGColor = GUI.backgroundColor;
                    GUI.backgroundColor = GetColorFromColorChannel(colorChannel);
                    GUILayout.BeginVertical("Box", GUILayout.MinWidth(225));
                    GUI.backgroundColor = originalBGColor;
                    DrawResultConnectionFilters(colorChannel);
                    GUILayout.EndVertical();
                }
                if (GUILayout.Button(addModifierButton, GUILayout.MinWidth(225)))
                {
                    lastModifierColorChannel = colorChannel;
                    modifierMenu.ShowAsContext();
                }
                GUI.color = GetColorFromColorChannel(colorChannel);
                GUI.enabled = false;
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DrawNodeConnection(colorChannel, -2);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUI.color = originalColor;
                GUI.enabled = true;

            }
            GUILayout.EndVertical();
        }

        void DrawResultConnectionFilters(EColorChannel colorChannel)
        {
            for (int i = 0; i < currentConnections[colorChannel].modifiers.Count; i++)
            {
                IResultModifier index = currentConnections[colorChannel].modifiers[i].modifier;
                Color originalBGColor = GUI.backgroundColor;
                GUI.backgroundColor = EditorGUIUtility.isProSkin ? Color.black : Color.white;
                GUILayout.BeginHorizontal("Box", GUILayout.MinWidth(225));
                GUI.backgroundColor = originalBGColor;
                index.Draw();
                GUI.enabled = i > 0;
                if (GUILayout.Button(arrowUpImage, reorderButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    currentConnections[colorChannel].modifiers.Swap(i, i - 1);
                }
                GUI.enabled = i < currentConnections[colorChannel].modifiers.Count - 1;
                if (GUILayout.Button(arrowDownImage, reorderButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    currentConnections[colorChannel].modifiers.Swap(i, i + 1);
                }
                GUI.enabled = true;
                Color originalColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f, 1.0f);
                if (GUILayout.Button(removeModifierButton, GUILayout.Width(20)))
                {
                    currentConnections[colorChannel].modifiers.RemoveAt(i);
                    return;
                }
                GUI.backgroundColor = originalColor;
                GUILayout.EndHorizontal();
            }
        }

        static void AddReferenceImage(string referenceTextureName = "")
        {
            referenceTextures.Add(null);
            referenceTextureNames.Add(referenceTextureName);
            referenceBulkTextures.Add(new List<Texture2D>());
            connectionNodeDatas.Add(referenceTextures.Count - 1, new ConnectionNodeData());
            UpdateHighestBulkAmount();

            if (autoUpdate)
                UpdatePreview();
        }

        static void RemoveReferenceImage(int index)
        {
            referenceTextures.RemoveAt(index);
            referenceTextureNames.RemoveAt(index);
            referenceBulkTextures.RemoveAt(index);
            connectionNodeDatas.Remove(referenceTextures.Count);
            if (currentConnections[EColorChannel.Red].connectedReferenceIndex == index) { currentConnections[EColorChannel.Red].connectedReferenceIndex = -1; currentConnections[EColorChannel.Red].connectedChannel = EColorChannel.Red; }
            if (currentConnections[EColorChannel.Green].connectedReferenceIndex == index) { currentConnections[EColorChannel.Green].connectedReferenceIndex = -1; currentConnections[EColorChannel.Green].connectedChannel = EColorChannel.Green; }
            if (currentConnections[EColorChannel.Blue].connectedReferenceIndex == index) { currentConnections[EColorChannel.Blue].connectedReferenceIndex = -1; currentConnections[EColorChannel.Blue].connectedChannel = EColorChannel.Blue; }
            if (currentConnections[EColorChannel.Alpha].connectedReferenceIndex == index) { currentConnections[EColorChannel.Alpha].connectedReferenceIndex = -1; currentConnections[EColorChannel.Alpha].connectedChannel = EColorChannel.Alpha; }
            if (currentConnections[EColorChannel.Red].connectedReferenceIndex > index) currentConnections[EColorChannel.Red].connectedReferenceIndex--;
            if (currentConnections[EColorChannel.Green].connectedReferenceIndex > index) currentConnections[EColorChannel.Green].connectedReferenceIndex--;
            if (currentConnections[EColorChannel.Blue].connectedReferenceIndex > index) currentConnections[EColorChannel.Blue].connectedReferenceIndex--;
            if (currentConnections[EColorChannel.Alpha].connectedReferenceIndex > index) currentConnections[EColorChannel.Alpha].connectedReferenceIndex--;
            UpdateHighestBulkAmount();

            if (autoUpdate)
                UpdatePreview();
        }

        static void RemoveBulkReferenceImage(int referenceIndex, int index)
        {
            referenceBulkTextures[referenceIndex].RemoveAt(index);
            UpdateHighestBulkAmount();
        }

        static void UpdateHighestBulkAmount()
        {
            highestBulkAmount = 0;
            foreach (var referenceIndex in referenceBulkTextures)
            {
                highestBulkAmount = Mathf.Max(highestBulkAmount, referenceIndex.Count);
            }
        }

        static int GetExportAmount()
        {
            int exportAmount = 0;
            exportAmount = Mathf.Max(exportAmount, currentConnections[EColorChannel.Red].connectedReferenceIndex >= 0 ? referenceBulkTextures[currentConnections[EColorChannel.Red].connectedReferenceIndex].Count : 0);
            exportAmount = Mathf.Max(exportAmount, currentConnections[EColorChannel.Green].connectedReferenceIndex >= 0 ? referenceBulkTextures[currentConnections[EColorChannel.Green].connectedReferenceIndex].Count : 0);
            exportAmount = Mathf.Max(exportAmount, currentConnections[EColorChannel.Blue].connectedReferenceIndex >= 0 ? referenceBulkTextures[currentConnections[EColorChannel.Blue].connectedReferenceIndex].Count : 0);
            exportAmount = Mathf.Max(exportAmount, currentConnections[EColorChannel.Alpha].connectedReferenceIndex >= 0 ? referenceBulkTextures[currentConnections[EColorChannel.Alpha].connectedReferenceIndex].Count : 0);
            return exportAmount;
        }

        void StartConnection(EColorChannel channel, bool startFromReference = false, int referenceIndex = -1)
        {
            connectingNodes = true;
            startedConnectionFromReference = startFromReference;
            startingConnectionChannel = channel;
            startingConnectionReferenceIndex = referenceIndex;
        }

        void EndConnection(EColorChannel channel, bool endOnReference = false, int referenceIndex = -1)
        {
            if (!connectingNodes) return;
            connectingNodes = false;
            if (endOnReference == startedConnectionFromReference) return;
            if (channel == EColorChannel.None) return;
            if (startedConnectionFromReference)
                ConnectNodes(startingConnectionChannel, startingConnectionReferenceIndex, channel);
            else
                ConnectNodes(channel, referenceIndex, startingConnectionChannel);
        }

        void ConnectNodes(EColorChannel startingChannel, int startingReferenceIndex, EColorChannel endingChannel)
        {
            currentConnections[endingChannel].connectedReferenceIndex = startingReferenceIndex;
            currentConnections[endingChannel].connectedChannel = startingChannel;

            if (autoUpdate)
                UpdatePreview();
        }

        void DisconnectNodes(EColorChannel channel, int referenceIndex)
        {
            if (referenceIndex >= 0)
            {
                if (currentConnections[EColorChannel.Red].connectedReferenceIndex == referenceIndex && currentConnections[EColorChannel.Red].connectedChannel == channel)
                {
                    currentConnections[EColorChannel.Red].connectedReferenceIndex = -1;
                    currentConnections[EColorChannel.Red].connectedChannel = EColorChannel.Red;
                }
                if (currentConnections[EColorChannel.Green].connectedReferenceIndex == referenceIndex && currentConnections[EColorChannel.Green].connectedChannel == channel)
                {
                    currentConnections[EColorChannel.Green].connectedReferenceIndex = -1;
                    currentConnections[EColorChannel.Green].connectedChannel = EColorChannel.Green;
                }
                if (currentConnections[EColorChannel.Blue].connectedReferenceIndex == referenceIndex && currentConnections[EColorChannel.Blue].connectedChannel == channel)
                {
                    currentConnections[EColorChannel.Blue].connectedReferenceIndex = -1;
                    currentConnections[EColorChannel.Blue].connectedChannel = EColorChannel.Blue;
                }
                if (currentConnections[EColorChannel.Alpha].connectedReferenceIndex == referenceIndex && currentConnections[EColorChannel.Alpha].connectedChannel == channel)
                {
                    currentConnections[EColorChannel.Alpha].connectedReferenceIndex = -1;
                    currentConnections[EColorChannel.Alpha].connectedChannel = EColorChannel.Alpha;
                }
            }
            else
            {
                currentConnections[channel].connectedReferenceIndex = -1;
                currentConnections[channel].connectedChannel = channel;
            }

            if (autoUpdate)
                UpdatePreview();
        }

        static void UpdatePreview()
        {
            if (autoSize)
            {
                resultWidth = 0;
                resultHeight = 0;

                if (currentConnections[EColorChannel.Red].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Red].connectedReferenceIndex] != null)
                {
                    resultWidth = Mathf.Max(resultWidth, referenceTextures[currentConnections[EColorChannel.Red].connectedReferenceIndex].width);
                    resultHeight = Mathf.Max(resultHeight, referenceTextures[currentConnections[EColorChannel.Red].connectedReferenceIndex].height);
                }

                if (currentConnections[EColorChannel.Green].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Green].connectedReferenceIndex] != null)
                {
                    resultWidth = Mathf.Max(resultWidth, referenceTextures[currentConnections[EColorChannel.Green].connectedReferenceIndex].width);
                    resultHeight = Mathf.Max(resultHeight, referenceTextures[currentConnections[EColorChannel.Green].connectedReferenceIndex].height);
                }

                if (currentConnections[EColorChannel.Blue].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Blue].connectedReferenceIndex] != null)
                {
                    resultWidth = Mathf.Max(resultWidth, referenceTextures[currentConnections[EColorChannel.Blue].connectedReferenceIndex].width);
                    resultHeight = Mathf.Max(resultHeight, referenceTextures[currentConnections[EColorChannel.Blue].connectedReferenceIndex].height);
                }

                if (currentConnections[EColorChannel.Alpha].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Alpha].connectedReferenceIndex] != null)
                {
                    resultWidth = Mathf.Max(resultWidth, referenceTextures[currentConnections[EColorChannel.Alpha].connectedReferenceIndex].width);
                    resultHeight = Mathf.Max(resultHeight, referenceTextures[currentConnections[EColorChannel.Alpha].connectedReferenceIndex].height);
                }

                if (resultWidth <= 0 || resultHeight <= 0)
                {
                    resultWidth = 512;
                    resultHeight = 512;
                }
            }

            resultTexture = new RenderTexture(resultWidth, resultHeight, 0);

            SetMaterialChannelData(EColorChannel.Red);
            SetMaterialChannelData(EColorChannel.Green);
            SetMaterialChannelData(EColorChannel.Blue);
            SetMaterialChannelData(EColorChannel.Alpha);

            Vector4 isNormalMapMask = Vector4.zero;
            if (currentConnections[EColorChannel.Red].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Red].connectedReferenceIndex] != null)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(referenceTextures[currentConnections[EColorChannel.Red].connectedReferenceIndex]));
                isNormalMapMask.x = importer == null ? 0.0f : (importer.textureType == TextureImporterType.NormalMap ? 1.0f : 0.0f);
            }
            if (currentConnections[EColorChannel.Green].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Green].connectedReferenceIndex] != null)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(referenceTextures[currentConnections[EColorChannel.Green].connectedReferenceIndex]));
                isNormalMapMask.y = importer == null ? 0.0f : (importer.textureType == TextureImporterType.NormalMap ? 1.0f : 0.0f);
            }
            if (currentConnections[EColorChannel.Blue].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Blue].connectedReferenceIndex] != null)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(referenceTextures[currentConnections[EColorChannel.Blue].connectedReferenceIndex]));
                isNormalMapMask.z = importer == null ? 0.0f : (importer.textureType == TextureImporterType.NormalMap ? 1.0f : 0.0f);
            }
            if (currentConnections[EColorChannel.Alpha].connectedReferenceIndex >= 0 && referenceTextures[currentConnections[EColorChannel.Alpha].connectedReferenceIndex] != null)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(referenceTextures[currentConnections[EColorChannel.Alpha].connectedReferenceIndex]));
                isNormalMapMask.w = importer == null ? 0.0f : (importer.textureType == TextureImporterType.NormalMap ? 1.0f : 0.0f);
            }
            mixChannelMaterial.SetVector("_IsChannelNormalMap", isNormalMapMask);

            if (resultWidth <= 0 || resultHeight <= 0) return;
            Graphics.Blit(null, resultTexture, mixChannelMaterial);
        }

        static void SetMaterialChannelData(EColorChannel colorChannel)
        {
            bool isConnected = currentConnections[colorChannel].connectedReferenceIndex >= 0;

            string shaderStringPrefix = "_None";
            switch (colorChannel)
            {
                case EColorChannel.Red:
                    shaderStringPrefix = "_Red";
                    break;
                case EColorChannel.Green:
                    shaderStringPrefix = "_Green";
                    break;
                case EColorChannel.Blue:
                    shaderStringPrefix = "_Blue";
                    break;
                case EColorChannel.Alpha:
                    shaderStringPrefix = "_Alpha";
                    break;
            }
            Texture2D channelTexture = null;
            if (isConnected)
            {
                channelTexture = referenceTextures[currentConnections[colorChannel].connectedReferenceIndex];
            }
            else
            {
                Vector4 defaultColor = new Vector4(
                   currentConnections[EColorChannel.Red].defaultValue,
                   currentConnections[EColorChannel.Green].defaultValue,
                   currentConnections[EColorChannel.Blue].defaultValue,
                   currentConnections[EColorChannel.Alpha].defaultValue
                   );
                defaultColor.Scale(GetColorMaskFromColorChannel(colorChannel));
                channelTexture = new Texture2D(1, 1);
                channelTexture.SetPixel(0, 0, defaultColor);
                channelTexture.Apply();
            }

            List<float> operationList = new List<float>(64);
            List<Vector4> operationDataList = new List<Vector4>(64);
            if (currentConnections[colorChannel].modifiers.Count > 0)
            {
                int operation;
                Vector4 operationData;
                foreach (var index in currentConnections[colorChannel].modifiers)
                {
                    index.modifier.GetShaderData(out operation, out operationData);
                    if (operation != -1)
                    {
                        operationList.Add(operation);
                        operationDataList.Add(operationData);
                    }
                }
            }
            mixChannelMaterial.SetInt(shaderStringPrefix + "OperationCount", operationList.Count);
            while (operationList.Count < 64)
            {
                operationList.Add(-1.0f);
                operationDataList.Add(Vector4.one * -1.0f);
            }
            mixChannelMaterial.SetFloatArray(shaderStringPrefix + "Operations", operationList);
            mixChannelMaterial.SetVectorArray(shaderStringPrefix + "OperationsData", operationDataList);

            mixChannelMaterial.SetTexture(shaderStringPrefix + "Tex", channelTexture);
            mixChannelMaterial.SetVector(shaderStringPrefix + "Mask", isConnected ? GetColorMaskFromColorChannel(currentConnections[colorChannel].connectedChannel) : GetColorMaskFromColorChannel(colorChannel));
        }

        static Vector4 GetColorMaskFromColorChannel(EColorChannel colorChannel)
        {
            switch (colorChannel)
            {
                case EColorChannel.Red:
                    return new Vector4(1, 0, 0, 0);
                case EColorChannel.Green:
                    return new Vector4(0, 1, 0, 0);
                case EColorChannel.Blue:
                    return new Vector4(0, 0, 1, 0);
                case EColorChannel.Alpha:
                    return new Vector4(0, 0, 0, 1);
            }
            return Vector4.zero;
        }

        void Export()
        {
            UpdatePreview();
            Texture2D exportTexture = new Texture2D(resultTexture.width, resultTexture.height);
            exportTexture.ReadPixels(new Rect(0, 0, resultTexture.width, resultTexture.height), 0, 0);
            exportTexture.Apply();
            byte[] bytes = exportTexture.EncodeToPNG();
            string path = EditorUtility.SaveFilePanel("Export Texture", exportPath, "Texture", "png");
            if (path == "") return;
            exportPath = path.Substring(0, path.LastIndexOf('/'));
            System.IO.File.WriteAllBytes(path, bytes);
            if (path.Contains(Application.dataPath))
                AssetDatabase.Refresh();
        }

        void BulkExport()
        {
            List<Texture2D> prevImages = new List<Texture2D>();
            foreach (var image in referenceTextures)
            {
                prevImages.Add(image);
            }
            int exportAmount = GetExportAmount();
            exportPath = EditorUtility.OpenFolderPanel("Bulk export folder destination", exportPath, "");
            if (exportPath == "") return;
            if (!Directory.Exists(exportPath))
            {
                EditorUtility.DisplayDialog("Error", "Directory path is not valid", "OK");
                return;
            }
            for (int i = 0; i < exportAmount; ++i)
            {
                EditorUtility.DisplayProgressBar("Bulk Export Progress", $"Exporting ({i+1}/{exportAmount})", (float)(i+1)/exportAmount);
                for (int r = 0; r < referenceTextures.Count; ++r)
                {
                    referenceTextures[r] = referenceBulkTextures[r].Count > i ? referenceBulkTextures[r][i] : (referenceBulkTextures[r].Count > 0 ? referenceBulkTextures[r][0] : null);
                }
                UpdatePreview();
                Texture2D exportTexture = new Texture2D(resultTexture.width, resultTexture.height);
                exportTexture.ReadPixels(new Rect(0, 0, resultTexture.width, resultTexture.height), 0, 0);
                exportTexture.Apply();
                byte[] bytes = exportTexture.EncodeToPNG();
                string path = exportPath + "/" + GetBulkExportName(i);
                exportPath = path.Substring(0, path.LastIndexOf('/'));
                System.IO.File.WriteAllBytes(path, bytes);
            }
            EditorUtility.ClearProgressBar();
            if (exportPath.Contains(Application.dataPath))
                AssetDatabase.Refresh();
            for (int i = 0; i < referenceTextures.Count; i++)
            {
                referenceTextures[i] = prevImages[i];
            }
        }

        string GetBulkExportName(int bulkIndex)
        {
            int referenceIndex = currentConnections[bulkNameChannel].connectedReferenceIndex;
            string exportName = referenceIndex < 0 || referenceBulkTextures[referenceIndex].Count <= bulkIndex ? "NoName" + bulkIndex : referenceBulkTextures[referenceIndex][bulkIndex].name;
            if (referenceIndex >= 0 && referenceBulkTextures[referenceIndex].Count > bulkIndex)
            {
                for (int i = 0; i < bulkLeftChop; ++i)
                {
                    if (exportName.Length <= 0) break;
                    exportName = exportName.Substring(1);
                }
                for (int i = 0; i < bulkRightChop; ++i)
                {
                    if (exportName.Length <= 0) break;
                    exportName = exportName.Substring(0, exportName.Length - 1);
                }
            }
            return bulkPrefix + exportName + bulkPostfix + ".png";
        }

        void ClearAll()
        {
            while(referenceTextures.Count > 0)
            {
                RemoveReferenceImage(0);
            }
            ClearModifiers(EColorChannel.Red);
            ClearModifiers(EColorChannel.Green);
            ClearModifiers(EColorChannel.Blue);
            ClearModifiers(EColorChannel.Alpha);
            resultWidth = 512;
            resultHeight = 512;
            Repaint();
        }

        void ClearModifiers(EColorChannel colorChannel)
        {
            while(currentConnections[colorChannel].modifiers.Count > 0)
            {
                currentConnections[colorChannel].modifiers.RemoveAt(0);
            }
            currentConnections[colorChannel].connectedChannel = EColorChannel.None;
            currentConnections[colorChannel].connectedReferenceIndex = -1;
            currentConnections[colorChannel].defaultValue = colorChannel == EColorChannel.Alpha ? 1.0f : 0.0f;
        }

        void GatherConnectionData(EColorChannel colorChannel)
        {
            foreach (var index in currentConnections[colorChannel].modifiers)
            {
                index.GatherData();
            }
        }

        void SavePreset()
        {
            string path = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
            path = Path.Combine(path, "Presets");
            path = EditorUtility.SaveFilePanel("Save as preset", path, "Preset", "json");
            if (path == "") return;

            GatherConnectionData(EColorChannel.Red);
            GatherConnectionData(EColorChannel.Green);
            GatherConnectionData(EColorChannel.Blue);
            GatherConnectionData(EColorChannel.Alpha);

            ResultConnectionData[] connectionData = new ResultConnectionData[] {
                currentConnections[EColorChannel.Red],
                currentConnections[EColorChannel.Green],
                currentConnections[EColorChannel.Blue],
                currentConnections[EColorChannel.Alpha] };

            string[] tempreferenceTextureNames = new string[referenceTextures.Count];
            for (int i = 0; i < referenceTextures.Count; ++i)
            {
                tempreferenceTextureNames[i] = referenceTextureNames[i];
            }

            ConfigPreset preset = new ConfigPreset(connectionData, tempreferenceTextureNames, autoSize, resultWidth, resultHeight);
            string json = JsonUtility.ToJson(preset);
            File.WriteAllText(path, json);
            UpdatePresetButtons();
            AssetDatabase.Refresh();
        }

        void LoadPreset(string presetName)
        {
            string path = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
            path = Path.Combine(path, "Presets");
            path += "/" + presetName + ".json";
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "The selected preset does not exist", "Ok");
                return;
            }
            string json = File.ReadAllText(path);
            if (json == "")
            {
                EditorUtility.DisplayDialog("Error", "The selected preset has no content", "Ok");
                return;
            }
            ConfigPreset preset = JsonUtility.FromJson<ConfigPreset>(json);
            if (preset == null)
            {
                EditorUtility.DisplayDialog("Error", "There was an error reading the preset from the JSON file", "Ok");
            }
            ApplyPreset(preset);
        }

        void LoadExternalPreset()
        {
            string path = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
            path = Path.Combine(path, "Presets");
            path = EditorUtility.OpenFilePanel("Load external preset", path, "json");
            if (path == "") return;
            if (!File.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "This path is invalid", "Ok");
                return;
            }
            string json = File.ReadAllText(path);
            if (json == "")
            {
                EditorUtility.DisplayDialog("Error", "The selected preset has no content", "Ok");
                return;
            }
            ConfigPreset preset = JsonUtility.FromJson<ConfigPreset>(json);
            if (preset == null)
            {
                EditorUtility.DisplayDialog("Error", "This is not a valid preset", "Ok");
            }
            File.Copy(path, Path.Combine(Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName()), "Presets") + "/" + Path.GetFileName(path));
            UpdatePresetButtons();
            AssetDatabase.Refresh();
        }

        void ApplyPreset(ConfigPreset preset)
        {
            ClearAll();
            foreach(string index in preset.referenceTextureNames)
            {
                AddReferenceImage(index);
            }
            for (int i = 0; i < preset.connections.Length; i++)
            {
                if (preset.connections[i].connectedReferenceIndex > -1)
                {
                    ConnectNodes(preset.connections[i].connectedChannel, preset.connections[i].connectedReferenceIndex, (EColorChannel)(i + 1));
                }
                foreach (var index in preset.connections[i].modifiers)
                {
                    Type type = Type.GetType(index.type);
                    IResultModifier modifier = (IResultModifier)Activator.CreateInstance(type);
                    modifier.Initialize();
                    modifier.SetShaderData(index.data);
                    modifier.ChannelColor = Vector4.Max(GetColorFromColorChannel((EColorChannel)(i + 1)), Vector4.one * 0.5f);
                    currentConnections[(EColorChannel)(i + 1)].modifiers.Add(new ResultConnectionModifier(modifier, type));
                }
                currentConnections[(EColorChannel)(i + 1)].defaultValue = preset.connections[i].defaultValue;
            }
            autoSize = preset.autoSize;
            if (!autoSize)
            {
                resultWidth = preset.exportWidth;
                resultHeight = preset.exportHeight;
            }
        }

        void RemovePreset(string preset)
        {
            if (EditorUtility.DisplayDialog("Sure?", $"Are you sure you want to remove preset '{preset}'?", "Yes", "No"))
            {
                string path = Path.GetDirectoryName(new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName());
                path = Path.Combine(path, "Presets");
                path += "/" + preset + ".json";
                if (!File.Exists(path))
                {
                    EditorUtility.DisplayDialog("Error", "The selected file could not be found", "Ok");
                    return;
                }
                File.Delete(path);
                if (File.Exists(path + ".meta"))
                    File.Delete(path + ".meta");
                AssetDatabase.Refresh();
            }
        }
    }

    public static class Extensions
    {
        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            T tmp = list[indexA];
            list[indexA] = list[indexB];
            list[indexB] = tmp;
        }
    }
}