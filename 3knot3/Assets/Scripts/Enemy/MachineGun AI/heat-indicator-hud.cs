using UnityEngine;
using UnityEngine.UI;

// Optional HUD component to display machine gunner status

namespace MachineGunAI
{
    public class MachineGunnerHUD : MonoBehaviour
    {
        [SerializeField] private MachineGunnerAI gunner;
        [SerializeField] private Image heatFillBar;
        [SerializeField] private Image ammoFillBar;
        [SerializeField] private Text stateText;
        [SerializeField] private GameObject warningIcon;

        private void Update()
        {
            // Only update if all UI elements and gunner exist
            if (gunner == null || heatFillBar == null || ammoFillBar == null || stateText == null)
                return;

            // Update heat indicator
            float heatPercentage = gunner.CurrentHeat / gunner.HeatThreshold;
            heatFillBar.fillAmount = heatPercentage;

            // Change heat bar color based on temperature
            if (heatPercentage < 0.5f)
                heatFillBar.color = Color.green;
            else if (heatPercentage < 0.8f)
                heatFillBar.color = Color.yellow;
            else
                heatFillBar.color = Color.red;

            // Update ammo indicator (assuming magazine size is constant)
            float ammoPercentage = (float)gunner.CurrentAmmo / 100f; // Using 100 as default magazine size
            ammoFillBar.fillAmount = ammoPercentage;

            // Show warning icon when heat is high or ammo is low
            warningIcon.SetActive(heatPercentage > 0.8f || ammoPercentage < 0.2f);

            // Update state text
            IMachineGunnerState currentState = GetCurrentState();
            if (currentState != null)
            {
                string stateName = currentState.GetType().Name;
                stateText.text = "State: " + stateName.Replace("State", "");
            }
        }

        private IMachineGunnerState GetCurrentState()
        {
            // This is a workaround since we don't have direct access to currentState
            // A better approach would be to expose a public property in MachineGunnerAI

            // Try to infer state from gunner properties
            if (gunner.CurrentHeat >= gunner.HeatThreshold)
                return gunner.overheatedState;

            if (gunner.CurrentAmmo <= 0)
                return gunner.reloadState;

            // For other states, we can't easily determine
            return null;
        }
    }
}