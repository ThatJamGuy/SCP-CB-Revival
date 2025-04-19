using System.Collections.Generic;
using UnityEngine;
using vectorarts.scpcbr;

public class RoomRenderer : MonoBehaviour {
    public Transform player;
    public float disableRange = 10f;
    private List<MeshRenderer> roomRenderers = new();

    void Start() {
        var generator = Object.FindFirstObjectByType<MapGenerator>();
        if (generator != null)
            generator.OnMapFinishedGenerating.AddListener(InitializeRendererList);
    }

    void InitializeRendererList() {
        roomRenderers.Clear();
        foreach (var obj in GameObject.FindGameObjectsWithTag("Room"))
            foreach (var renderer in obj.GetComponentsInChildren<MeshRenderer>())
                if (!roomRenderers.Contains(renderer))
                    roomRenderers.Add(renderer);
    }

    void Update() {
        foreach (var renderer in roomRenderers)
            if (renderer)
                renderer.enabled = Vector3.Distance(player.position, renderer.transform.position) <= disableRange;
    }
}