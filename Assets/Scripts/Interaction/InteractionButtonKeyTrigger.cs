using SingletonManagers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Triggers the interaction button when the Tab key is pressed.
/// </summary>
namespace Interaction
{
    public class InteractionButtonKeyTrigger : MonoBehaviour
    {
        [SerializeField] private Button _interactionButton;

        private void OnEnable()
        {
            InputHandler.Instance.SubscribeInteract(Interact);
        }

        private void OnDisable()
        {
            InputHandler.Instance.UnsubscribeInteract(Interact);
        }
        private void Interact()
        {
            _interactionButton.onClick.Invoke();
        }

    }
}

