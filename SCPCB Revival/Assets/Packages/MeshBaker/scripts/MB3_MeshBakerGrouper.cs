using UnityEngine;
using System;
using System.Collections.Generic;
using DigitalOpus.MB.Core;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MB3_MeshBakerGrouper : MonoBehaviour, MB_IMeshBakerSettingsHolder
{
    public enum ClusterType
    {
        none,
        grid,
        pie,
        agglomerative,
    }

    public static readonly Color WHITE_TRANSP = new Color(1f,1f,1f,.1f);

    public MB3_MeshBakerGrouperBehaviour grouper;
    public ClusterType clusterType = ClusterType.none;

    /// <summary>
    /// Baked meshes will be added as a child of this scene object.
    /// </summary>
    public Transform parentSceneObject;
    public GrouperData data;

    // these are for getting a reasonable bounds in which to draw gizmos.
    [HideInInspector] public Bounds sourceObjectBounds = new Bounds(Vector3.zero, Vector3.one);
    
    public string prefabOptions_outputFolder = "";
    public bool prefabOptions_autoGeneratePrefabs;
    public bool prefabOptions_mergeOutputIntoSinglePrefab;

    public MB3_MeshCombinerSettings meshBakerSettingsAsset;
    public MB3_MeshCombinerSettingsData meshBakerSettings;

    public MB_IMeshBakerSettings GetMeshBakerSettings()
    {
        if (meshBakerSettingsAsset == null)
        {
            if (meshBakerSettings == null) meshBakerSettings = new MB3_MeshCombinerSettingsData();
            return meshBakerSettings;
        }
        else
        {
            return meshBakerSettingsAsset.GetMeshBakerSettings();
        }
    }

    public void GetMeshBakerSettingsAsSerializedProperty(out string propertyName, out UnityEngine.Object targetObj)
    {
        if (meshBakerSettingsAsset == null)
        {
            targetObj = this;
            propertyName = "meshBakerSettings";
        }
        else
        {
            targetObj = meshBakerSettingsAsset;
            propertyName = "data";
        }
    }


    void OnDrawGizmosSelected()
    {
        if (grouper == null)
        {
            grouper = CreateGrouper(clusterType);
        }
        grouper.DrawGizmos(sourceObjectBounds, data);
    }

    public MB3_MeshBakerGrouperBehaviour CreateGrouper(ClusterType t)
    {
        if (t == ClusterType.grid) grouper = new MB3_MeshBakerGrouperGrid();
        if (t == ClusterType.pie) grouper = new MB3_MeshBakerGrouperPie();
        if (t == ClusterType.agglomerative)
        {
            grouper = new MB3_MeshBakerGrouperCluster();
        }
        if (t == ClusterType.none) grouper = new MB3_MeshBakerGrouperNone();
        return grouper;
    }
    
    /// <summary>
    /// Removes all child MeshBakers for the current object
    /// </summary>
    public void DeleteAllChildMeshBakers()
    {
        MB3_MeshBakerCommon[] mBakers = GetComponentsInChildren<MB3_MeshBakerCommon>();
        for (int i = 0; i < mBakers.Length; i++)
        {
            MB3_MeshBakerCommon mb = mBakers[i];
            GameObject resultGameObject = mb.meshCombiner.resultSceneObject;
            MB_Utility.Destroy(resultGameObject);
            MB_Utility.Destroy(mb.gameObject);
        }
    }
    
    /// <summary>
    /// Organize a TextureBakers objects to be combined into groups and generate a MeshBaker child for each group.
    /// </summary>
    public List<MB3_MeshBakerCommon> GenerateMeshBakers()
    {
        MB3_TextureBaker tb = GetComponent<MB3_TextureBaker>();
        if (tb == null)
        {
            Debug.LogError("There must be an MB3_TextureBaker attached to this game object.");
            return new List<MB3_MeshBakerCommon>();
        }

        if (tb.GetObjectsToCombine().Count == 0)
        {
            Debug.LogError("The MB3_MeshBakerGrouper creates clusters based on the objects to combine in the MB3_TextureBaker component. There were no objects in this list.");
            return new List<MB3_MeshBakerCommon>();
        }

        //check if any of the objes that will be added to bakers already exist in child bakers
        List<GameObject> objsWeAreGrouping = tb.GetObjectsToCombine();
        MB3_MeshBakerCommon[] alreadyExistBakers = GetComponentsInChildren<MB3_MeshBakerCommon>();
        bool foundChildBakersWithObjsToCombine = false;
        for (int i = 0; i < alreadyExistBakers.Length; i++)
        {
            List<GameObject> childOjs2Combine = alreadyExistBakers[i].GetObjectsToCombine();
            for (int j = 0; j < childOjs2Combine.Count; j++)
            {
                if (childOjs2Combine[j] != null && objsWeAreGrouping.Contains(childOjs2Combine[j]))
                {
                    foundChildBakersWithObjsToCombine = true;
                    break;
                }
            }
        }

        bool proceed = true;
        if (foundChildBakersWithObjsToCombine)
        {
#if UNITY_EDITOR
                proceed = EditorUtility.DisplayDialog("Replace Previous Generated MeshBaker Objects", "Delete child MeshBaker objects?\n\n" +
                    "This grouper has child MeshBaker objects from a previous clustering. Do you want to delete these and create new ones?", "OK", "Cancel");
#else
                proceed = false;
                Debug.LogError("There are previously generated MeshBaker objects. Please use the editor to delete or replace them");
#endif
        }

        if (Application.isPlaying && prefabOptions_autoGeneratePrefabs)
        {
            Debug.LogError("Can only use Auto Generate Prefabs in the editor when the game is not playing.");
            proceed = false;
        }

        List<MB3_MeshBakerCommon> newBakers;
        if (proceed)
        {
            if (foundChildBakersWithObjsToCombine) DeleteAllChildMeshBakers();
            if (grouper == null || grouper.GetClusterType() != clusterType)
            {
                grouper = CreateGrouper(clusterType);
            }
            newBakers = grouper.DoClustering(tb, this, data);
        }
        else
        {
            newBakers = new List<MB3_MeshBakerCommon>();
        }

        if (parentSceneObject != null &&
            MB_Utility.IsSceneInstance(parentSceneObject.gameObject))
        {
            for (int i = 0; i < newBakers.Count; i++)
            {
                newBakers[i].parentSceneObject = parentSceneObject;
            }
        }

        return newBakers;
    }
}

