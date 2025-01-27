using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Allows the Controllable Player Look Around using Mouse.
/// </summary>
public class PlayerLookAround : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 5f;
    private Camera _mainCamera;
    private Plane _groundPlane;

    private void Awake()
    {
        _mainCamera=Camera.main;
        if (_mainCamera == null)
        {
            print("Main Camera is Missing");
            enabled = false;
            return;
        }
        _groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0)); //Plane where the ray is hitting.
    }
    private void Start()
    {
        if (_rotationSpeed <= 0f)
        {
            print("Invalid Rotation Speed. Defaulting to 5");
            _rotationSpeed = 5f;
        }
    }
    // Update is called once per frame
    private void Update()
    {
        LookAround();
    }

    public void LookAround()
    {
        if (Mouse.current == null)  return;
        Vector2 MousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(MousePosition);                  // Ray from screen mouse position to world
        // Checks is ray hit the ground
        if (_groundPlane.Raycast(ray, out float Distance))
        {

            Vector3 mouseWorldPosition = ray.GetPoint(Distance);
            // Rotate the player to face the mouse position
            Vector3 direction = mouseWorldPosition - transform.position;
            direction.y = 0; // Ignore Y-axis for rotation
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), _rotationSpeed * Time.deltaTime);
            }

        }
    }
}
