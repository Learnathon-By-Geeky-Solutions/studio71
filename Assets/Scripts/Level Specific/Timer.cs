using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
namespace LevelSpecific
{
    public class Timer : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText; // Assign in inspector

        private float remainingTime;

        public float RemainingTime { get; private set; }

        public void StartTimer(float duration)
        {
            remainingTime = duration;
            StartCoroutine(TimerCoroutine());
        }

        private IEnumerator TimerCoroutine()
        {
            while (remainingTime > 0f)
            {
                UpdateTimerDisplay();
                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

            remainingTime = 0f; // Safety reset
            UpdateTimerDisplay(); // Show 00:00 at the end
        }

        private void UpdateTimerDisplay()
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}