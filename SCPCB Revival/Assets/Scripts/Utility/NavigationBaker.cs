using UnityEngine;
using Unity.AI.Navigation;

public class NavigationBaker : MonoBehaviour
{
    [SerializeField] private bool bakeOnStart;
    [SerializeField] private bool allowRealtimeBaking;

    [SerializeField] private NavMeshSurface surface;

    private void Start()
    {
        if (bakeOnStart)
            BakeNavigationMesh();
    }

    private void Update()
    {
        if (allowRealtimeBaking)
            BakeNavigationMesh();
    }

    public void BakeNavigationMesh()
    {
        surface.BuildNavMesh();
    }
}