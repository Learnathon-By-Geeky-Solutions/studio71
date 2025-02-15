using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public UnityEngine.UI.Image actorImage;
    public TextMeshProUGUI actorName;
    public TextMeshProUGUI messageText;
    public RectTransform backgroundBox;
    Message[] currentMessages;
    Actor[] currentActors;

    int activeMessages = 0;
    int currentActorId = -1; // Store the current actor ID
    public static bool isDialogueOpen = false;

    public void OpenDialogue(Message[] messages, Actor[] actors)
    {
        currentMessages = messages;
        currentActors = actors;
        activeMessages = 0;
        currentActorId = currentMessages[0].actorId;

        Debug.Log("Opening dialogue with " + messages.Length + " messages and " + actors.Length + " actors");

        LeanTween.scale(backgroundBox, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
        DisplayMessage();
    }

  void DisplayMessage()
{
    if (activeMessages < currentMessages.Length)
    {
        Message messageToDisplay = currentMessages[activeMessages];
        Actor actorToDisplay = currentActors[messageToDisplay.actorId];

        // Check if the actor has changed
        if (messageToDisplay.actorId != currentActorId)
        {
            currentActorId = messageToDisplay.actorId;
            // Move the dialogue box out and back in from the left
            LeanTween.moveX(backgroundBox, -backgroundBox.rect.width, 0.25f).setOnComplete(() =>
            {
                UpdateDialogueBox(actorToDisplay, messageToDisplay);
                // Set the starting position just off-screen to the left
                backgroundBox.anchoredPosition = new Vector2(-backgroundBox.rect.width, backgroundBox.anchoredPosition.y);
                // Move it to the intended position
                LeanTween.moveX(backgroundBox, 0, .8f).setEase(LeanTweenType.easeOutBack);
            });
        }
        else
        {
            UpdateDialogueBox(actorToDisplay, messageToDisplay);
        }

        isDialogueOpen = true;
    }
    else
    {
        Debug.Log("Dialogue finished");

        LeanTween.scale(backgroundBox, Vector3.zero, 0.5f).setEase(LeanTweenType.easeInBack);
        isDialogueOpen = false;
    }
}

    void UpdateDialogueBox(Actor actorToDisplay, Message messageToDisplay)
    {
        actorImage.sprite = actorToDisplay.sprite;
        actorName.text = actorToDisplay.name;
        messageText.text = messageToDisplay.message;
        StartCoroutine(TypeSentence(messageToDisplay.message));
    }

    IEnumerator TypeSentence(string sentence)
    {
        messageText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            messageText.text += letter;
            yield return new WaitForSeconds(0.05f); // Adjust speed here
        }
    }

    public void NextMessage()
    {
        activeMessages++;
        DisplayMessage();
    }

    void Start()
    {
        backgroundBox.transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isDialogueOpen)
        {
            NextMessage();
        }
    }
}