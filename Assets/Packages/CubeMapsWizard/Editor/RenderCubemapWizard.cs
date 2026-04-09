using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CubeMapsWizard
{


    public class CubemapEditorWindow : EditorWindow
    {
        [Header("Render Settings")] [SerializeField]
        private List<Transform> renderPositions = new();

        [Header("Camera Settings")] public float cameraNearClipPlane = 0.01f;

        public float cameraFarClipPlane = 1000f;
        public float cameraFieldOfView = 90f;
        public bool useHDR = true;

        [Header("Cubemap Face Settings")] public bool renderFacePositiveX = true;

        public bool renderFaceNegativeX = true;
        public bool renderFacePositiveY = true;
        public bool renderFaceNegativeY = true;
        public bool renderFacePositiveZ = true;
        public bool renderFaceNegativeZ = true;

        [Header("Cubemap Properties")] [Tooltip("Size of each cubemap face in pixels")]
        public int faceSize = 64;

        [Tooltip("Generate mipmaps for the cubemap")]
        public bool generateMipMaps = true;

        [Tooltip("Use linear color space")] public bool linear = true;

        [Tooltip("Make the cubemap readable")] public bool readable = true;

        private IntegerField faceSizeField;
        private TextField folderField;
        private Toggle generateMipMapsToggle;
        private bool isProcessing;
        private string lastUsedFolder = "Assets";
        private Toggle linearToggle;
        private ListView positionsListView;
        private Toggle readableToggle;
        private Button renderButton;
        private Label statusLabel;

        private void CreateGUI()
        {
            // Load the last used folder
            lastUsedFolder = EditorPrefs.GetString("CubemapEditor_LastFolder", "Assets");

            // Create root element
            var root = rootVisualElement;

            // Create UI programmatically
            CreateUIProgrammatically(root);

            // Set initial values
            UpdateUIValues();

            // Register value change callbacks
            RegisterValueChangeCallbacks();
        }

        [MenuItem("Tools/Cubemaps Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<CubemapEditorWindow>("Cubemaps Wizard");
            window.minSize = new Vector2(400, 600);
        }

        private void CreateUIProgrammatically(VisualElement root)
        {
            // Create a scroll view for the content
            var scrollView = new ScrollView();
            root.Add(scrollView);

            // Create main container
            var container = new VisualElement();
            container.style.paddingTop = 10;
            container.style.paddingBottom = 10;
            container.style.paddingLeft = 10;
            container.style.paddingRight = 10;
            scrollView.Add(container);

            // Title
            var titleLabel = new Label("Cubemap Wizard");
            titleLabel.style.fontSize = 20;
            titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            titleLabel.style.marginBottom = 10;
            container.Add(titleLabel);

            // Render positions section
            var positionsSection = CreateSection("Render Positions");
            container.Add(positionsSection);

            positionsListView = new ListView();
            positionsListView.bindingPath = nameof(renderPositions);
            positionsListView.makeItem = () => new Label();
            positionsListView.bindItem = (element, index) =>
            {
                var label = element as Label;
                var t = renderPositions[index];
                label.text = t != null ? t.name : "Null";
            };
            positionsListView.style.height = 100;
            positionsSection.Add(positionsListView);

            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Row;
            positionsSection.Add(buttonsContainer);

            var addButton = new Button(AddSelectedPositions);
            addButton.text = "Add Selected";
            buttonsContainer.Add(addButton);

            var clearButton = new Button(ClearPositions);
            clearButton.text = "Clear";
            buttonsContainer.Add(clearButton);

            // Output folder section
            var folderSection = CreateSection("Output Folder");
            container.Add(folderSection);

            folderField = new TextField("Folder");
            folderField.isReadOnly = true;
            folderSection.Add(folderField);

            var chooseFolderButton = new Button(ChooseFolder);
            chooseFolderButton.text = "Choose Folder";
            chooseFolderButton.style.marginTop = 5;
            folderSection.Add(chooseFolderButton);

            // Cubemap properties section
            var propertiesSection = CreateSection("Cubemap Properties");
            container.Add(propertiesSection);

            faceSizeField = new IntegerField("Face Size");
            faceSizeField.tooltip = "Size of each cubemap face in pixels (64-2048)";
            propertiesSection.Add(faceSizeField);

            generateMipMapsToggle = new Toggle("Generate Mip Maps");
            generateMipMapsToggle.tooltip = "Generate mipmaps for the cubemap";
            propertiesSection.Add(generateMipMapsToggle);

            linearToggle = new Toggle("Linear");
            linearToggle.tooltip = "Use linear color space";
            propertiesSection.Add(linearToggle);

            readableToggle = new Toggle("Readable");
            readableToggle.tooltip = "Make the cubemap readable";
            readableToggle.value = true; // Default to readable
            propertiesSection.Add(readableToggle);

            // Render settings section
            var renderSettingsSection = CreateSection("Render Settings");
            container.Add(renderSettingsSection);

            var nearClipField = new FloatField("Near Clip Plane");
            nearClipField.bindingPath = nameof(cameraNearClipPlane);
            renderSettingsSection.Add(nearClipField);

            var farClipField = new FloatField("Far Clip Plane");
            farClipField.bindingPath = nameof(cameraFarClipPlane);
            renderSettingsSection.Add(farClipField);

            var fovField = new FloatField("Field of View");
            fovField.bindingPath = nameof(cameraFieldOfView);
            renderSettingsSection.Add(fovField);

            var useHDRToggle = new Toggle("Use HDR");
            useHDRToggle.bindingPath = nameof(useHDR);
            renderSettingsSection.Add(useHDRToggle);

            // Face settings section
            var faceSettingsSection = CreateSection("Face Settings");
            container.Add(faceSettingsSection);

            var positiveXToggle = new Toggle("Positive X");
            positiveXToggle.bindingPath = nameof(renderFacePositiveX);
            faceSettingsSection.Add(positiveXToggle);

            var negativeXToggle = new Toggle("Negative X");
            negativeXToggle.bindingPath = nameof(renderFaceNegativeX);
            faceSettingsSection.Add(negativeXToggle);

            var positiveYToggle = new Toggle("Positive Y");
            positiveYToggle.bindingPath = nameof(renderFacePositiveY);
            faceSettingsSection.Add(positiveYToggle);

            var negativeYToggle = new Toggle("Negative Y");
            negativeYToggle.bindingPath = nameof(renderFaceNegativeY);
            faceSettingsSection.Add(negativeYToggle);

            var positiveZToggle = new Toggle("Positive Z");
            positiveZToggle.bindingPath = nameof(renderFacePositiveZ);
            faceSettingsSection.Add(positiveZToggle);

            var negativeZToggle = new Toggle("Negative Z");
            negativeZToggle.bindingPath = nameof(renderFaceNegativeZ);
            faceSettingsSection.Add(negativeZToggle);

            // Render button
            renderButton = new Button(StartRenderProcess);
            renderButton.text = "Create and Render Cubemaps";
            renderButton.style.marginTop = 10;
            renderButton.style.height = 30;
            renderButton.style.backgroundColor = new Color(0.2f, 0.6f, 0.8f);
            container.Add(renderButton);

            // DevSite link
            var devSiteButton = new Button(() => Application.OpenURL("https://veerdna.ru"));
            devSiteButton.text = "DevSite: veerdna.ru";
            devSiteButton.style.marginTop = 5;
            devSiteButton.style.height = 25;
            devSiteButton.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            devSiteButton.style.color = new Color(0.4f, 0.7f, 1.0f);
            devSiteButton.style.unityTextAlign = TextAnchor.MiddleCenter;
            container.Add(devSiteButton);

            // Status label
            statusLabel = new Label("Ready to render");
            statusLabel.style.marginTop = 10;
            statusLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            statusLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            container.Add(statusLabel);

            // Bind serialized object
            var so = new SerializedObject(this);
            so.Update();
            root.Bind(so);
        }

        private VisualElement CreateSection(string title)
        {
            var section = new VisualElement();
            section.style.marginTop = 10;
            section.style.marginBottom = 10;
            section.style.paddingTop = 5;
            section.style.paddingBottom = 5;
            section.style.borderLeftWidth = 3;
            section.style.borderLeftColor = new Color(0.3f, 0.3f, 0.3f);
            section.style.paddingLeft = 10;

            var sectionLabel = new Label(title);
            sectionLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionLabel.style.marginBottom = 5;
            section.Add(sectionLabel);

            return section;
        }

        private void UpdateUIValues()
        {
            if (faceSizeField != null) faceSizeField.value = faceSize;

            if (generateMipMapsToggle != null) generateMipMapsToggle.value = generateMipMaps;

            if (linearToggle != null) linearToggle.value = linear;

            if (readableToggle != null) readableToggle.value = readable;

            if (folderField != null) folderField.value = lastUsedFolder;
        }

        private void RegisterValueChangeCallbacks()
        {
            if (faceSizeField != null)
                faceSizeField.RegisterValueChangedCallback(evt => { faceSize = Mathf.Clamp(evt.newValue, 64, 2048); });

            if (generateMipMapsToggle != null)
                generateMipMapsToggle.RegisterValueChangedCallback(evt => { generateMipMaps = evt.newValue; });

            if (linearToggle != null)
                linearToggle.RegisterValueChangedCallback(evt => { linear = evt.newValue; });

            if (readableToggle != null)
                readableToggle.RegisterValueChangedCallback(evt => { readable = evt.newValue; });
        }

        private void AddSelectedPositions()
        {
            foreach (var sel in Selection.transforms)
                if (!renderPositions.Contains(sel))
                    renderPositions.Add(sel);

            positionsListView.RefreshItems();
        }

        private void ClearPositions()
        {
            renderPositions.Clear();
            positionsListView.RefreshItems();
        }

        private void ChooseFolder()
        {
            var path = EditorUtility.OpenFolderPanel("Select Output Folder", lastUsedFolder, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    lastUsedFolder = "Assets" + path.Substring(Application.dataPath.Length);
                    EditorPrefs.SetString("CubemapEditor_LastFolder", lastUsedFolder);
                    folderField.value = lastUsedFolder;
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Folder must be inside the Assets folder.", "OK");
                }
            }
        }

        private void StartRenderProcess()
        {
            if (isProcessing)
            {
                UpdateStatus("Already processing...");
                return;
            }

            if (renderPositions.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please add render positions.", "OK");
                return;
            }

            isProcessing = true;
            renderButton.SetEnabled(false);
            UpdateStatus("Creating cubemaps...");

            var count = 0;
            foreach (var pos in renderPositions)
            {
                count++;
                UpdateStatus($"Rendering {count}/{renderPositions.Count}");

                var objName = pos.gameObject.name;
                var defaultName = $"CBM_CubemapFrom_{objName}";
                var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{lastUsedFolder}/{defaultName}.asset");

                var format = useHDR ? TextureFormat.RGBAFloat : TextureFormat.RGBA32;
                var newCubemap = new Cubemap(faceSize, format, generateMipMaps);

                AssetDatabase.CreateAsset(newCubemap, assetPath);

                var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    importer.isReadable = readable;
                    importer.mipmapEnabled = generateMipMaps;
                    importer.sRGBTexture = !linear;
                    importer.textureShape = TextureImporterShape.TextureCube;
                    importer.SaveAndReimport();
                }

                RenderCubemap(newCubemap, pos.position);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UpdateStatus("Cubemap creation completed!");
            isProcessing = false;
            renderButton.SetEnabled(true);
        }

        private void RenderCubemap(Cubemap target, Vector3 position)
        {
            var go = new GameObject("CubemapCamera");
            var tempCamera = go.AddComponent<Camera>();

            tempCamera.nearClipPlane = cameraNearClipPlane;
            tempCamera.farClipPlane = cameraFarClipPlane;
            tempCamera.fieldOfView = cameraFieldOfView;
            tempCamera.backgroundColor = Color.black;
            tempCamera.clearFlags = CameraClearFlags.Skybox;
            tempCamera.useOcclusionCulling = false;
            tempCamera.allowHDR = useHDR;

            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;

            var faceMask = 0;
            if (renderFacePositiveX) faceMask |= 1 << 0;
            if (renderFaceNegativeX) faceMask |= 1 << 1;
            if (renderFacePositiveY) faceMask |= 1 << 2;
            if (renderFaceNegativeY) faceMask |= 1 << 3;
            if (renderFacePositiveZ) faceMask |= 1 << 4;
            if (renderFaceNegativeZ) faceMask |= 1 << 5;

            if (faceMask == 0) faceMask = 63; // All faces (0b111111)

            tempCamera.RenderToCubemap(target, faceMask);

            target.Apply(generateMipMaps);

            DestroyImmediate(go);

            EditorUtility.SetDirty(target);
        }

        private void UpdateStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.text = message;
                statusLabel.schedule.Execute(() =>
                {
                    if (statusLabel != null && !isProcessing) statusLabel.text = "Ready to render";
                }).ExecuteLater(3000); // Reset after 3 seconds
            }
        }
    }
}