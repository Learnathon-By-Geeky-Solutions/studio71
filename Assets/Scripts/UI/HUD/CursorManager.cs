using UnityEngine;
using SingletonManagers;
namespace UI.HUD
{
    public class CursorManager : MonoBehaviour
    {
        private RectTransform rectTransform;
        [SerializeField] private Texture2D _transparentCursor;
        [SerializeField] private float _yOffset;
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        private void Start()
        {
            Cursor.SetCursor(_transparentCursor, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true; // Must be true for UI to work
        }

        private void Update()
        {
            rectTransform.position = new Vector3(InputHandler.Instance.MousePosition.x, InputHandler.Instance.MousePosition.y + _yOffset, 0);
        }
    }
}
