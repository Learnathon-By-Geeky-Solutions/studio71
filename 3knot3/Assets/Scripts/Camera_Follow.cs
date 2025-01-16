using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform _player; // Reference to the player's transform
    [SerializeField] Vector3 _offset;   // Offset position of the camera from the player
    [SerializeField] Vector3 _rotation;

    void LateUpdate()
    {
        // Move the camera to follow the player while maintaining the offset
        transform.position = _player.position + _offset;

        // Keep the camera's rotation fixed (e.g., isometric view)
        transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, _rotation.z); // Example rotation for an isometric angle
    }
}
