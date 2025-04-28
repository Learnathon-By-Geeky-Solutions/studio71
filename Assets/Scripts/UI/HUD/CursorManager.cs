using System;
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

        private RectTransform _rectTransform;
        private PauseMenu _pauseMenu;
        private Image _image;
        private void Awake()
        {
            _image = GetComponent<Image>();
            _rectTransform = GetComponent<RectTransform>();
            _pauseMenu=GetComponentInParent<PauseMenu>();
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
            if (_crossHair == null||_generalCursor==null)
            {
                Debug.LogWarning($"Sprite Field empty on {gameObject.name}");
            }
            _image.sprite = SceneManager.GetActiveScene().buildIndex >= 2 ? _crossHair : _generalCursor;
        }
        private void OnEnable()
        {
            InputHandler.Instance.OnPause += CursorChange;
        }
        private void OnDisable()
        {
            InputHandler.Instance.OnPause -= CursorChange;
        }
        private void Start()
        {
            Cursor.SetCursor(_transparentCursor, Vector2.zero, CursorMode.Auto);
            Cursor.visible = true; // Must be true for UI to work
        }

        private void Update()
        {
            _rectTransform.position = new Vector3(InputHandler.Instance.MousePosition.x, InputHandler.Instance.MousePosition.y + _yOffset, 0);
        }

        private void CursorChange()
        {
            if (SceneManager.GetActiveScene().buildIndex < 2) return;
            if (_pauseMenu.IsGamePaused() )
            {
                _yOffset = 35;
                _image.sprite = _crossHair;
            }
            else { _yOffset = 0; _image.sprite = _generalCursor; }
        }
    }
}
