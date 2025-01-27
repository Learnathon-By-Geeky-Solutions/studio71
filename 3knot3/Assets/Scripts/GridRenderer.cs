using System;
using UnityEngine;
/// <summary>
/// Renders a grid on the plane for better visualization.
/// </summary>
public class GridRenderer : MonoBehaviour
{
    [SerializeField] private int _gridSize = 10;
    [Min(0.1f)]
    [SerializeField] private float _cellSize = 1f; // Size of each cell
    private void OnDrawGizmos()
    {
        Vector2 gridSize = new Vector2(_gridSize, _gridSize);
        Gizmos.color = Color.white; // Set grid line color

        for (float x = 0; x <= gridSize.x; x++)
        {
            Vector3 start = new Vector3(x * _cellSize, 0, 0);
            Vector3 end = new Vector3(x * _cellSize, 0, gridSize.y * _cellSize);
            Gizmos.DrawLine(start, end);
        }

        for (float z = 0; z <= gridSize.y; z++)
        {
            Vector3 start = new Vector3(0, 0, z * _cellSize);
            Vector3 end = new Vector3(gridSize.x * _cellSize, 0, z * _cellSize);
            Gizmos.DrawLine(start, end);
        }
    }
}
