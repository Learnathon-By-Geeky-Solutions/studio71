using System.Collections;
using UnityEngine;

namespace MortarAI
{
    public class MortarTeleporter : MonoBehaviour
    {
        [Header("Smoke Bomb Settings")]
        [SerializeField] private float smokeBombCooldown = 20f;
        [SerializeField] private float teleportDistance = 10f;
        [SerializeField] private float teleportMaxHeight = 5f;
        [SerializeField] private GameObject smokeBombEffectPrefab;
        [SerializeField] private LayerMask teleportableSurfaces;

        private float lastSmokeBombTime;
        private Renderer enemyRenderer;
        private Collider enemyCollider;

        private void Awake()
        {
            enemyRenderer = GetComponent<Renderer>();
            enemyCollider = GetComponent<Collider>();
            lastSmokeBombTime = -smokeBombCooldown; // Allow immediate first teleport
        }

        public bool CanTeleport()
        {
            return Time.time > lastSmokeBombTime + smokeBombCooldown;
        }

        public IEnumerator ExecuteTeleport(Vector3 playerPosition, float optimalRange)
        {
            lastSmokeBombTime = Time.time;

            // Play smoke effect
            Instantiate(smokeBombEffectPrefab, transform.position, Quaternion.identity);

            // Disable renderer and collider
            enemyRenderer.enabled = false;
            enemyCollider.enabled = false;

            yield return new WaitForSeconds(0.5f);

            // Find a suitable teleport position
            Vector3 newPosition = FindTeleportPosition(playerPosition, optimalRange);
            transform.position = newPosition;

            // Re-enable renderer with another smoke effect
            Instantiate(smokeBombEffectPrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.3f);

            enemyRenderer.enabled = true;
            enemyCollider.enabled = true;
        }

        private Vector3 FindTeleportPosition(Vector3 playerPos, float optimalRange)
        {
            for (int i = 0; i < 10; i++) // Try up to 10 positions
            {
                // Get random direction away from player (in a semicircle facing away)
                Vector3 directionFromPlayer = transform.position - playerPos;
                directionFromPlayer.y = 0;
                directionFromPlayer.Normalize();

                // Add some randomness to the direction
                float randomAngle = Random.Range(-90f, 90f);
                directionFromPlayer = Quaternion.Euler(0, randomAngle, 0) * directionFromPlayer;

                // Calculate potential new position
                Vector3 potentialPosition = playerPos +
                    directionFromPlayer * Random.Range(optimalRange * 0.6f, optimalRange * 0.9f);

                // Raycast down to find ground
                RaycastHit hit;
                if (Physics.Raycast(potentialPosition + Vector3.up * teleportMaxHeight, Vector3.down,
                    out hit, teleportMaxHeight * 2, teleportableSurfaces))
                {
                    // Check if we have line of sight to player from this position
                    Vector3 finalPosition = hit.point + Vector3.up * 0.5f; // Offset from ground

                    if (!Physics.Linecast(finalPosition + Vector3.up,
                        playerPos + Vector3.up, teleportableSurfaces))
                    {
                        return finalPosition;
                    }
                }
            }

            // If no good position found, just move directly away from player
            return transform.position + (transform.position - playerPos).normalized * teleportDistance;
        }

        // Getter for state pattern
        public float GetSmokeBombCooldown() => smokeBombCooldown;
    }
}