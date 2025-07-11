#if UNITY_EDITOR && SORTIFY
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sortify
{
    public static class SortifyComponentManager
    {
        private const string COMPONENTS_FILE_NAME = "Sortify_CustomComponents.json";

        private static ComponentListSerializer _cachedComponentList;
        private static List<System.Type> _cachedUserComponents = new List<System.Type>();
        private static readonly List<System.Type> _cachedAllTypes = GetAllComponentTypes();

        private static bool _dataLoaded = false;

        public static List<System.Type> GetDefaultComponents()
        {
            if (SortifyUserDataManager.GetUserSetting("ShowDefaultComponents", true))
            {
                return new List<System.Type>
                {
                    typeof(BoxCollider),
                    typeof(SphereCollider),
                    typeof(CapsuleCollider),
                    typeof(MeshCollider),
                    typeof(Rigidbody),
                };
            }
            else
            {
                return new List<System.Type>();
            }
        }

        public static List<System.Type> LoadUserComponents()
        {
            LoadComponentListIfNeeded();
            LoadUserComponentsIfNeeded();
            return _cachedUserComponents;
        }

        public static void AddComponent(string typeName)
        {
            LoadComponentListIfNeeded();
            LoadUserComponentsIfNeeded();
            var component = GetComponent(typeName);
            if (component != null)
            {
                if (!_cachedUserComponents.Contains(component))
                {
                    _cachedUserComponents.Add(component);
                    SaveComponentListToFile();
                }
            }
        }

        public static void RemoveComponent(string typeName)
        {
            LoadComponentListIfNeeded();
            LoadUserComponentsIfNeeded();
            var component = GetComponent(typeName);
            if (component != null)
            {
                if (_cachedUserComponents.Contains(component))
                {
                    _cachedUserComponents.Remove(component);
                    SaveComponentListToFile();
                }
            }
        }

        private static System.Type GetComponent(string typeName)
        {
            System.Type foundType = _cachedAllTypes.FirstOrDefault(t => t.FullName == typeName);
            if (foundType != null)
            {
                return foundType;
            }
            else
            {
                return null;
            }
        }

        private static List<System.Type> GetAllComponentTypes()
        {
            try
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(t => typeof(Component).IsAssignableFrom(t))
                    .ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SortifyComponentManager] Error getting all component types: {ex.Message}");
                return new List<System.Type>();
            }
        }

        public static void LoadComponentListIfNeeded(bool forceLoad = false)
        {
            if (forceLoad)
                _dataLoaded = false;

            if (!_dataLoaded)
            {
                LoadComponentListFromFile();
                _dataLoaded = true;
            }
        }

        private static void LoadUserComponentsIfNeeded()
        {
            if (_cachedUserComponents.Count == 0)
            {
                foreach (var typeName in _cachedComponentList.customComponents)
                {
                    System.Type foundType = _cachedAllTypes.FirstOrDefault(t => t.FullName == typeName);
                    if (foundType == null)
                    {
                        Debug.LogError($"[SortifyComponentManager] Type {typeName} could not be found.");
                    }
                    else if (!foundType.IsAbstract && typeof(Component).IsAssignableFrom(foundType))
                    {
                        _cachedUserComponents.Add(foundType);
                    }
                }
            }
        }

        private static void LoadComponentListFromFile()
        {
            var loadedData = SortifyFileManager.LoadFromFile<ComponentListSerializer>(COMPONENTS_FILE_NAME);
            if (loadedData != null)
            {
                _cachedComponentList = loadedData;
            }
            else
            {
                _cachedComponentList = new ComponentListSerializer();
            }
        }

        private static void SaveComponentListToFile()
        {
            var componentsList = _cachedUserComponents.Select(component => component.FullName).ToList();
            _cachedComponentList.customComponents = componentsList;
            SortifyFileManager.SaveToFile(COMPONENTS_FILE_NAME, _cachedComponentList);
        }
    }

    [System.Serializable]
    public class ComponentListSerializer
    {
        public List<string> customComponents = new List<string>();

        public ComponentListSerializer() { }
        public ComponentListSerializer(List<string> customComponents)
        {
            this.customComponents = customComponents;
        }
    }
}
#endif
