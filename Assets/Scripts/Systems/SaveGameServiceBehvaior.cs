using System.Collections.Generic;
using UnityEngine;

public class SaveGameServiceBehvaior : MonoBehaviour {
    [SerializeField] private MonoBehaviour[] saveableComponents;

    private SaveGameService service;

    public bool CanSave { get; set; } = true;

    private void Awake() {
        List<ISaveable> saveables = new();

        foreach (MonoBehaviour component in saveableComponents) {
            if (component == null) continue;
            if (component is not ISaveable saveable) {
                Debug.LogError($"{component.name} is assigned as a saveable but does not implement ISaveable.", component);
                continue;
            }

            saveables.Add(saveable);
        }

        service = new SaveGameService(saveables);
    }

    public void Save() {
        if (!CanSave)
            return;

        service.Save();
    }

    public void Load() {
        service.Load();
    }
}