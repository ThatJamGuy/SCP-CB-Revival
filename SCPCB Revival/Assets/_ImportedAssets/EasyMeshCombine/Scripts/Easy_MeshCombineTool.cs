// Easy Mesh Combiner
// Aliyeredon@gmail.com
// Originally written at Jan 2022
// Used the below link for combining algorithm (user @Bunzaga):
// https://answers.unity.com/questions/196649/combinemeshes-with-different-materials.html

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class Easy_MeshCombineTool : EditorWindow
{
    
    // Options
    public bool generateLightmapUV = true;
    public bool optimizeMeshBuffer = true;
    public bool backUpOriginalOjects = true;
    public bool addMeshCollider = false;

    // Use 32 bit mesh index for too many vertex models (not optimized for mobile)
    public bool use32BitIndex = false;
    public string combinedName = "Combined_Models";

    // internal variables
    GameObject[] selectedOjects;
    bool vertexLimitError;
    int selectedVertexCounts = 0;
    int selectedTrinaglesCount = 0;
    public MeshFilter[] mf;

    [MenuItem("Window/Easy Mesh Combine Tool")]
    static void Init()
    {
        // Display window
        Easy_MeshCombineTool window = (Easy_MeshCombineTool)EditorWindow.GetWindow(typeof(Easy_MeshCombineTool));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.Space();

        // Top label ( window title and version name)
        GUILayout.Label("Easy Mesh Combiner - Update 1.1 Jan 2022", EditorStyles.helpBox);

        EditorGUILayout.Space();

        generateLightmapUV = EditorGUILayout.Toggle("Generate Lightmap UV", generateLightmapUV);

        optimizeMeshBuffer = EditorGUILayout.Toggle("Optimize Mesh Buffer", optimizeMeshBuffer);
        
        EditorGUILayout.Space();

        backUpOriginalOjects = EditorGUILayout.Toggle("Backup Original Objects", backUpOriginalOjects);
        
        addMeshCollider = EditorGUILayout.Toggle("Add Mesh Collider", addMeshCollider);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        use32BitIndex = EditorGUILayout.Toggle("Use 32 Bit Mesh Index", use32BitIndex);
       
        EditorGUILayout.Space();
       
        combinedName = EditorGUILayout.TextField("Enter Name", combinedName);
        
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // Show error box when the selected game objects has higher than 65535 vertexs
        if (vertexLimitError)
            EditorGUILayout.HelpBox("Reached to the 16 bit vertex count limit : 65535 \n You can use 32 bit mesh index format \n 16 bit is better for mobile platforms", MessageType.Error);
        else
        {
            if (GUILayout.Button("Make Group and Combine"))
                Combine_Meshes();
        }

        // Actions when selecting game object in the scene/hierarchy
        if (Selection.activeGameObject)
        {

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            // Display vertex and triangles count
            EditorGUILayout.HelpBox("Vertex count : " + selectedVertexCounts.ToString(), MessageType.None);
            EditorGUILayout.HelpBox("Trinagles count : " + selectedTrinaglesCount.ToString(), MessageType.None);

            GameObject[] selectedObjects = Selection.gameObjects;
            List<GameObject> allObjects = new List<GameObject>();
            allObjects.Clear();

            // Read all selected game objects and its childs
            for (int a = 0;a< selectedObjects.Length;a++)
            {
                Transform[] childs = selectedObjects[a].GetComponentsInChildren<Transform>();
                for (int c = 0; c < childs.Length; c++)
                { 
                    if(childs[c].gameObject.GetComponent<MeshFilter>())
                       allObjects.Add(childs[c].gameObject);
                }
            }

            mf = new MeshFilter[allObjects.Count];

            for (int d = 0; d < allObjects.Count; d++)
            {
                if(allObjects[d].GetComponent<MeshFilter>())
                   mf[d] = allObjects[d].GetComponent<MeshFilter>();
            }

            // Display vertex counts
            int vertexCounts = 0;
            int trianglesCount = 0;

            for (int vl = 0; vl < mf.Length; vl++)
            {
                vertexCounts += mf[vl].sharedMesh.vertexCount;
                trianglesCount += mf[vl].sharedMesh.triangles.Length / 3;
            }

            if (vertexCounts > 65535 && !use32BitIndex)
                vertexLimitError = true;
            else
                vertexLimitError = false;

            // Save the vertexs and tringles count
            selectedVertexCounts = vertexCounts;
            selectedTrinaglesCount = trianglesCount;
        }

        
        
    }

    // Group game objects before combine
    void Group_Objects(Transform targetParent)
    {
        selectedOjects = Selection.gameObjects;
        for(int a = 0;a< selectedOjects.Length;a++)
        {
            selectedOjects[a].transform.parent = targetParent;

            // Disconnect prefabs before combine
            try
            {
                if (PrefabUtility.IsPartOfAnyPrefab(selectedOjects[a]))
                    PrefabUtility.UnpackPrefabInstance(selectedOjects[a], PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
            catch { }
            }
    }

    // Main mesh combine function
    void Combine_Meshes()
    {
        if (vertexLimitError)
            return;

        // Create the group parent
        GameObject target = new GameObject();
        target.name = "Back Up";
        // Group selected game objects before combine
        Group_Objects(target.transform);
        //________________________________________________________________
        // Keep the original model or destroy
        if (backUpOriginalOjects)
        { 
            GameObject keepOriginal;

            keepOriginal = Instantiate(target) as GameObject;

            keepOriginal.name = target.name;
            keepOriginal.transform.position = target.transform.position;
            keepOriginal.transform.rotation = target.transform.rotation;
            keepOriginal.transform.localScale = target.transform.localScale;
            keepOriginal.SetActive(false);                
        }
        //________________________________________________________________
        // Save initialize data
        string originalName = "default";
        originalName = target.GetComponent<Transform>().name;

        //________________________________________________________________
        #region Combine
        // This code is originalay copied from below link and modified by me (user @Bunzaga):
        // https://answers.unity.com/questions/196649/combinemeshes-with-different-materials.html
        ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();
        MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

            if (!meshRenderer ||
                !meshFilter.sharedMesh ||
                meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
            {
                continue;
            }

            for (int s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
            {
                int materialArrayIndex = Contains(materials, meshRenderer.sharedMaterials[s].name);
                if (materialArrayIndex == -1)
                {
                    materials.Add(meshRenderer.sharedMaterials[s]);
                    materialArrayIndex = materials.Count - 1;
                }
                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                combineInstance.subMeshIndex = s;
                combineInstance.mesh = meshFilter.sharedMesh;
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);
            }
        }

        // Get / Create mesh filter & renderer
        MeshFilter meshFilterCombine = target.GetComponent<MeshFilter>();
        if (meshFilterCombine == null)
        {
            meshFilterCombine = target.AddComponent<MeshFilter>();
        }
        MeshRenderer meshRendererCombine = target.GetComponent<MeshRenderer>();
        if (meshRendererCombine == null)
        {
            meshRendererCombine = target.AddComponent<MeshRenderer>();
        }

        
        // Combine by material index into per-material meshes
        // also, Create CombineInstance array for next step
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for (int m = 0; m < materials.Count; m++)
        {
            CombineInstance[] combineInstanceArray = (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            meshes[m] = new Mesh();

            //________________________________________________________________
            // use 32 bit mesh index format for when combined mesh's vertex counts is higher than 65535
            if (use32BitIndex && selectedVertexCounts > 65535)
                meshes[m].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 4 billions vertex counts limit
            else
                meshes[m].indexFormat = UnityEngine.Rendering.IndexFormat.UInt16; // 65535 vertex counts limit

            meshes[m].CombineMeshes(combineInstanceArray, true, true);

            combineInstances[m] = new CombineInstance();
            combineInstances[m].mesh = meshes[m];
            combineInstances[m].subMeshIndex = 0;
        }
                
        // Combine into one
        meshFilterCombine.sharedMesh = new Mesh();
        //________________________________________________________________
        // use 32 bit mesh index format for when combined mesh's vertex counts is higher than 65535
        if (use32BitIndex && selectedVertexCounts > 65535)
            meshFilterCombine.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // 4 billions vertex counts limit
        else
            meshFilterCombine.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16; // 65535 vertex counts limit

        meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

        // Destroy other meshes
        foreach (Mesh oldMesh in meshes)
        {
            oldMesh.Clear();
            DestroyImmediate(oldMesh);
        }

        // Assign materials
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];
        meshRendererCombine.materials = materialsArray;

        // Destroy original mesh models
        while (target.transform.childCount > 0)
        {
            foreach (Transform child in target.transform)
            {
                DestroyImmediate(child.gameObject);
            }
        }

        #endregion
        //________________________________________________________________
        // Assign the combined game object's name
        target.transform.GetComponent<MeshFilter>().sharedMesh.name = combinedName;
        target.name = combinedName;

        //________________________________________________________________
        // Optimize mesh buffer
        if (optimizeMeshBuffer)
            MeshUtility.Optimize(target.GetComponent<MeshFilter>().sharedMesh);

        // Generate lightmap uv
        if (generateLightmapUV)
            Unwrapping.GenerateSecondaryUVSet(target.transform.GetComponent<MeshFilter>().sharedMesh);

        // Add Mesh Collider
        if(addMeshCollider)
            target.AddComponent<MeshCollider>();

        //________________________________________________________________

        // Use the original position, rotation and scale fro the new combined mesh
        target.transform.position = Vector3.zero;
        target.transform.rotation = Quaternion.identity;
        target.transform.localScale = new Vector3(1,1,1);
   

    }
    // Used for materials combination
    private int Contains(ArrayList searchList, string searchName)
    {
        for (int i = 0; i < searchList.Count; i++)
        {
            if (((Material)searchList[i]).name == searchName)
            {
                return i;
            }
        }
        return -1;
    }

}
#endif