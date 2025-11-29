//----------------------------------------------
//            MeshBaker
// Copyright © 2011-2012 Ian Deane
//----------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Specialized;
using System;
using System.Collections.Generic;
using System.Text;
using DigitalOpus.MB.Core;
using System.Text.RegularExpressions;

/// <summary>
/// Component that manages a single combined mesh. 
/// 
/// This class is a Component. It must be added to a GameObject to use it. It is a wrapper for MB2_MeshCombiner which contains the same functionality but is not a component
/// so it can be instantiated like a normal class.
/// </summary>
public class MB3_MeshBaker : MB3_MeshBakerCommon {	  
	
	[SerializeField] protected MB3_MeshCombinerSingle _meshCombiner = new MB3_MeshCombinerSingle();

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
        
        double db_addDeleteGameObjects_CollectMeshData_a = 0;
        double db_addDeleteGameObjects_CollectMeshData_b = 0;
        double db_addDeleteGameObjects_CollectMeshData_c = 0;

        double db_apply = 0;
        double db_applyShowHide = 0;
        double db_updateGameObjects = 0;

        {
            MB3_MeshCombinerSingle c = _meshCombiner;

            db_showHideGameObjects += c.db_showHideGameObjects.Elapsed.TotalSeconds;
            db_addDeleteGameObjects += c.db_addDeleteGameObjects.Elapsed.TotalSeconds;

            db_addDeleteGameObjects_CollectMeshData += c.db_addDeleteGameObjects_CollectMeshData.Elapsed.TotalSeconds;

            db_addDeleteGameObjects_CollectMeshData_a += c.db_addDeleteGameObjects_CollectMeshData_a.Elapsed.TotalSeconds;
            db_addDeleteGameObjects_CollectMeshData_b += c.db_addDeleteGameObjects_CollectMeshData_b.Elapsed.TotalSeconds;
            db_addDeleteGameObjects_CollectMeshData_c += c.db_addDeleteGameObjects_CollectMeshData_c.Elapsed.TotalSeconds;

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

        sb.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshDataA\t\t" + db_addDeleteGameObjects_CollectMeshData_a);
        sb.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshDataB\t\t" + db_addDeleteGameObjects_CollectMeshData_b);
        sb.AppendLine("\t\tdb_addDeleteGameObjects_CollectMeshDataC\t\t" + db_addDeleteGameObjects_CollectMeshData_c);

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
		get{return _meshCombiner;}	
	}
	
	public void BuildSceneMeshObject(){
		_meshCombiner.BuildSceneMeshObject();
	}

	public virtual bool ShowHide(GameObject[] gos, GameObject[] deleteGOs){
		return _meshCombiner.ShowHideGameObjects(gos, deleteGOs);
	}

	public virtual void ApplyShowHide(){
		_meshCombiner.ApplyShowHide();		
	}
	
	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource){
		UpgradeToCurrentVersionIfNecessary();
		//		if ((_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoSceneObject || (_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoPrefab && _meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer) )) BuildSceneMeshObject();
		_meshCombiner.name = name + "-mesh";
		return _meshCombiner.AddDeleteGameObjects(gos,deleteGOs,disableRendererInSource);		
	}
	
	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource){
		UpgradeToCurrentVersionIfNecessary();
		//		if ((_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoSceneObject || (_meshCombiner.outputOption == MB2_OutputOptions.bakeIntoPrefab && _meshCombiner.renderType == MB_RenderType.skinnedMeshRenderer) )) BuildSceneMeshObject();
		_meshCombiner.name = name + "-mesh";
		return _meshCombiner.AddDeleteGameObjectsByID(gos,deleteGOinstanceIDs,disableRendererInSource);
	}

    public void OnDestroy()
    {
		if (meshCombiner != null)
		{
            meshCombiner.Dispose();
		}
    }
}
