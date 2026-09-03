using UnityEngine;

// Note: Currently only works for initialized objects.
// Objects spawned in later as of right now will not have a wireframe
public class WireframeUtility : MonoBehaviour {
    [SerializeField] private bool enableWireframe;
    [SerializeField] private Shader wireframeShader;

    private bool isEnabled;
    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private Material wireMaterial;

    private void Awake() {
        renderers = FindObjectsByType<Renderer>();
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++) {
            Material[] mats = renderers[i].sharedMaterials;
            Material[] copy = new Material[mats.Length];
            for (int j = 0; j < mats.Length; j++) {
                copy[j] = mats[j];
            }
            originalMaterials[i] = copy;
        }

        if (wireframeShader != null) {
            wireMaterial = new Material(wireframeShader);
        }
    }

    private void Update() {
        if (enableWireframe && !isEnabled) {
            SetWireframe(true);
            isEnabled = true;
        } else if (!enableWireframe && isEnabled) {
            SetWireframe(false);
            isEnabled = false;
        }
    }

    private void SetWireframe(bool on) {
        if (wireMaterial == null) return;

        for (int i = 0; i < renderers.Length; i++) {
            Renderer r = renderers[i];
            if (on) {
                Material[] wireSet = new Material[originalMaterials[i].Length];
                for (int j = 0; j < wireSet.Length; j++) {
                    wireSet[j] = wireMaterial;
                }
                r.materials = wireSet;
            } else {
                r.materials = originalMaterials[i];
            }
        }
    }

    private void OnDestroy() {
        if (wireMaterial != null) {
            Destroy(wireMaterial);
        }
    }
}