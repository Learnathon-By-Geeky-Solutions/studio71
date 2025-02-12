using UnityEngine;
using UnityEngine.InputSystem;
/// <summary>
/// Controls player interaction.
/// </summary>
namespace Interaction
{
    public class InteractionSystem : MonoBehaviour
    {
        [SerializeField] private float _interactionRadius = 2f;      // Interaction distance
        LayerMask _interactableLayer = 1 << 6;     // Layer for interactable objects

        //Interacts with the selected layer.
        public void Interact(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, _interactionRadius, _interactableLayer);

                foreach (Collider hit in hits)
                {
                    print($"Interacted with {hit.gameObject.name}");

                }
            }
        }
        
        //Changes the layer that should be interacted with.
        public void LayerChange(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                if (_interactableLayer == (1 << 6))
                {
                    _interactableLayer = (1 << 7);
                }
                else { _interactableLayer = (1 << 6); }
            }
        }
        //To Visualize interaction sphere.
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _interactionRadius);
        }
    }
}
