using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Triggers the interaction button when the Tab key is pressed.
/// </summary>
namespace Interaction{
public class InteractionButtonKeyTrigger : MonoBehaviour
{
    [SerializeField] private Button _interactionButton;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && _interactionButton != null)
        {
            _interactionButton.onClick.Invoke();
        }
    }
}
}
