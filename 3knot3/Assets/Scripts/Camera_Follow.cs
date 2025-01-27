using System;
using UnityEngine;
/// <summary>
/// Controls the camera movement to follow a controllable player while maintaining a fixed offset and rotation.
/// </summary>
namespace CameraManager
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _player; // Reference to the player's transform
        [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);  // Offset position of the camera from the player
        [SerializeField] private Vector3 _rotation = new Vector3(30f, 0f, 0f);
        void Start()
        {
            if (_player == null)
            {
                print("Player reference not set in the CameraFollow component");
                enabled = false;
            }
        }
        void LateUpdate()
        {
            // Move the camera to follow the player while maintaining the offset
            transform.position = _player.position + _offset;

            // Keep the camera's rotation fixed (e.g., isometric view)
            transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, _rotation.z); // Example rotation for an isometric angle
        }
    }
}
