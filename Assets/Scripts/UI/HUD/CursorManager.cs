using UnityEngine;
using SingletonManagers;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace UI.HUD
{
    public class CursorManager : MonoBehaviour
    {
        [SerializeField] private Texture2D _transparentCursor;
        [SerializeField] private Sprite _crossHair;
        [SerializeField] private Sprite _generalCursor;
        [SerializeField] private float _yOffset;

        private RectTransform rectTransform;
        private Image _image;
        private void Awake()
        {
            _image = GetComponent<Image>();
            rectTransform = GetComponent<RectTransform>();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            if (_crossHair == null||_generalCursor==null)
            {
                Debug.LogWarning($"Sprite Field empty on {gameObject.name}");
            }
            if(SceneManager.GetActiveScene().buildIndex==1)
            {
                _image.sprite = _crossHair;
            }
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
