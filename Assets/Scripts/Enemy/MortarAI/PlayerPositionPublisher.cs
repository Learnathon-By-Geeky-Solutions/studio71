using UnityEngine;

namespace MortarAI
{
    public class PlayerPositionPublisher : MonoBehaviour
    {
        [SerializeField] private float publishInterval = 0.1f;

        private float lastPublishTime;
        private Vector3 lastPosition;
        private Vector3 velocity;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (Time.time - lastPublishTime >= publishInterval)
            {
                // Use rigidbody velocity if available, otherwise calculate from position
                velocity = rb != null ? rb.linearVelocity : (transform.position - lastPosition) / publishInterval;
                lastPosition = transform.position;

                // Publish position update
                GameEvents.PublishPlayerPosition(transform.position, velocity);

                lastPublishTime = Time.time;
            }
        }
    }
}