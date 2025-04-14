using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D _reticleTexture;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector2 hotspot = new Vector2(_reticleTexture.width / 2, _reticleTexture.height / 2);
        Cursor.SetCursor(_reticleTexture, hotspot, CursorMode.Auto);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }
}
