using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private Gun _equippedGun;
    // Start is called once before the first execution of Update after the MonoBehaviour is create
    public void OnFire(InputAction.CallbackContext trigger)
    {
        if (trigger.started || trigger.performed)
        {
            _equippedGun.StartShooting();
        }
        else if(trigger.canceled)
        { 
            _equippedGun.StopShooting();        
        }
    }    
}
