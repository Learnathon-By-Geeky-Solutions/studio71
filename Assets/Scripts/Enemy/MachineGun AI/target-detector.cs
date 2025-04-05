using UnityEngine;
using System.Collections.Generic;

// Optional advanced sensor component for machine gunner

namespace MachineGunAI
{
    public class MachineGunnerSensors : MonoBehaviour
{
    [SerializeField] private MachineGunnerAI gunner;
    [SerializeField] private float detectionInterval = 0.2f;
    [SerializeField] private float threatAssessmentRange = 50f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private LayerMask obstacleLayers;
    
    private Dictionary<Transform, float> knownThreats = new Dictionary<Transform, float>();
    private float detectionTimer = 0f;
    
    private void Update()
    {
        // Update detection timer
        detectionTimer += Time.deltaTime;
        
        // Perform detection at regular intervals
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            DetectThreats();
        }
    }
    
    private void DetectThreats()
    {
        // Clear old threats that haven't been seen in a while
        List<Transform> threatsToRemove = new List<Transform>();
        foreach (var threat in knownThreats.Keys)
        {
            // Increment time since last seen
            knownThreats[threat] += detectionInterval;
            
            // If too much time has passed, mark for removal
            if (knownThreats[threat] > 5.0f)
                threatsToRemove.Add(threat);
        }
        
        // Remove old threats
        foreach (var threat in threatsToRemove)
            knownThreats.Remove(threat);
        
        // Find all potential targets in range
        Collider[] hits = Physics.OverlapSphere(transform.position, threatAssessmentRange, targetLayers);
        
        // Process each potential target
        foreach (Collider hit in hits)
        {
            Transform target = hit.transform;
            
            // Check if we have line of sight
            Vector3 directionToTarget = target.position - transform.position;
            if (!Physics.Raycast(transform.position, directionToTarget.normalized, 
                                directionToTarget.magnitude, obstacleLayers))
            {
                // We can see this target, add or update in known threats
                knownThreats[target] = 0f;
                
                // Calculate threat level based on distance and other factors
                float distance = directionToTarget.magnitude;
                float threatLevel = CalculateThreatLevel(target, distance);
                
                // If this is the highest threat and we don't already have a target, set it
                if (gunner.Target == null && threatLevel > 0)
                {
                    gunner.SetTarget(target);
                }
            }
        }
    }
    
    private float CalculateThreatLevel(Transform target, float distance)
    {
        // Simple threat calculation based on distance (closer = higher threat)
        float distanceFactor = 1f - (distance / threatAssessmentRange);
        
        // Could add additional factors:
        // - Is target moving towards us?
        // - Is target currently attacking?
        // - Has target damaged us recently?
        // - Target type priority
        
        return distanceFactor;
    }
    
    // Called when the gunner takes damage
    public void ReportDamageFrom(Transform attacker)
    {
        // Immediately add attacker to known threats with high priority
        knownThreats[attacker] = 0f;
        
        // Set as current target with higher priority
        gunner.SetTarget(attacker);
    }
}

}
