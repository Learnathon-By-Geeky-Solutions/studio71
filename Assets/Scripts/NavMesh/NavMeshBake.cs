using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
namespace InGameNavMesh
{
    public class NavMeshBake : MonoBehaviour
    {
        private NavMeshSurface _navMeshSurface;
        private void Awake()
        {
            _navMeshSurface = GetComponent<NavMeshSurface>();
        }

        private void Start()
        {
            _navMeshSurface.BuildNavMesh();
        }
    }
}
