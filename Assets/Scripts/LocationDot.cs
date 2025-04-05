using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class LocationDot : MonoBehaviour
{
    [SerializeField] private Image _image;
    
    private void OnValidate()
    {
        if (_image == null)
            _image = GetComponent<Image>();
    }
    
    // You could add tooltip or other hover functionality here if needed
} 