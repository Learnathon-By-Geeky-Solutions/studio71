using System;
using UnityEngine;

namespace MortarAI
{
    public static class GameEvents
    {
        // Player events
        public static event Action<Vector3, Vector3> OnPlayerPositionUpdated; // Position, Velocity

        // Publish player position and velocity
        public static void PublishPlayerPosition(Vector3 position, Vector3 velocity)
        {
            OnPlayerPositionUpdated?.Invoke(position, velocity);
        }
    }
}