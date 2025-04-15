using UnityEngine;
using DG.Tweening;

namespace UI.MainMenu
{
    public class WindManager : MonoBehaviour
    {
        [Header("Wind Settings")]
        [SerializeField] private Transform[] objectsToSway; // Array of objects affected by wind
        [SerializeField] private float minSwayAmount = 5f; // Minimum sway angle
        [SerializeField] private float maxSwayAmount = 15f; // Maximum sway angle
        [SerializeField] private float minSwayDuration = 1.5f; // Minimum duration for sway
        [SerializeField] private float maxSwayDuration = 3f; // Maximum duration for sway
        [SerializeField] private float minDelay = 0f; // Minimum delay before sway starts
        [SerializeField] private float maxDelay = 1f; // Maximum delay before sway starts

        private void Start()
        {
            ApplyWindEffect();
        }

        private void ApplyWindEffect()
        {
            foreach (Transform obj in objectsToSway)
            {
                // Generate random values for sway amount, duration, and delay
                float randomSwayAmount = Random.Range(minSwayAmount, maxSwayAmount);
                float randomDuration = Random.Range(minSwayDuration, maxSwayDuration);
                float randomDelay = Random.Range(minDelay, maxDelay);

                // Animate rotation with random parameters
                obj.DORotate(new Vector3(0, 0, randomSwayAmount), randomDuration)
                   .SetEase(Ease.InOutSine) // Smooth easing for natural movement
                   .SetLoops(-1, LoopType.Yoyo) // Infinite looping with Yoyo effect
                   .SetDelay(randomDelay); // Add random delay
            }
        }
    }
}
