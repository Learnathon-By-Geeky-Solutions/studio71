using System;
using UnityEngine;
using SingletonManagers;
using dialogue;
/// <summary>
/// Controls the camera movement to follow a controllable player while maintaining a fixed offset and rotation.
/// </summary>
namespace CameraManager
{
    public class CameraFollow : MonoBehaviour
    {
        private Transform _player; // Reference to the player's transform
        [SerializeField] private Vector3 _offset = new Vector3(0f, 5f, -10f);  // Default offset position
        [SerializeField] private Vector3 _rotation = new Vector3(30f, 0f, 0f); // Camera rotation
        [SerializeField] private float followSpeed = 5f; // Speed at which the camera adjusts its position
        [SerializeField] private float movementOffsetMultiplier = 2f; // How much movement affects the camera offset

        private Vector3 _targetOffset; // The dynamically adjusted offset

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player").transform;
            _targetOffset = _offset;
        }

        private void LateUpdate()
        {
            if (_player == null) return;

            // Get movement direction from singleton input handler
            var moveDirection = InkDialogueManager.IsDialogueOpen ? Vector2.zero : InputHandler.Instance.MoveDirection;

            // Convert moveDirection into world space offset
            var movementOffset = new Vector3(moveDirection.x, 0, moveDirection.y) * movementOffsetMultiplier;

            // Smoothly adjust the offset based on movement
            _targetOffset = Vector3.Lerp(_targetOffset, _offset + movementOffset, Time.deltaTime * followSpeed);

            // Apply the final position and rotation
            transform.position = _player.position + _targetOffset;
            transform.rotation = Quaternion.Euler(_rotation);
        }
    }
}
