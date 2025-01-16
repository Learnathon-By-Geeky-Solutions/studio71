using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLookAround : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LookAround();
    }

    public void LookAround()
    {
        Vector2 MousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(MousePosition);                  // Ray from screen mouse position to world
        Plane GroundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));      // Plane where the ray is hitting

        // Checks is ray hit the ground
        if (GroundPlane.Raycast(ray, out float Distance))
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
