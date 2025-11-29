//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using DigitalOpus.MB.Core;

/// <summary>
/// Component that is an endless mesh. You don't need to worry about the 65k limit when adding meshes. It is like a List of combined meshes. Internally it manages
/// a collection of CombinedMeshes that are added and deleted as necessary. 
/// 
/// Note that this implementation does
/// not attempt to split meshes. Each mesh is added to one of the internal meshes as an atomic unit.
/// 
/// This class is a Component. It must be added to a GameObject to use it. It is a wrapper for MB2_Multi_meshCombiner which contains the same functionality but is not a component
/// so it can be instantiated like a normal class.
/// </summary>
public class MB3_MultiMeshBaker : MB3_MeshBakerCommon {
        
    [SerializeField] protected MB3_MultiMeshCombiner _meshCombiner = new MB3_MultiMeshCombiner();

    // [ContextMenu("Print Timings")]
    public void PrintTimings()
    {
        double db_showHideGameObjects = 0;
        double db_addDeleteGameObjects = 0;

        double db_addDeleteGameObjects_InitFromMeshCombiner = 0;
        double db_addDeleteGameObjects_Init = 0;
        double db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers = 0;
        double db_addDeleteGameObjects_CopyFromDGOMeshToBuffers = 0;
        double db_addDeleteGameObjects_CollectMeshData = 0;

        double db_apply = 0;
        double db_applyShowHide = 0;
        double db_updateGameObjects = 0;
        
        for (int i = 0; i < _meshCombiner.meshCombiners.Count; i++)
        {
            MB3_MeshCombinerSingle c = _meshCombiner.meshCombiners[i].combinedMesh;

            db_showHideGameObjects += c.db_showHideGameObjects.Elapsed.TotalSeconds;
            db_addDeleteGameObjects += c.db_addDeleteGameObjects.Elapsed.TotalSeconds;

            db_addDeleteGameObjects_CollectMeshData += c.db_addDeleteGameObjects_CollectMeshData.Elapsed.TotalSeconds;
            db_addDeleteGameObjects_InitFromMeshCombiner += c.db_addDeleteGameObjects_InitFromMeshCombiner.Elapsed.TotalSeconds;
            db_addDeleteGameObjects_Init += c.db_addDeleteGameObjects_Init.Elapsed.TotalSeconds;
            db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers += c.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Elapsed.TotalSeconds;
            db_addDeleteGameObjects_CopyFromDGOMeshToBuffers += c.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Elapsed.TotalSeconds;

            db_apply += c.db_apply.Elapsed.TotalSeconds;
            db_applyShowHide += c.db_applyShowHide.Elapsed.TotalSeconds;
            db_updateGameObjects += c.db_updateGameObjects.Elapsed.TotalSeconds;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("Timings  " + (_meshCombiner.settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI ? "  newMeshAPI " : " oldMeshAPI"));
        sb.AppendLine("db_showHideGameObjects\t" + db_showHideGameObjects);
        sb.AppendLine("db_addDeleteGameObjects\t" + db_addDeleteGameObjects);

        sb.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshData\t" + db_addDeleteGameObjects_CollectMeshData);
        sb.AppendLine("\t\tdb_addDeleteGameObjects_InitFromMeshCombiner\t" + db_addDeleteGameObjects_InitFromMeshCombiner);
        sb.AppendLine("\t\tdb_addDeleteGameObjects_Init\t" + db_addDeleteGameObjects_Init);
        sb.AppendLine("\t\tdb_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers\t" + db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers);
        sb.AppendLine("\t\tdb_addDeleteGameObjects_CopyFromDGOMeshToBuffers\t" + db_addDeleteGameObjects_CopyFromDGOMeshToBuffers);

        sb.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshData  tdb_addDeleteGameObjects_CollectMeshData ");
        //sb.AppendLine("\t\t\t\t MeshChannelsCache_NativeArray.db_AA\t" + MeshChannelsCache_NativeArray.db_AA);
        //sb.AppendLine("\t\t\t\t MeshChannelsCache_NativeArray.db_BB\t" + MeshChannelsCache_NativeArray.db_BB);
        //sb.AppendLine("\t\t\t\t MeshChannelsCache_NativeArray.db_CC\t" + MeshChannelsCache_NativeArray.db_CC);
        //sb.AppendLine("\t\t\t\t MeshChannelsCache_NativeArray.db_DD\t" + MeshChannelsCache_NativeArray.db_DD);


        sb.AppendLine("db_apply\t" + db_apply);
        sb.AppendLine("db_applyShowHide\t" + db_applyShowHide);
        sb.AppendLine("db_updateGameObjects\t" + db_updateGameObjects);
        Debug.Log(sb.ToString());
    }

    public override MB3_MeshCombiner meshCombiner{
        get {return _meshCombiner;}	
    }		
    
    public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource)
    {
        UpgradeToCurrentVersionIfNecessary();
        if (_meshCombiner.resultSceneObject == null){
            _meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + name);	
        }
        meshCombiner.name = name + "-mesh";
        return _meshCombiner.AddDeleteGameObjects(gos,deleteGOs,disableRendererInSource);		
    }
    
    public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOs, bool disableRendererInSource)
    {
        UpgradeToCurrentVersionIfNecessary();
        if (_meshCombiner.resultSceneObject == null){
            _meshCombiner.resultSceneObject = new GameObject("CombinedMesh-" + name);	
        }
        meshCombiner.name = name + "-mesh";
        return _meshCombiner.AddDeleteGameObjectsByID(gos,deleteGOs,disableRendererInSource);	
    }

    public void OnDestroy()
    {
        if (_meshCombiner != null)
        {
            _meshCombiner.Dispose();
        }
    }
}
