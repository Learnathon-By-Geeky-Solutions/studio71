using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    PlayerInput PlayerInput;
    InputAction MoveAction;

    // Awake is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        MoveAction = PlayerInput.actions.FindAction("Move");
        //Cursor.visible=false;
    }
    /*void Start()
    {
        // Getting all the necessary components for movement
        PlayerInput = GetComponent<PlayerInput>();
        MoveAction = PlayerInput.actions.FindAction("Move");

    }*/
    // Remove Documentation Before Build

    // Update is called once per fixed frame that adheres by Physics System
    void Update()
    {
        MovePlayer();
    }

    // Directional Movement Method
    public void MovePlayer()
    {
        Vector2 Direction = MoveAction.ReadValue<Vector2>();
        transform.position += new Vector3(Direction.x, 0, Direction.y) * _moveSpeed * Time.deltaTime;
                
    }
}
