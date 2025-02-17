using UnityEngine;
/// <summary>
/// Helps understand which direction the player is facing.
/// </summary>

namespace Player
{
    public class PlayerFacingDirection : MonoBehaviour
    {
        [SerializeField] private float _lineLength;
        private void OnDrawGizmos()
        {
            Vector3 startPosition = transform.position;

            // End position of the line (in the forward direction)
            Vector3 endPosition = startPosition + transform.forward * _lineLength;

            // Draw the line in the Scene view
            Gizmos.DrawLine(startPosition, endPosition);
        }
    }
}
