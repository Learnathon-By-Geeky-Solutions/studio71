using System.Collections;
using UnityEngine;

namespace MortarAI
{
    public class MortarTargetingSystem : MonoBehaviour
    {
        [Header("Attack Settings")]
        [SerializeField] private float attackRange = 15f; // Range within which the enemy can attack
        [SerializeField] private float minAttackRange = 5f; // Minimum range to avoid attacking too close
        [SerializeField] private float attackCooldown = 3f; // Cooldown between attacks
        [SerializeField] private float mortarSpeed = 15f; // Speed of the mortar projectile
        [SerializeField] private float targetingTime = 1.5f; // Time spent targeting before firing

        [Header("Projectile Settings")]
        [SerializeField] private GameObject mortarProjectilePrefab; // Prefab for the mortar projectile
        [SerializeField] private GameObject targetingIndicatorPrefab; // Prefab for the targeting indicator
        [SerializeField] private Transform projectileSpawnPoint; // Spawn point for the projectile
        [SerializeField] private float projectileDamage = 25f; // Damage dealt by the projectile
        [SerializeField] private float explosionRadius = 2f; // Radius of the projectile's explosion

        private GameObject currentTargetingIndicator; // Current targeting indicator instance
        private Vector3 targetPosition; // Predicted target position
        private float lastAttackTime; // Time of the last attack

        private void Awake()
        {
            lastAttackTime = -attackCooldown; // Allow immediate first attack
        }

        // Returns the attack range
        public float GetAttackRange()
        {
            return attackRange;
        }

        // Returns the attack cooldown
        public float GetAttackCooldown()
        {
            return attackCooldown;
        }

        // Returns the targeting time
        public float GetTargetingTime()
        {
            return targetingTime;
        }

        // Checks if the enemy can attack
        public bool CanAttack()
        {
            return Time.time > lastAttackTime + attackCooldown;
        }

        // Checks if the player is within attack range
        public bool IsInAttackRange(float distance)
        {
            return distance <= attackRange && distance >= minAttackRange;
        }

        // Checks if the player is too close
        public bool IsPlayerTooClose(float distance)
        {
            return distance < minAttackRange;
        }

        // Coroutine to handle targeting and firing
        public IEnumerator TargetAndFire(Vector3 playerPos, Vector3 playerVelocity)
        {
            // Predict where the player will be
            targetPosition = PredictLandingPosition(playerPos, playerVelocity, targetingTime);

            // Show targeting indicator
            currentTargetingIndicator = Instantiate(targetingIndicatorPrefab,
                new Vector3(targetPosition.x, 0.1f, targetPosition.z),
                Quaternion.identity);

            // Wait while targeting
            yield return new WaitForSeconds(targetingTime);

            // Destroy the targeting indicator
            if (currentTargetingIndicator != null)
                Destroy(currentTargetingIndicator);
        }

        // Fires the mortar projectile
        public void FireProjectile()
        {
            // Launch projectile
            GameObject projectile = Instantiate(mortarProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
            Rigidbody projRb = projectile.GetComponent<Rigidbody>();

            if (projRb != null)
            {
                // Calculate trajectory to hit the target
                Vector3 launchVelocity = CalculateLaunchVelocity(projectileSpawnPoint.position, targetPosition, mortarSpeed);
                projRb.linearVelocity = launchVelocity;

                // Setup projectile
                MortarProjectile mortarProj = projectile.GetComponent<MortarProjectile>();
                if (mortarProj != null)
                {
                    mortarProj.damage = projectileDamage;
                    mortarProj.explosionRadius = explosionRadius;
                }
            }

            // Set cooldown
            lastAttackTime = Time.time;
        }

        // Predicts the landing position of the projectile
        private Vector3 PredictLandingPosition(Vector3 playerPos, Vector3 playerVelocity, float predictionTime)
        {
            // Simple prediction - assumes player keeps moving in the same direction/speed
            Vector3 predictedPos = playerPos + playerVelocity * predictionTime;

            // Add some randomness based on distance (less accuracy at greater distances)
            float distance = Vector3.Distance(transform.position, playerPos);
            float randomRadius = distance * 0.1f; // 10% of distance as randomness

            Vector2 randomOffset = Random.insideUnitCircle * randomRadius;
            predictedPos += new Vector3(randomOffset.x, 0, randomOffset.y);

            return predictedPos;
        }

        // Calculates the launch velocity for the projectile
        private Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 target, float speed)
        {
            // Get direction and distance
            Vector3 direction = target - start;
            float distance = direction.magnitude;

            // Calculate height based on distance (further = higher arc)
            float height = Mathf.Max(direction.y + distance * 0.5f, 2.0f);

            // Calculate gravity-based trajectory
            float gravity = Physics.gravity.magnitude;

            // Angle for reaching target
            float angle = 45f * Mathf.Deg2Rad; // 45 degrees in radians

            // Calculate velocity components
            float initialVelocity = Mathf.Sqrt(distance * gravity / Mathf.Sin(2 * angle));

            // Limit to max speed if necessary
            if (initialVelocity > speed)
                initialVelocity = speed;

            // Calculate velocity vector
            direction.y = 0; // Flatten direction for horizontal component
            direction.Normalize();
            Vector3 velocityVector = direction * initialVelocity * Mathf.Cos(angle);
            velocityVector.y = initialVelocity * Mathf.Sin(angle);

            return velocityVector;
        }

        // Optional: Visualization of attack and min attack ranges
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, minAttackRange);
        }
    }
}