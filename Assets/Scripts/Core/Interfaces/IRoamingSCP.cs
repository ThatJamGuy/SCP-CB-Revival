using UnityEngine;

/// <summary>
/// Interface for NPCs so the EntitySystem can keep track of active NPCs
/// </summary>
public interface IRoamingSCP {
    public void WalkTo(Vector3 position);
    public void Teleport(Vector3 position);
}