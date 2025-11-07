using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace UnityEditor.Rendering.UnifiedRayTracing
{
    internal class ShaderTemplates
    {
        // TODO: Uncomment when API is made public
        //[MenuItem("Assets/Create/Shader/Unified RayTracing Shader", false, 1)]
        internal static void CreateNewUnifiedRayTracingShader()
        {
            var action = ScriptableObject.CreateInstance<DoCreateUnifiedRayTracingShaders>();
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, action, "NewRayTracingShader.hlsl", null, null);
        }

        internal static Object CreateScriptAssetWithContent(string pathName, string templateContent)
        {
            string fullPath = Path.GetFullPath(pathName);
            File.WriteAllText(fullPath, templateContent);
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }

        internal class DoCreateUnifiedRayTracingShaders : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var includeName = Path.GetFileNameWithoutExtension(pathName);
                Object o = CreateScriptAssetWithContent(pathName, shaderContent);
                CreateScriptAssetWithContent(Path.ChangeExtension(pathName, ".compute"), computeShaderContent.Replace("SHADERNAME", includeName));
                CreateScriptAssetWithContent(Path.ChangeExtension(pathName, ".raytrace"), raytracingShaderContent.Replace("SHADERNAME", includeName));
                ProjectWindowUtil.ShowCreatedAsset(o);
            }
        }

const string computeShaderContent =
@"#define UNIFIED_RT_BACKEND_COMPUTE
#define UNIFIED_RT_GROUP_SIZE_X 16
#define UNIFIED_RT_GROUP_SIZE_Y 8
#include ""SHADERNAME.hlsl""
#include_with_pragmas ""Packages/com.unity.rendering.light-transport/Runtime/UnifiedRayTracing/Compute/ComputeRaygenShader.hlsl""
";

const string raytracingShaderContent =
@"#define UNIFIED_RT_BACKEND_HARDWARE
#include ""SHADERNAME.hlsl""
#include_with_pragmas ""Packages/com.unity.rendering.light-transport/Runtime/UnifiedRayTracing/Hardware/HardwareRaygenShader.hlsl""
";

const string shaderContent =
@"#include ""Packages/com.unity.rendering.light-transport/Runtime/UnifiedRayTracing/FetchGeometry.hlsl""
#include ""Packages/com.unity.rendering.light-transport/Runtime/UnifiedRayTracing/TraceRay.hlsl""

UNIFIED_RT_DECLARE_ACCEL_STRUCT(_AccelStruct);

void RayGenExecute(UnifiedRT::DispatchInfo dispatchInfo)
{
    // Example code:
    UnifiedRT::Ray ray;
    ray.origin = 0;
    ray.direction = float3(0, 0, 1);
    ray.tMin = 0;
    ray.tMax = 1000.0f;
    UnifiedRT::RayTracingAccelStruct accelStruct = UNIFIED_RT_GET_ACCEL_STRUCT(_AccelStruct);
    UnifiedRT::Hit hitResult = UnifiedRT::TraceRayClosestHit(dispatchInfo, accelStruct, 0xFFFFFFFF, ray, 0);
    if (hitResult.IsValid())
    {
        UnifiedRT::HitGeomAttributes attributes = UnifiedRT::FetchHitGeomAttributes(accelStruct, hitResult);
    }

}
";
    }
}


