using UnityEngine;
using System.Collections.Generic;

namespace PatrolEnemy
{
    public class TargetSwapper : MonoBehaviour
    {
        private List<Transform> potentialTargets = new List<Transform>();
        private Transform currentTarget;
        
        [SerializeField] private string targetTag = "Player";
        [SerializeField] private float targetUpdateFrequency = 0.5f;
        private float targetUpdateTimer;
        
        private void Start()
        {
            targetUpdateTimer = targetUpdateFrequency;
        }
        
        private void Update()
        {
            targetUpdateTimer -= Time.deltaTime;
            
            if (targetUpdateTimer <= 0)
            {
                RefreshTargets();
                targetUpdateTimer = targetUpdateFrequency;
            }
        }
        
        private void RefreshTargets()
        {
            // Find all players in the scene
            GameObject[] players = GameObject.FindGameObjectsWithTag(targetTag);
            potentialTargets.Clear();
            
            foreach (GameObject player in players)
            {
                potentialTargets.Add(player.transform);
            }
        }
        
        public Transform GetClosestTarget(Vector3 position)
        {
            if (potentialTargets.Count == 0)
                return null;
                
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (Transform target in potentialTargets)
            {
                if (target == null)
                    continue;
                    
                float distance = Vector3.Distance(position, target.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = target;
                }
            }
            
            currentTarget = closestTarget;
            return currentTarget;
        }
        
        public Transform GetCurrentTarget()
        {
            return currentTarget;
        }
    }
}
