using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{   public Image actorImage;
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI messageText;
    public RectTransform backgroundBox;
    Message[]currentMessages;
    Actor[]currentActors;
    int activeMessages=0;
    public static bool isActive=false;
    public void openDialogue(Message[]messages,Actor[]actors)
    {
        currentMessages=messages;
        currentActors=actors;
        activeMessages=0;
        isActive=true;
        Debug.Log("Opening dialogue"+currentMessages.Length);
        DisplayMessage();
    }
    void DisplayMessage()
    {
        Message messageToDisplay=currentMessages[activeMessages];
        messageText.text=messageToDisplay.text;
        Actor actorToDisplay=currentActors[messageToDisplay.actorId];
        actorImage.sprite=actorToDisplay.sprite;
        actorName.text=actorToDisplay.name;

    }
    public void NextMessage()
    {
        activeMessages++;
        if(activeMessages<currentMessages.Length)
        {
            DisplayMessage();
        }
        else
        {
            Debug.Log("End of dialogue");
            isActive=false;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)  && isActive)
        {
            NextMessage();
        }
    }
}
