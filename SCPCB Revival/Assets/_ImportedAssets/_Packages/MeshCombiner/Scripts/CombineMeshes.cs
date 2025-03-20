using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LylekGames.Tools
{
	[RequireComponent(typeof(MeshFilter))]
	[RequireComponent(typeof(MeshRenderer))]
	public class CombineMeshes : MonoBehaviour
    {
        public string path;

        public static string staticPath = "/MeshCombiner/";

		private Matrix4x4 myMatrix;

        [HideInInspector]
        public MeshFilter myMeshFilter;

        [HideInInspector]
        public MeshCollider myMeshCollider;

        private MeshRenderer myMeshRenderer;

        [HideInInspector]
        [SerializeField]
        private MeshFilter[] meshFilters;

        private MeshRenderer[] meshRenderers;

        private BoxCollider[] colliders;

        private MeshCollider[] meshColliders;

        [HideInInspector]
        public bool saved = false;

        public void Start()
        {
            myMeshFilter = GetComponent<MeshFilter>();

            myMeshRenderer = GetComponent<MeshRenderer>();

            if (path == "" || path == null) { path = staticPath; }
        }
        public void CombineMultiMaterialMesh()
        {
            #region FAIL SAFE
            //MAKE SURE THE MESH FILTER WE ARE USING TO STORE OUR FINAL COMBINE DOES NOT ALREADY CONTAIN MESH DATA
            if (myMeshFilter != null && myMeshFilter.sharedMesh != null)
            {
                Debug.LogError("This object already contains mesh data. (This may occur when adding the Combine script to an existing mesh. Please take a quick look at the Readme to find out how to use the combine script as intended).");
                Debug.Log("Combine script removed.");
                DestroyImmediate(this);

                return;
            }
            #endregion

            meshRenderers = GetComponentsInChildren<MeshRenderer>();

            meshFilters = GetComponentsInChildren<MeshFilter>();

            colliders = GetComponentsInChildren<BoxCollider>();

            meshColliders = GetComponentsInChildren<MeshCollider>();

            List<Material> materials = new List<Material>();

            Dictionary<Material, List<CombineInstance>> combineMaterials = new Dictionary<Material, List<CombineInstance>>();

            //FOR EACH MESH-FILTER
            for (int i = 0; i < meshFilters.Length; i++)
            {
                MeshFilter filter = meshFilters[i];

                if (filter.sharedMesh != null)
                {
                    MeshRenderer renderer = filter.GetComponent<MeshRenderer>();

                    if (renderer != null)
                    {
                        if (renderer.enabled)
                        {
                            //FOR EACH SUB-MESH
                            for (int j = 0; j < filter.sharedMesh.subMeshCount; j++)
                            {
                                if (renderer.sharedMaterials.Length >= j)
                                {
                                    //GET MATERIAL REFERENCE
                                    Material mat = renderer.sharedMaterials[j];

                                    //GET MESH COMBINE INSTANCE
                                    CombineInstance combine = new CombineInstance();
                                    combine.mesh = filter.sharedMesh;
                                    combine.subMeshIndex = j;
                                    combine.transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;

                                    //GROUP OUR COMBINE INSTANCES ACORDING TO MATERIAL
                                    if (combineMaterials.ContainsKey(mat))
                                    {
                                        combineMaterials[mat].Add(combine);
                                    }
                                    else
                                    {
                                        List<CombineInstance> instances = new List<CombineInstance>();
                                        instances.Add(combine);

                                        combineMaterials.Add(mat, instances);

                                        materials.Add(mat);
                                    }
                                }
                            }
                        }
                    }
                }

                if (meshFilters[i] != myMeshFilter)
                {
                    meshFilters[i].gameObject.SetActive(false);
                }
            }

            //COMBINE OUR MATERIAL-GROUPED MESHES AND GET A LAST COMBINE-INSTANCE OF THE COMBINED GROUP
            //EACH COMBINE-INSTANCE HERE REPRESENTS A SUBMESH
            CombineInstance[] combineInstances = new CombineInstance[combineMaterials.Count];
            myMeshFilter.sharedMesh = new Mesh();
            for(int m = 0; m < materials.Count; m++)
            {
                Material mat = materials[m];

                Mesh mesh = new Mesh();
                mesh.CombineMeshes(combineMaterials[mat].ToArray(), true);

                CombineInstance ci = new CombineInstance();
                ci.mesh = mesh;
                ci.subMeshIndex = 0;
                ci.transform = Matrix4x4.identity;
                combineInstances[m] = ci;
            }

            //FINAL COMBINE, ASSIGN MATERIALS ARRAY
            myMeshFilter.sharedMesh.CombineMeshes(combineInstances, false);
            myMeshRenderer.sharedMaterials = materials.ToArray();

            //UPDATE GAMEOBJECT LAYER, AND SET STATIC
            string objectLayer = LayerMask.LayerToName(transform.GetChild(0).gameObject.layer);
            if (gameObject.layer == LayerMask.NameToLayer("Default") && objectLayer != "Default") { gameObject.layer = LayerMask.NameToLayer(objectLayer); }

            gameObject.isStatic = true;
        }
        public void CreateMeshCollider()
        {
            if (colliders != null && meshColliders != null)
            {
                if (colliders.Length == 0 && meshColliders.Length == 0)
                {
                    colliders = GetComponentsInChildren<BoxCollider>();

                    meshColliders = GetComponentsInChildren<MeshCollider>();
                }

                if (colliders.Length == 0 && meshColliders.Length == 0) { Debug.Log("No Collision Data found."); return; }
            }
            else { Debug.Log("No Collision Data found."); return; }

            CombineInstance[] combineInstances = new CombineInstance[colliders.Length + meshColliders.Length];
            int combineIndex = 0;
            for(int i = 0; i < meshColliders.Length; i++)
            {
                MeshCollider collider = meshColliders[i];

                if (collider.convex)
                {
                    Mesh newMesh = new Mesh();
                    newMesh.vertices = collider.sharedMesh.vertices;
                    newMesh.triangles = collider.sharedMesh.triangles;
                    //NEED TO GET CONVEX MESH... ???
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = newMesh;
                    ci.subMeshIndex = 0;
                    ci.transform = transform.worldToLocalMatrix * collider.transform.localToWorldMatrix;
                    combineInstances[combineIndex] = ci;
                    combineIndex++;
                }
                else
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = collider.sharedMesh;
                    ci.subMeshIndex = 0;
                    ci.transform = transform.worldToLocalMatrix * collider.transform.localToWorldMatrix;
                    combineInstances[combineIndex] = ci;
                    combineIndex++;
                }
            }
            for (int i = 0; i < colliders.Length; i++)
            {
                Mesh newMesh = new Mesh();

                BoxCollider collider = colliders[i];

                Vector3 center = collider.center;
                Vector3 size = collider.size * 0.5f;

                Vector3[] vertices = new Vector3[8];
                vertices[0] = new Vector3(-size.x, -size.y, size.z) + center;
                vertices[1] = new Vector3(size.x, -size.y, size.z) + center;
                vertices[2] = new Vector3(-size.x, -size.y, -size.z) + center;
                vertices[3] = new Vector3(size.x, -size.y, -size.z) + center;
                vertices[4] = new Vector3(-size.x, size.y, size.z) + center;
                vertices[5] = new Vector3(size.x, size.y, size.z) + center;
                vertices[6] = new Vector3(-size.x, size.y, -size.z) + center;
                vertices[7] = new Vector3(size.x, size.y, -size.z) + center;

                for (int k = 0; k < 8; k++)
                {
                    vertices[k] = collider.transform.TransformPoint(vertices[k]);
                }

                newMesh.vertices = vertices;
                newMesh.triangles = new int[] { 2, 1, 0, 1, 2, 3, 4, 0, 1, 5, 4, 1, 0, 4, 6, 0, 6, 2, 2, 6, 3, 3, 6, 7, 7, 5, 1, 7, 1, 3, 4, 5, 6, 5, 7, 6 };

                CombineInstance ci = new CombineInstance();
                ci.mesh = newMesh;
                ci.subMeshIndex = 0;
                ci.transform = transform.worldToLocalMatrix;
                combineInstances[combineIndex] = ci;
                combineIndex++;
            }

            if (combineInstances.Length > 0)
            {
                Mesh mesh = new Mesh();
                mesh.CombineMeshes(combineInstances);

                myMeshCollider = gameObject.AddComponent<MeshCollider>();
                myMeshCollider.sharedMesh = mesh;
            }
        }
		public void DisableMesh()
        {
            if (meshFilters != null && meshFilters.Length > 0)
            {
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    meshFilters[i].gameObject.SetActive(true);
                }

                myMeshFilter.mesh = null;

                myMeshRenderer.materials = new Material[0];

                if (GetComponent<Collider>())
                {
                    DestroyImmediate(gameObject.GetComponent<Collider>());
                }
            }
            else
            {
                Debug.LogError("There is no mesh data to disable. (This may occur when adding the Combine script to an existing mesh. Please take a quick look at the Readme to find out how to use the combine script as intended.)");
                Debug.Log("Combine script removed.");
                DestroyImmediate(this);
                return;
            }
        }
	}
}
