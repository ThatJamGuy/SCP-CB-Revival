using UnityEngine;

[System.Serializable]
public class Entity {
    public string entityIdentifier;
    public GameObject entityPrefab;
}

public class EntitySystem : MonoBehaviour {
    public static EntitySystem Instance { get; private set; }

    [SerializeField] private Entity[] entities;

    #region Unity Callbacks

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    #endregion

    #region Private Methods

    #endregion

    #region Public Methods

    #endregion
}