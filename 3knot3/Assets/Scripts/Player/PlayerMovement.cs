using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Controls the player movement.
/// </summary>
namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public float Move_Speed { get; set; } = 5f;
        private InputAction _moveAction;

        // Awake is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            PlayerInput _playerInput = GetComponent<PlayerInput>();
            _moveAction = _playerInput.actions.FindAction("Move");
            if (_playerInput == null) { print($"Input System is missing on {gameObject.name}"); }
        }
        // Update is called once per fixed frame that adheres by Physics System
        private void FixedUpdate()
        {
            MovePlayer();
        }
        

        // Directional Movement Method
        public void MovePlayer()
        {
            Vector2 Direction = _moveAction.ReadValue<Vector2>();
            transform.position += new Vector3(Direction.x, 0, Direction.y) * Move_Speed * Time.deltaTime;
        }
    }
}
