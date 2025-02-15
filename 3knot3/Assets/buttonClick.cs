using UnityEngine;
using UnityEngine.UI;

public class ButtonKeyTrigger : MonoBehaviour
{
    public Button interactionButton; // Drag and drop your button in the Inspector

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // Check if Tab key is pressed
        {
            interactionButton.onClick.Invoke(); // Simulate button click
        }
    }
}
