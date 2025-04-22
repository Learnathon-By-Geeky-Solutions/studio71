using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Weapon;
namespace UI.HUD{
public class BulletCountUI : MonoBehaviour
{
    [SerializeField] private Gun gunReference;
    [SerializeField] private Transform bulletIconContainer;
    [SerializeField] private GameObject bulletIconPrefab;
    
    [SerializeField] private Vector2 iconSize = new Vector2(20, 20);
    
    private readonly List<GameObject> bulletIcons = new List<GameObject>();
    private int lastBulletCount = -1;

    private void Start()
    {
        if (gunReference == null)
        {
            Debug.LogError("Gun reference not assigned to BulletCountUI!");
            return;
        }

        // Initialize bullet icons for maximum magazine size
        CreateBulletIcons();
        
        // Update once at start
        UpdateBulletUI();
    }

    private void Update()
    {
        // Only update UI when bullet count changes
        if (lastBulletCount != gunReference.CurrentMagazineSize)
        {
            UpdateBulletUI();
        }
    }

    private void CreateBulletIcons()
    {
        // Clear any existing icons
        foreach (var icon in bulletIcons)
        {
            if (icon != null)
                Destroy(icon);
        }
        bulletIcons.Clear();

        // Create new icons
        for (int i = 0; i < gunReference.Magazine_Size; i++)
        {
            GameObject newIcon = Instantiate(bulletIconPrefab, bulletIconContainer);
            RectTransform rectTransform = newIcon.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = iconSize;
            }
            
            bulletIcons.Add(newIcon);
        }
    }

    private void UpdateBulletUI()
    {
        // Make sure we have the right number of icons
        if (bulletIcons.Count != gunReference.Magazine_Size)
        {
            CreateBulletIcons();
        }

        // Update visibility of icons based on current magazine size
        for (int i = 0; i < bulletIcons.Count; i++)
        {
            bulletIcons[i].SetActive(i < gunReference.CurrentMagazineSize);
        }

        lastBulletCount = gunReference.CurrentMagazineSize;
    }
    }
}