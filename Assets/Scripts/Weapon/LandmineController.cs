using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using HealthSystem;
using SingletonManagers;
using System.Linq;
namespace Weapon
{
    public class LandmineController : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float alertRadius = 5f;        // How close the player needs to be to trigger alert
        [SerializeField] private float alertDuration = 3f;      // Time between alert and explosion
        [SerializeField] private LayerMask hitLayers;           // Layers that can be damaged

        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 3f;    // Explosion damage radius
        [SerializeField] private float explosionForce = 700f;   // Force applied by explosion
        [SerializeField] private int damage = 100;              // Damage caused by explosion

        [Header("References")]
        [SerializeField] private GameObject alertIndicator;     // Visual indicator for alert state (optional)
        [SerializeField] private string explosionEffectName = "Grenade Explosion";  // Effect name from ParticleManager

        private enum LandmineState { Idle, Alert, Exploded }
        private LandmineState currentState = LandmineState.Idle;
        private bool playerDetected = false;
        private bool isProcessingAlert = false;
        
        private void Start()
        {
            // Make sure alert indicator is inactive at start
            if (alertIndicator != null)
            {
                alertIndicator.SetActive(false);
            }
        }

        private void Update()
        {
            if (currentState == LandmineState.Exploded || isProcessingAlert)
                return;
                
            // Check if player is in alert range
         Collider[] colliders = Physics.OverlapSphere(transform.position, alertRadius);
playerDetected = colliders.Any(collider => collider.CompareTag("Player"));
            
            // If player detected and mine is idle, switch to alert state
            if (playerDetected && currentState == LandmineState.Idle)
            {
                AlertSequenceAsync().Forget();
            }
        }
        
        private async UniTaskVoid AlertSequenceAsync()
        {
            // Prevent multiple alert sequences
            if (isProcessingAlert) return;
            isProcessingAlert = true;
            
            // Switch to alert state
            currentState = LandmineState.Alert;
            
            // Activate alert indicator if available
            if (alertIndicator != null)
            {
                alertIndicator.SetActive(true);
            }
            
            // Play alert sound if available
            AudioManager.PlaySound(SoundKeys.BombBeep, transform.position, 1f, 1f);
        
            // Wait for alert duration
            await UniTask.Delay(TimeSpan.FromSeconds(alertDuration), cancellationToken: this.GetCancellationTokenOnDestroy());
            
            // Check if player is still in range
            if (playerDetected)
            {
                Explode();
            }
            else
            {
                // Player left range, return to idle
                currentState = LandmineState.Idle;
                if (alertIndicator != null)
                {
                    alertIndicator.SetActive(false);
                }
            }
            
            isProcessingAlert = false;
        }
        
        private void Explode()
        {
            // Switch to exploded state
            currentState = LandmineState.Exploded;
            
            // Play explosion effects using ParticleManager
            if (!string.IsNullOrEmpty(explosionEffectName) && ParticleManager.Instance != null)
            {
                ParticleManager.Instance.PlayParticle(explosionEffectName, transform.position, Quaternion.identity);
            }
            
            // Play explosion sound using AudioManager
                AudioManager.PlaySound(SoundKeys.BombExplosion, transform.position, 1f, 1f);
            
            // Apply damage and physics force to objects in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider hit in colliders)
            {
                // Skip if not in hit layers
                if (((1 << hit.gameObject.layer) & hitLayers) == 0) continue;
                
                // Apply damage to objects with Health component
                Health health = hit.GetComponent<Health>();
                if (health != null)
                {
                    // Calculate damage with falloff based on distance
                    float distance = Vector3.Distance(transform.position, hit.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    int calculatedDamage = Mathf.RoundToInt(damage * damageMultiplier);
                    health.TakeDmg(calculatedDamage);
                }
                
                // Apply physics force
                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }
            }
            
            // Make mine mesh invisible but keep effects playing
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
            
            // Destroy the landmine after explosion (with delay for effects)
            Destroy(gameObject, 5f);
        }
        
        // Draw gizmos to visualize alert and explosion ranges
        private void OnDrawGizmos()
        {
            // Draw alert radius
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
            Gizmos.DrawSphere(transform.position, alertRadius);
            
            // Draw alert radius wire
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, alertRadius);
            
            // Draw explosion radius 
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f); // Semi-transparent red
            Gizmos.DrawSphere(transform.position, explosionRadius);
            
            // Draw explosion radius wire
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
            
            // Draw line connecting center to ground for better visibility
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z));
        }
    }
}