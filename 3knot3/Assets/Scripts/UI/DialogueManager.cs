using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Runtime.CompilerServices;
using SingletonManagers;
using UnityEngine.InputSystem;
namespace UI.DialogueSystem
{

    public class DialogueManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Image actorImage;
        [SerializeField] private TextMeshProUGUI actorNameText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private RectTransform dialogueBox;

        private Message[] currentMessages;
        private Actor[] currentActors;
        private int activeMessageIndex = 0;
        private int currentActorId = -1;

        public static bool IsDialogueOpen { get; private set; } = false;

        private void OnEnable()
        {
            InputHandler.Instance.OnInteract += NextDialogue;
        }

        private void OnDisable()
        {
            InputHandler.Instance.OnInteract -= NextDialogue;
            
        }


        private void Start()
        {
            // Start with dialogue box off-screen (hidden)
            dialogueBox.localScale = Vector3.zero;
        }

        public void OpenDialogue(Message[] messages, Actor[] actors)
        {
            if (messages == null || actors == null || messages.Length == 0) return;

            currentMessages = messages;
            currentActors = actors;
            activeMessageIndex = 0;
            currentActorId = currentMessages[activeMessageIndex].ActorId; // Reset actor ID to ensure proper transition


            IsDialogueOpen = true;

            Debug.Log($"Opening dialogue with {messages.Length} messages and {actors.Length} actors");

            // Animate dialogue box appearing
            dialogueBox.DOScale(1, 0.5f).SetEase(Ease.OutBack);
            DisplayMessage();
        }


        private void DisplayMessage()
        {
            if (activeMessageIndex >= currentMessages.Length)
            {
                CloseDialogue();
                return;
            }

            Message messageToDisplay = currentMessages[activeMessageIndex];
            Actor actorToDisplay = currentActors[messageToDisplay.ActorId];

            if (messageToDisplay.ActorId != currentActorId)
            {
                currentActorId = messageToDisplay.ActorId;
                float originalX = dialogueBox.position.x;

                // Animate transition when actor changes
                dialogueBox.DOAnchorPosX(-dialogueBox.rect.width, 0.3f).OnComplete(() =>
                {
                    UpdateDialogueBox(actorToDisplay, messageToDisplay);
                    dialogueBox.DOMoveX(originalX, 0.5f).SetEase(Ease.OutSine);
                });
            }
            else
            {
                UpdateDialogueBox(actorToDisplay, messageToDisplay);
            }
        }

        private void UpdateDialogueBox(Actor actor, Message message)
        {
            actorImage.sprite = actor.Sprite;
            actorNameText.text = actor.Name;
            StartCoroutine(TypeText(message.Dialogue));
        }

        private IEnumerator TypeText(string text)
        {
            messageText.text = "";
            foreach (char letter in text)
            {
                messageText.text += letter;
                yield return new WaitForSeconds(0.05f); // Adjust typing speed
            }
        }

        public void NextMessage()
        {
            activeMessageIndex++;
            StopAllCoroutines();
            DisplayMessage();
        }

        private void CloseDialogue()
        {
            Debug.Log("Dialogue finished");

            dialogueBox.DOScale(0, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                IsDialogueOpen = false;
            });
        }

        private void NextDialogue()
        {
            if (IsDialogueOpen)
            {
                NextMessage();
            }
        }


    }
}
