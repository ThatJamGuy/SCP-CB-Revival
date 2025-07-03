using UnityEngine;
using UnityEngine.InputSystem;

public class ReferenceHolder : MonoBehaviour
{
    public static ReferenceHolder Instance { get; private set; }

    void Awake() {
        Instance = this;
    }
}