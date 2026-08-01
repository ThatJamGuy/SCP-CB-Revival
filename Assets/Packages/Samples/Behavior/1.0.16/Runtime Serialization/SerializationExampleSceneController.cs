using System;
using System.Collections.Generic;
using System.IO;
using Unity.Behavior.Example;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Unity.Behavior.SerializationExample
{
    public class SerializationExampleSceneController : MonoBehaviour
    {
        private const string k_SaveFileName = "serializationSaveFile.json";

        [SerializeField] private BehaviorGraphAgent m_AgentPrefab;
        [SerializeField] private int m_Count;

        private List<BehaviorGraphAgent> m_Agents = new();
        private GameObjectResolver m_GameObjectResolver = new();
        private RuntimeSerializationUtility.JsonBehaviorSerializer m_JsonSerializer = new();

        [Serializable]
        private class AgentData
        {
            public string Id;
            public Vector3 Position;

            /// <summary>
            /// It's important to note, when saving any agents graph data,
            /// you must use the provided <see cref="BehaviorGraphAgent.Serialize"/> & <see cref="BehaviorGraphAgent.Deserialize"/> methods.
            /// Serializing the graph data directly via JSON will not work.
            /// </summary>
            public string SerializedGraphData;

            public AgentData(string id, Vector3 position, string serializedGraphGraphData)
            {
                Id = id;
                Position = position;
                SerializedGraphData = serializedGraphGraphData;
            }
        }

        [Serializable]
        private class SaveData
        {
            public List<AgentData> AgentsData = new List<AgentData>();
        }

        private void Start()
        {
            Random.InitState(0);
            for (int idx = 0; idx < m_Count; ++idx)
            {
                BehaviorGraphAgent agent = Instantiate(m_AgentPrefab, transform);
                agent.name = $"Agent_{idx}";

                // We also need to rename the weapon to avoid name conflicts.
                // This allows us to keep the correct reference using the current implementation of the GameObjectResolver.
                var weapon = agent.GetComponentInChildren<Weapon>();
                weapon.name = $"Weapon_{idx}";
                m_Agents.Add(agent);
            }
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(5, 5, 150, 90), "Menu");
            if (GUI.Button(new Rect(10, 30, 130, 20), "Save"))
            {
                SerializeAgents();
            }
            if (GUI.Button(new Rect(10, 60, 130, 20), "Load"))
            {
                DeserializeAgents();
            }
        }

        private void SerializeAgents()
        {
            SaveData saveData = new SaveData
            {
                AgentsData = new List<AgentData>()
            };

            string path = Path.Combine(Application.dataPath, k_SaveFileName);

            foreach (var agent in m_Agents)
            {
                string serializedGraphData = agent.Serialize(m_JsonSerializer, m_GameObjectResolver);
                saveData.AgentsData.Add(new AgentData
                (
                    agent.name,
                    agent.transform.position,
                    serializedGraphData
                ));
            }

            string json = JsonUtility.ToJson(saveData);
            File.WriteAllText(path, json);
        }

        private void DeserializeAgents()
        {
            string path = Path.Combine(Application.dataPath, k_SaveFileName);

            if (!File.Exists(path))
            {
                Debug.LogWarning("There was no file to load from. Make sure to save first.");
                return;
            }

            string jsonData = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            foreach (var agent in m_Agents)
            {
                foreach (var agentData in saveData.AgentsData)
                {
                    if (agentData.Id == agent.name)
                    {
                        agent.transform.position = agentData.Position;
                        agent.Deserialize(agentData.SerializedGraphData, m_JsonSerializer, m_GameObjectResolver);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// We recommend writing your own GameObject resolver that is more efficient & meets your projects needs, this is a simple example.
    /// </summary>
    public class GameObjectResolver : RuntimeSerializationUtility.IUnityObjectResolver<string>
    {
        public string Map(Object obj) => obj ? obj.name : null;

        public TSerializedType Resolve<TSerializedType>(string mappedValue) where TSerializedType : Object
        {
            // It would be recommended to have a more robust way to resolve objects by name or id using a registry.
            // Due to using the gameobject name as the key, you should to ensure that the names are unique.
            // If you do not have unique gameobject naming, you may run into exceptions caused by the wrong objects being resolved due to naming conflicts.
            GameObject obj = GameObject.Find(mappedValue);
            if (!obj)
            {
                // If we didn't find the object by name in the scene, it might be a prefab.
                TSerializedType[] prefabs = Resources.FindObjectsOfTypeAll<TSerializedType>();
                foreach (var prefab in prefabs)
                {
                    if (prefab.name == mappedValue)
                    {
                        return prefab;
                    }
                }

                return null;
            }
            if (typeof(TSerializedType) == typeof(GameObject))
            {
                return obj as TSerializedType;
            }
            if (typeof(Component).IsAssignableFrom(typeof(TSerializedType)))
            {
                return obj.GetComponent<TSerializedType>();
            }
            return null;
        }
    }
}
