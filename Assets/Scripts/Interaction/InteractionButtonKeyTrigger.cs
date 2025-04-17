using SingletonManagers;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects keyboard input to the interaction button in the UI.
/// Forwards interaction input events to the button's onClick handler.
/// </summary>
namespace Interaction
{
    public class InteractionButtonKeyTrigger : MonoBehaviour
    {
        [SerializeField] private Button _interactionButton;

        private void Start()
        {
            if (_interactionButton == null)
            {
                Debug.LogError("Interaction button reference is missing!", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnInteract += HandleInteractionInput;
            }
            else
            {
                Debug.LogError("InputHandler instance is not available!", this);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnInteract -= HandleInteractionInput;
            }
        }

        /// <summary>
        /// Handles the interaction input event by forwarding it to the button click.
        /// </summary>
        private void HandleInteractionInput()
        {
            if (_interactionButton != null && _interactionButton.isActiveAndEnabled)
            {
                _interactionButton.onClick.Invoke();
            }
        }
    }
}

