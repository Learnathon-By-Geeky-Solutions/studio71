using System;
using UnityEngine;
<<<<<<< HEAD
/// <summary>
/// Controls the camera movement to follow a controllable player while maintaining a fixed offset and rotation.
/// </summary>
=======
using SingletonManagers;

>>>>>>> Level-1
namespace CameraManager
{
    public class CameraFollow : MonoBehaviour
    {
<<<<<<< HEAD
         private Transform _player; // Reference to the player's transform
        [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);  // Offset position of the camera from the player
        [SerializeField] private Vector3 _rotation = new Vector3(30f, 0f, 0f);
        void Start()
        {
            _player= GameObject.FindGameObjectWithTag("Player")?.transform;

        }
        void LateUpdate()
        {
            if (_player == null) return;
            // Move the camera to follow the player while maintaining the offset
            transform.position = _player.position + _offset;

            // Keep the camera's rotation fixed (e.g., isometric view)
            transform.rotation = Quaternion.Euler(_rotation.x, _rotation.y, _rotation.z); // Example rotation for an isometric angle
        }
    }
}
=======
        private Transform _player; // Reference to the player's transform
        [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);  // Default offset position
        [SerializeField] private Vector3 _rotation = new Vector3(30f, 0f, 0f); // Camera rotation
        [SerializeField] private float followSpeed = 5f; // Speed at which the camera adjusts its position
        [SerializeField] private float movementOffsetMultiplier = 2f; // How much movement affects the camera offset

        private Vector3 _targetOffset; // The dynamically adjusted offset

        void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            _targetOffset = _offset;
        }

        void LateUpdate()
        {
            if (_player == null) return;

            // Get movement direction from singleton input handler
            Vector2 moveDirection = InputHandler.Instance.MoveDirection;

            // Convert moveDirection into world space offset
            Vector3 movementOffset = new Vector3(moveDirection.x, 0, moveDirection.y) * movementOffsetMultiplier;

            // Smoothly adjust the offset based on movement
            _targetOffset = Vector3.Lerp(_targetOffset, _offset + movementOffset, Time.deltaTime * followSpeed);

            // Apply the final position and rotation
            transform.position = _player.position + _targetOffset;
            transform.rotation = Quaternion.Euler(_rotation);
        }
    }
}

>>>>>>> Level-1
