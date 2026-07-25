using UnityEngine;

public class EntitySystem : MonoBehaviour {
    public static EntitySystem Instance { get; private set; }

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