namespace DigitalOpus.MB.Core
{
    /// all properties go here so that settings are remembered as user switches between cluster types
    [Serializable]
    public class GrouperData
    {
        public bool clusterOnLMIndex;
        public bool clusterByLODLevel;
        public Vector3 origin;

        //Normally these properties would be in the subclasses but putting them here makes writing the inspector much easier
        //for grid
        public Vector3 cellSize = new Vector3(5, 5, 5);

        //for pie
        public int pieNumSegments = 4;
        public Vector3 pieAxis = Vector3.up;
        public float ringSpacing = 100f;
        public bool combineSegmentsInInnermostRing = false;

        //for clustering
        public bool includeCellsWithOnlyOneRenderer = true;
        public MB3_AgglomerativeClustering cluster;
        public float maxDistBetweenClusters = 1f;
        public float _lastMaxDistBetweenClusters;
        public float _ObjsExtents = 10f;
        public float _minDistBetweenClusters = .001f;
        public List<MB3_AgglomerativeClustering.ClusterNode> _clustersToDraw = new List<MB3_AgglomerativeClustering.ClusterNode>();
        public float[] _radii;
    }
    
    public abstract class MB3_MeshBakerGrouperBehaviour
    {
        public abstract Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d);
        public abstract void DrawGizmos(Bounds sourceObjectBounds, GrouperData d);
        public List<MB3_MeshBakerCommon> DoClustering(MB3_TextureBaker tb, MB3_MeshBakerGrouper grouper, GrouperData d)
        {
            List<MB3_MeshBakerCommon> outBakers = new List<MB3_MeshBakerCommon>();
            if (grouper.prefabOptions_autoGeneratePrefabs || grouper.prefabOptions_mergeOutputIntoSinglePrefab)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("Cannot generate prefabs while playing. Prefabs can only be generated in the editor and not in play mode.");
                    return outBakers;
                }
            }

            //todo warn for no objects and no Texture Bake Result
            Dictionary<string, List<Renderer>> cell2objs = FilterIntoGroups(tb.GetObjectsToCombine(), d);

            if (d.clusterOnLMIndex)
            {
                Dictionary<string, List<Renderer>> cell2objsNew = new Dictionary<string, List<Renderer>>();
                foreach (string key in cell2objs.Keys)
                {
                    List<Renderer> gaws = cell2objs[key];
                    Dictionary<int, List<Renderer>> idx2objs = GroupByLightmapIndex(gaws);
                    foreach (int keyIdx in idx2objs.Keys)
                    {
                        string keyNew = key + "-LM-" + keyIdx;
                        cell2objsNew.Add(keyNew, idx2objs[keyIdx]);
                    }
                }
                cell2objs = cell2objsNew;
            }
            if (d.clusterByLODLevel)
            {
                //visit each cell
                //visit each renderer
                //check if that renderer is a child of an LOD group
                //      visit each LOD level check if this renderer is in that list.
                //      if not add it to LOD0 for that cell
                //      otherwise add it to LODX for that cell creating LODs as necessary
                Dictionary<string, List<Renderer>> cell2objsNew = new Dictionary<string, List<Renderer>>();
                foreach (string key in cell2objs.Keys)
                {
                    List<Renderer> gaws = cell2objs[key];
                    foreach (Renderer r in gaws)
                    {
                        if (r == null) continue;
                        bool foundInLOD = false;
                        LODGroup lodg = r.GetComponentInParent<LODGroup>();
                        if (lodg != null)
                        {
                            LOD[] lods = lodg.GetLODs();
                            for (int i = 0; i < lods.Length; i++)
                            {
                                LOD lod = lods[i];
                                if (Array.Find<Renderer>(lod.renderers, x => x == r) != null)
                                {
                                    foundInLOD = true;
                                    List<Renderer> rs;
                                    string newKey = String.Format("{0}_LOD{1}", key, i);
                                    if (!cell2objsNew.TryGetValue(newKey, out rs))
                                    {
                                        rs = new List<Renderer>();
                                        cell2objsNew.Add(newKey, rs);
                                    }
                                    if (!rs.Contains(r)) rs.Add(r);
                                }
                            }
                        }
                        if (!foundInLOD)
                        {
                            List<Renderer> rs;
                            string newKey = String.Format("{0}_LOD0", key);
                            if (!cell2objsNew.TryGetValue(newKey, out rs))
                            {
                                rs = new List<Renderer>();
                                cell2objsNew.Add(newKey, rs);
                            }
                            if (!rs.Contains(r)) rs.Add(r);
                        }
                    }
                }
                cell2objs = cell2objsNew;
            }

            int clustersWithOnlyOneRenderer = 0;
            foreach (string key in cell2objs.Keys)
            {
                List<Renderer> gaws = cell2objs[key];
                if (gaws.Count > 1 || grouper.data.includeCellsWithOnlyOneRenderer)
                {
                    outBakers.Add(AddMeshBaker(grouper, tb, key, gaws));
                }
                else
                {
                    clustersWithOnlyOneRenderer++;
                }
            }

            Debug.Log(String.Format("Found {0} cells with Renderers. Not creating bakers for {1} because there is only one mesh in the cell. Creating {2} bakers.", cell2objs.Count, clustersWithOnlyOneRenderer, cell2objs.Count - clustersWithOnlyOneRenderer));
            return outBakers;
        }

        Dictionary<int, List<Renderer>> GroupByLightmapIndex(List<Renderer> gaws)
        {
            Dictionary<int, List<Renderer>> idx2objs = new Dictionary<int, List<Renderer>>();
            for (int i = 0; i < gaws.Count; i++)
            {
                List<Renderer> objs = null;
                if (idx2objs.ContainsKey(gaws[i].lightmapIndex))
                {
                    objs = idx2objs[gaws[i].lightmapIndex];
                }
                else
                {
                    objs = new List<Renderer>();
                    idx2objs.Add(gaws[i].lightmapIndex, objs);
                }
                objs.Add(gaws[i]);
            }
            return idx2objs;
        }

        MB3_MeshBakerCommon AddMeshBaker(MB3_MeshBakerGrouper grouper, MB3_TextureBaker tb, string key, List<Renderer> gaws)
        {
            int numVerts = 0;
            for (int i = 0; i < gaws.Count; i++)
            {
                Mesh m = MB_Utility.GetMesh(gaws[i].gameObject);
                if (m != null)
                    numVerts += m.vertexCount;
            }

            GameObject nmb = new GameObject("MeshBaker-" + key);
            nmb.transform.position = Vector3.zero;
            MB3_MeshBakerCommon newMeshBaker;
            if (numVerts >= 65535)
            {
                newMeshBaker = nmb.AddComponent<MB3_MultiMeshBaker>();
                newMeshBaker.useObjsToMeshFromTexBaker = false;
            }
            else
            {
                newMeshBaker = nmb.AddComponent<MB3_MeshBaker>();
                newMeshBaker.useObjsToMeshFromTexBaker = false;
            }

            newMeshBaker.textureBakeResults = tb.textureBakeResults;
            newMeshBaker.transform.parent = tb.transform;
            newMeshBaker.meshCombiner.settingsHolder = grouper;
            for (int i = 0; i < gaws.Count; i++)
            {
                newMeshBaker.GetObjectsToCombine().Add(gaws[i].gameObject);
            }

            return newMeshBaker;
        }

        public virtual MB3_MeshBakerGrouper.ClusterType GetClusterType()
        {
            return MB3_MeshBakerGrouper.ClusterType.none;
        }
    }
    
    public class MB3_MeshBakerGrouperNone : MB3_MeshBakerGrouperBehaviour
    {
        public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d)
        {
            Debug.Log("Filtering into groups none");

            Dictionary<string, List<Renderer>> cell2objs = new Dictionary<string, List<Renderer>>();

            List<Renderer> rs = new List<Renderer>();
            for (int i = 0; i < selection.Count; i++)
            {
                if (selection[i] != null)
                {
                    rs.Add(selection[i].GetComponent<Renderer>());
                }
            }

            cell2objs.Add("MeshBaker", rs);
            return cell2objs;
        }

        public override void DrawGizmos(Bounds sourceObjectBounds, GrouperData d)
        {

        }

        public override MB3_MeshBakerGrouper.ClusterType GetClusterType()
        {
            return MB3_MeshBakerGrouper.ClusterType.none;
        }
    }
    
    public class MB3_MeshBakerGrouperGrid : MB3_MeshBakerGrouperBehaviour
    {
        public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d)
        {
            Dictionary<string, List<Renderer>> cell2objs = new Dictionary<string, List<Renderer>>();
            if (d.cellSize.x <= 0f || d.cellSize.y <= 0f || d.cellSize.z <= 0f)
            {
                Debug.LogError("cellSize x,y,z must all be greater than zero.");
                return cell2objs;
            }

            Debug.Log("Collecting renderers in each cell");
            foreach (GameObject t in selection)
            {
                if (t == null)
                {
                    continue;
                }

                GameObject go = t;
                Renderer mr = go.GetComponent<Renderer>();
                if (mr is MeshRenderer || mr is SkinnedMeshRenderer)
                {
                    //get the cell this gameObject is in
                    Vector3 gridVector = mr.bounds.center;
                    gridVector.x = Mathf.Floor((gridVector.x - d.origin.x) / d.cellSize.x) * d.cellSize.x;
                    gridVector.y = Mathf.Floor((gridVector.y - d.origin.y) / d.cellSize.y) * d.cellSize.y;
                    gridVector.z = Mathf.Floor((gridVector.z - d.origin.z) / d.cellSize.z) * d.cellSize.z;
                    List<Renderer> objs = null;
                    string gridVectorStr = gridVector.ToString();
                    if (cell2objs.ContainsKey(gridVectorStr))
                    {
                        objs = cell2objs[gridVectorStr];
                    }
                    else
                    {
                        objs = new List<Renderer>();
                        cell2objs.Add(gridVectorStr, objs);
                    }

                    if (!objs.Contains(mr))
                    {
                        //Debug.Log("Adding " + mr + " todo " + gridVectorStr);
                        objs.Add(mr);
                    }
                }
            }
            return cell2objs;
        }

        public override void DrawGizmos(Bounds sourceObjectBounds, GrouperData d)
        {
            Vector3 cs = d.cellSize;
            if (cs.x <= .00001f || cs.y <= .00001f || cs.z <= .00001f) return;
            Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
            Vector3 p = sourceObjectBounds.center - sourceObjectBounds.extents;
            Vector3 offset = d.origin;
            offset.x = offset.x % cs.x;
            offset.y = offset.y % cs.y;
            offset.z = offset.z % cs.z;
            //snap p to closest cell center
            Vector3 start;
            p.x = Mathf.Round((p.x) / cs.x) * cs.x + offset.x;
            p.y = Mathf.Round((p.y) / cs.y) * cs.y + offset.y;
            p.z = Mathf.Round((p.z) / cs.z) * cs.z + offset.z;
            if (p.x > sourceObjectBounds.center.x - sourceObjectBounds.extents.x) p.x = p.x - cs.x;
            if (p.y > sourceObjectBounds.center.y - sourceObjectBounds.extents.y) p.y = p.y - cs.y;
            if (p.z > sourceObjectBounds.center.z - sourceObjectBounds.extents.z) p.z = p.z - cs.z;
            start = p;
            int numcells = Mathf.CeilToInt(sourceObjectBounds.size.x / cs.x + sourceObjectBounds.size.y / cs.y + sourceObjectBounds.size.z / cs.z);
            if (numcells > 200)
            {
                Gizmos.DrawWireCube(d.origin + cs / 2f, cs);
            }
            else
            {
                for (; p.x < sourceObjectBounds.center.x + sourceObjectBounds.extents.x; p.x += cs.x)
                {
                    p.y = start.y;
                    for (; p.y < sourceObjectBounds.center.y + sourceObjectBounds.extents.y; p.y += cs.y)
                    {
                        p.z = start.z;
                        for (; p.z < sourceObjectBounds.center.z + sourceObjectBounds.extents.z; p.z += cs.z)
                        {
                            Gizmos.DrawWireCube(p + cs / 2f, cs);
                        }
                    }
                }
            }
        }

        public override MB3_MeshBakerGrouper.ClusterType GetClusterType()
        {
            return MB3_MeshBakerGrouper.ClusterType.grid;
        }
    }
    
    public class MB3_MeshBakerGrouperPie : MB3_MeshBakerGrouperBehaviour
    {
        public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d)
        {
            Dictionary<string, List<Renderer>> cell2objs = new Dictionary<string, List<Renderer>>();
            if (d.pieNumSegments == 0)
            {
                Debug.LogError("pieNumSegments must be greater than zero.");
                return cell2objs;
            }

            if (d.pieAxis.magnitude <= .000001f)
            {
                Debug.LogError("Pie axis vector is too short.");
                return cell2objs;
            }

            if (d.ringSpacing <= .000001f)
            {
                Debug.LogError("Ring spacing is too small.");
                return cell2objs;
            }

            d.pieAxis.Normalize();
            Quaternion pieAxis2yIsUp = Quaternion.FromToRotation(d.pieAxis, Vector3.up);

            Debug.Log("Collecting renderers in each cell");
            foreach (GameObject t in selection)
            {
                if (t == null)
                {
                    continue;
                }

                GameObject go = t;
                Renderer mr = go.GetComponent<Renderer>();
                if (mr is MeshRenderer || mr is SkinnedMeshRenderer)
                {
                    //get the cell this gameObject is in
                    Vector3 origin2obj = mr.bounds.center - d.origin;
                    origin2obj = pieAxis2yIsUp * origin2obj;
                    Vector2 origin2Obj2D = new Vector2(origin2obj.x, origin2obj.z);
                    float radius = origin2Obj2D.magnitude;
                    origin2obj.Normalize();

                    float deg_aboutY = 0f;
                    if (Mathf.Abs(origin2obj.x) < 10e-5f && Mathf.Abs(origin2obj.z) < 10e-5f)
                    {
                        deg_aboutY = 0f;
                    }
                    else
                    {
                        deg_aboutY = Mathf.Atan2(origin2obj.x, origin2obj.z) * Mathf.Rad2Deg;
                        if (deg_aboutY < 0f) deg_aboutY = 360f + deg_aboutY;
                    }

                    //	Debug.Log ("Obj " + mr + " angle " + d_aboutY);
                    int segment = Mathf.FloorToInt(deg_aboutY / 360f * d.pieNumSegments);
                    int ring = Mathf.FloorToInt(radius / d.ringSpacing);
                    if (ring == 0 && d.combineSegmentsInInnermostRing)
                    {
                        segment = 0;
                    }

                    List<Renderer> objs = null;
                    string segStr = "seg_" + segment + "_ring_" + ring;
                    if (cell2objs.ContainsKey(segStr))
                    {
                        objs = cell2objs[segStr];
                    }
                    else
                    {
                        objs = new List<Renderer>();
                        cell2objs.Add(segStr, objs);
                    }

                    if (!objs.Contains(mr))
                    {
                        objs.Add(mr);
                    }
                }
            }

            return cell2objs;
        }

        public override void DrawGizmos(Bounds sourceObjectBounds, GrouperData d)
        {
            
            if (d.pieAxis.magnitude < .1f) return;
            if (d.pieNumSegments < 1) return;

            Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
            float rad = sourceObjectBounds.extents.magnitude;

            int numRings = Mathf.CeilToInt(rad / d.ringSpacing);
            numRings = Mathf.Max(1, numRings);
            for (int i = 0; i < numRings; i++)
            {
                DrawCircle(d.pieAxis.normalized, d.origin, d.ringSpacing * (i + 1), 24);
            }
            
            Quaternion yIsUp2PieAxis = Quaternion.FromToRotation(Vector3.up, d.pieAxis);
            Quaternion rStep = Quaternion.AngleAxis(180f / d.pieNumSegments, Vector3.up);
            Vector3 r = Vector3.forward;
            for (int i = 0; i < d.pieNumSegments; i++)
            {
                Vector3 rr = yIsUp2PieAxis * r;
                Vector3 origin = d.origin;
                int nr = numRings;
                if (d.combineSegmentsInInnermostRing)
                {
                    origin = d.origin + rr.normalized * d.ringSpacing;
                    nr = numRings - 1;
                }

                if (nr == 0) break;

                Gizmos.DrawLine(origin, origin + nr * d.ringSpacing * rr.normalized);
                r = rStep * r;
                r = rStep * r;
            }
        }

        static int MaxIndexInVector3(Vector3 v)
        {
            int idx = 0;
            float val = v.x;
            if (v.y > val)
            {
                idx = 1;
                val = v.y;
            }
            if (v.z > val)
            {
                idx = 2;
                val = v.z;
            }
            return idx;
        }

        public static void DrawCircle(Vector3 axis, Vector3 center, float radius, int subdiv)
        {
            Quaternion q = Quaternion.AngleAxis(360 / subdiv, axis);
            int maxIdx = MaxIndexInVector3(axis);
            int otherIdx = maxIdx == 0 ? maxIdx + 1 : maxIdx - 1;
            Vector3 r = axis; //r construct a vector perpendicular to axis
            float temp = r[maxIdx];
            r[maxIdx] = r[otherIdx];
            r[otherIdx] = -temp;
            r = Vector3.ProjectOnPlane(r, axis);
            r.Normalize();
            r *= radius;
            for (int i = 0; i < subdiv + 1; i++)
            {
                Vector3 r2 = q * r;
                Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
                Gizmos.DrawLine(center + r, center + r2);
                r = r2;
            }
        }
        public override MB3_MeshBakerGrouper.ClusterType GetClusterType()
        {
            return MB3_MeshBakerGrouper.ClusterType.pie;
        }
    }
    
    public class MB3_MeshBakerGrouperCluster : MB3_MeshBakerGrouperBehaviour
    {

        public override Dictionary<string, List<Renderer>> FilterIntoGroups(List<GameObject> selection, GrouperData d)
        {
            Dictionary<string, List<Renderer>> cell2objs = new Dictionary<string, List<Renderer>>();
            for (int i = 0; i < d._clustersToDraw.Count; i++)
            {
                MB3_AgglomerativeClustering.ClusterNode node = d._clustersToDraw[i];
                List<Renderer> rrs = new List<Renderer>();
                for (int j = 0; j < node.leafs.Length; j++)
                {
                    Renderer r = d.cluster.clusters[node.leafs[j]].leaf.go.GetComponent<Renderer>();
                    if (r is MeshRenderer || r is SkinnedMeshRenderer)
                    {
                        rrs.Add(r);
                    }
                }
                cell2objs.Add("Cluster_" + i, rrs);
            }
            return cell2objs;
        }

        public void BuildClusters(List<GameObject> gos, ProgressUpdateCancelableDelegate progFunc, GrouperData d)
        {
            if (gos.Count == 0)
            {
                Debug.LogWarning("No objects to cluster. Add some objects to the list of Objects To Combine.");
                return;
            }
            if (d.cluster == null) { d.cluster = new MB3_AgglomerativeClustering(); }
            List<MB3_AgglomerativeClustering.item_s> its = new List<MB3_AgglomerativeClustering.item_s>();
            for (int i = 0; i < gos.Count; i++)
            {
                if (gos[i] != null && its.Find(x => x.go == gos[i]) == null)
                {
                    Renderer mr = gos[i].GetComponent<Renderer>();
                    if (mr != null && (mr is MeshRenderer || mr is SkinnedMeshRenderer))
                    {
                        MB3_AgglomerativeClustering.item_s ii = new MB3_AgglomerativeClustering.item_s();
                        ii.go = gos[i];
                        ii.coord = mr.bounds.center;
                        its.Add(ii);
                    }
                }
            }
            d.cluster.items = its;
            //yield return cluster.agglomerate();
            d.cluster.agglomerate(progFunc);
            if (!d.cluster.wasCanceled)
            {
                float smallest, largest;
                _BuildListOfClustersToDraw(progFunc, out smallest, out largest, d);
                d.maxDistBetweenClusters = Mathf.Lerp(smallest, largest, .9f);
            }
        }

        public void _BuildListOfClustersToDraw(ProgressUpdateCancelableDelegate progFunc, out float smallest, out float largest, GrouperData d)
        {
            if (d._clustersToDraw == null)
            {
                d._clustersToDraw = new List<MB3_AgglomerativeClustering.ClusterNode>();
            }
            d._clustersToDraw.Clear();
            if (d.cluster.clusters == null || d.cluster.clusters.Length == 0)
            {
                smallest = 1f;
                largest = 10f;
            }
            else
            {
                if (progFunc != null) progFunc("Building Clusters To Draw A:", 0);
                largest = 1f;
                smallest = 10e6f;
                for (int i = 0; i < d.cluster.clusters.Length; i++)
                {
                    MB3_AgglomerativeClustering.ClusterNode node = d.cluster.clusters[i];
                    //don't draw clusters that were merged too far apart and only want leaf nodes
                    if (node.distToMergedCentroid <= d.maxDistBetweenClusters /*&& node.leaf == null*/)
                    {
                        if (d.includeCellsWithOnlyOneRenderer)
                        {
                            d._clustersToDraw.Add(node);
                        }
                        else if (node.leaf == null)
                        {
                            d._clustersToDraw.Add(node);
                        }
                    }
                    if (node.distToMergedCentroid > largest)
                    {
                        largest = node.distToMergedCentroid;
                    }
                    if (node.height > 0 && node.distToMergedCentroid < smallest)
                    {
                        smallest = node.distToMergedCentroid;
                    }
                }
            }
            if (progFunc != null) progFunc("Building Clusters To Draw B:", 0);
            {
                List<MB3_AgglomerativeClustering.ClusterNode> removeMe = new List<MB3_AgglomerativeClustering.ClusterNode>();
                for (int i = 0; i < d._clustersToDraw.Count; i++)
                {
                    removeMe.Add(d._clustersToDraw[i].cha);
                    removeMe.Add(d._clustersToDraw[i].chb);
                }

                for (int i = 0; i < removeMe.Count; i++)
                {
                    d._clustersToDraw.Remove(removeMe[i]);
                }
            }
            
            d._radii = new float[d._clustersToDraw.Count];
            if (progFunc != null) progFunc("Building Clusters To Draw C:", 0);
            for (int i = 0; i < d._radii.Length; i++)
            {
                MB3_AgglomerativeClustering.ClusterNode n = d._clustersToDraw[i];
                Bounds b = new Bounds(n.centroid, Vector3.one);
                for (int j = 0; j < n.leafs.Length; j++)
                {
                    Renderer r = d.cluster.clusters[n.leafs[j]].leaf.go.GetComponent<Renderer>();
                    if (r != null)
                    {
                        b.Encapsulate(r.bounds);
                    }
                }
                d._radii[i] = b.extents.magnitude;
            }
            if (progFunc != null) progFunc("Building Clusters To Draw D:", 0);
            if (smallest >= largest)
            {
                Debug.LogError("The smallest distance between clusters is greater than the largest distance between clusters. This should not happen.");
                smallest = 10e-6f;
                if (largest < 10f)
                {
                    largest = 10f;
                }
            }
            d._ObjsExtents = largest + 1f;
            d._minDistBetweenClusters = 0.1f * smallest;

            if (d._ObjsExtents < 2f) d._ObjsExtents = 2f;
        }

        public override void DrawGizmos(Bounds sceneObjectBounds, GrouperData d)
        {
            if (d.cluster == null || d.cluster.clusters == null)
            {
                return;
            }

            Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
            for (int i = 0; i < d._clustersToDraw.Count; i++)
            {
                Gizmos.color = MB3_MeshBakerGrouper.WHITE_TRANSP;
                MB3_AgglomerativeClustering.ClusterNode node = d._clustersToDraw[i];
                Gizmos.DrawWireSphere(node.centroid, d._radii[i]);
            }
        }

        public override MB3_MeshBakerGrouper.ClusterType GetClusterType()
        {
            return MB3_MeshBakerGrouper.ClusterType.agglomerative;
        }
    }
}

