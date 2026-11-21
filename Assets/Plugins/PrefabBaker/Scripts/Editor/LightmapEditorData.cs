#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace PrefabLightMapBaker {
    internal static class LightmapEditorData {
        // Snapshot of only the values SetProfileQuickBake() overrides
        struct Snapshot {
            public int bounces;
            public int directSampleCount;
            public int indirectSampleCount;
            public float lightmapResolution;
            public bool ao;
            public LightmapCompression lightmapCompression;
            public float indirectResolution;
            public LightingSettings.Lightmapper lightmapper;
        }

        static Snapshot backup;
        static bool hasBackup;

        public static bool HasBackup => hasBackup;

        static LightingSettings GetSettings() {
            LightingSettings settings;

            if (!Lightmapping.TryGetLightingSettings(out settings)) {
                settings = new LightingSettings();
                Lightmapping.lightingSettings = settings;
            }

            return settings;
        }

        public static void Backup() {
            var s = GetSettings();

            backup.bounces = s.maxBounces;
            backup.directSampleCount = s.directSampleCount;
            backup.indirectSampleCount = s.indirectSampleCount;
            backup.lightmapResolution = s.lightmapResolution;
            backup.ao = s.ao;
            backup.lightmapCompression = s.lightmapCompression;
            backup.indirectResolution = s.indirectResolution;
            backup.lightmapper = s.lightmapper;

            hasBackup = true;
        }

        public static void Restore() {
            if (!hasBackup) return;

            hasBackup = false;

            LightingSettings s;

            if (!Lightmapping.TryGetLightingSettings(out s)) return;

            s.maxBounces = backup.bounces;
            s.directSampleCount = backup.directSampleCount;
            s.indirectSampleCount = backup.indirectSampleCount;
            s.lightmapResolution = backup.lightmapResolution;
            s.ao = backup.ao;
            s.lightmapCompression = backup.lightmapCompression;
            s.indirectResolution = backup.indirectResolution;
            s.lightmapper = backup.lightmapper;
        }

        public static new string ToString() {
            LightingSettings s;

            if (!Lightmapping.TryGetLightingSettings(out s)) return "No LightingSettings";

            return "maxBounces:" + s.maxBounces
                + ",directSampleCount:" + s.directSampleCount
                + ",indirectSampleCount:" + s.indirectSampleCount
                + ",lightmapResolution:" + s.lightmapResolution
                + ",ao:" + s.ao
                + ",lightmapCompression:" + s.lightmapCompression
                + ",indirectResolution:" + s.indirectResolution
                + ",lightmapper:" + s.lightmapper;
        }

        public static void SetProfileQuickBake() {
            var s = GetSettings();

            s.maxBounces = 1;
            s.directSampleCount = 2;
            s.indirectSampleCount = 8;
            s.lightmapResolution = 20f;
            s.ao = false;
            s.lightmapCompression = LightmapCompression.NormalQuality;
            s.indirectResolution = 2f;
            s.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
        }
    }
}

#endif