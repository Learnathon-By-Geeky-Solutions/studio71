using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using DG.Tweening;
using SingletonManagers;
using TextProcessing;
using Player;

namespace dialogue
{
    /// <summary>
    /// Manages in-game dialogue using Ink narrative scripting system.
    /// Handles dialogue UI, character transitions, and player choice interactions.
    /// </summary>
    public class InkDialogueManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI ActorName;
        [SerializeField] private Animator Avatar;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject choicePanel;
        [SerializeField] private GameObject[] choiceButtons;

        [Header("Animation Settings")]
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private float slideOffset = 1000f;
        [SerializeField] private Ease easeType = Ease.OutQuint;
        [SerializeField] private float bounceStrength = 1.1f;

        [Header("Character Data")]
        [SerializeField] private CharacterData[] characters;
        #endregion

        #region Private Fields
        private RectTransform dialoguePanelRect;
        private Vector2 dialoguePanelOriginalPos;
        private Vector3 dialoguePanelOriginalScale;
        private int currentCharacterId = -1;
        private Dictionary<int, CharacterData> characterDictionary;
        private Story story;
        private bool canContinueToNextLine = true;
        private PlayerController _playerController;
        private PlayerAnimation _playerAnimation;
        #endregion

        #region Public Properties
        /// <summary>
        /// Indicates whether a dialogue is currently active.
        /// </summary>
        public static bool IsDialogueOpen { get; private set; } = false;
        #endregion

        #region Data Classes
        [System.Serializable]
        public class CharacterData
        {
            public int id;
            public string characterName;
            public string animationState;
        }
        #endregion

        #region Unity Lifecycle Methods
        private void Awake()
        {
            InitializeCharacterDictionary();
            InitializeDialoguePanel();
            FindPlayerReferences();
        }

        private void Start()
        {
            ValidateComponents();
            HideDialogueUI();
        }

        private void OnEnable()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnInteract += LetsContinueStory;
            }
            else
            {
                Debug.LogWarning("InputHandler instance not found. Dialogue continuation via input will not work.");
            }
        }

        private void OnDisable()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnInteract -= LetsContinueStory;
            }
        }
        #endregion

        #region Initialization Methods
        private void InitializeCharacterDictionary()
        {
            characterDictionary = new Dictionary<int, CharacterData>();
            foreach (var character in characters)
            {
                characterDictionary[character.id] = character;
            }
        }

        private void InitializeDialoguePanel()
        {
            if (dialoguePanel == null)
            {
                Debug.LogError("Dialogue panel reference is missing!", this);
                return;
            }

            dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
            dialoguePanelOriginalPos = dialoguePanelRect.anchoredPosition;
            dialoguePanelOriginalScale = dialoguePanelRect.localScale;
        }

        private void FindPlayerReferences()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerController = player.GetComponent<PlayerController>();
                _playerAnimation = player.GetComponent<PlayerAnimation>();
            }
            else
            {
                Debug.LogWarning("Player not found. Dialogue system won't be able to control player during dialogue.");
            }
        }

        private void ValidateComponents()
        {
            if (dialogueText == null) Debug.LogError("Dialogue text component is missing!", this);
            if (ActorName == null) Debug.LogError("Actor name component is missing!", this);
            if (Avatar == null) Debug.LogError("Avatar animator component is missing!", this);
        }

        private void HideDialogueUI()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (choicePanel != null) choicePanel.SetActive(false);
        }
        #endregion

        #region Public Dialogue Methods
        /// <summary>
        /// Starts a dialogue sequence using the provided Ink JSON file.
        /// </summary>
        /// <param name="inkJSON">The Ink JSON file containing the dialogue.</param>
        public void StartDialogue(TextAsset inkJSON)
        {
            if (inkJSON == null)
            {
                Debug.LogError("Cannot start dialogue with null Ink JSON file.", this);
                return;
            }

            // Reset character and create new story
            currentCharacterId = -1;
            story = new Story(inkJSON.text);

            // Show and position dialogue panel with animation
            ShowDialoguePanel();

            // Update dialogue state
            IsDialogueOpen = true;
            DisablePlayerControl();
            ContinueStory();
        }

        /// <summary>
        /// Advances the dialogue to the next line or choice.
        /// </summary>
        public void ContinueStory()
        {
            if (story == null) return;

            if (story.canContinue)
            {
                string text = story.Continue();
                text = BanglaTextFixer.ApplyTextFix(text);
                HandleTags(story.currentTags);
                
                if (dialogueText != null)
                {
                    dialogueText.text = text;
                }

                if (story.currentChoices.Count > 0)
                {
                    DisplayChoices();
                    canContinueToNextLine = false;
                }
                else
                {
                    canContinueToNextLine = true;
                    HideChoices();
                }
            }
            else
            {
                StartCoroutine(EndDialogue());
            }
        }

        /// <summary>
        /// Selects a choice option and continues the story.
        /// </summary>
        /// <param name="choiceIndex">The index of the chosen option.</param>
        public void ChooseOption(int choiceIndex)
        {
            if (story == null) return;
            
            story.ChooseChoiceIndex(choiceIndex);
            canContinueToNextLine = true;
            HideChoices();
            ContinueStory();
        }
        #endregion

        #region Private Dialogue Management Methods
        private void LetsContinueStory()
        {
            if (IsDialogueOpen && canContinueToNextLine)
            {
                ContinueStory();
            }
        }

        private void HandleTags(List<string> tags)
        {
            foreach (string tag in tags)
            {
                string[] splitTag = tag.Split(':');
                if (splitTag.Length != 2) continue;

                string tagKey = splitTag[0].Trim();
                string tagValue = splitTag[1].Trim();

                if (tagKey == "id" && int.TryParse(tagValue, out int characterId) && (currentCharacterId != characterId))
                {
                    TransitionToNewCharacter(characterId);
                }
            }
        }

        private void TransitionToNewCharacter(int newCharacterId)
        {
            if (dialoguePanelRect == null) return;
            
            dialoguePanelRect.DOKill();
            Sequence sequence = DOTween.Sequence();

            if (currentCharacterId != -1)
            {
                // Just scale to zero instead of sliding right
                sequence.Append(dialoguePanelRect.DOScale(Vector3.zero, .1f));
            }

            currentCharacterId = newCharacterId;
            if (characterDictionary.TryGetValue(newCharacterId, out CharacterData character))
            {
                if (ActorName != null)
                {
                    ActorName.text = BanglaTextFixer.ApplyTextFix(character.characterName);
                }
                
                if (Avatar != null)
                {
                    if (string.IsNullOrEmpty(character.animationState))
                    {
                        Debug.LogWarning($"No animation state defined for character {character.characterName}", this);
                    }
                    else
                    {
                        Avatar.Play(character.animationState);
                    }
                }
            }

            sequence.AppendCallback(() =>
            {
                // Prepare the new panel to come in from the left
                dialoguePanelRect.anchoredPosition = new Vector2(-slideOffset, dialoguePanelOriginalPos.y);
                dialoguePanelRect.localScale = Vector3.zero;
            });

            // Animate the new panel in
            sequence.Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale * bounceStrength, animationDuration * 0.3f))
                   .Join(dialoguePanelRect.DOAnchorPosX(dialoguePanelOriginalPos.x, animationDuration * 0.3f))
                   .Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale, animationDuration * 0.2f));
        }

        private void ShowDialoguePanel()
        {
            if (dialoguePanel == null || dialoguePanelRect == null) return;
            
            dialoguePanel.SetActive(true);
            dialoguePanelRect.localScale = Vector3.zero;
            dialoguePanelRect.anchoredPosition = new Vector2(-slideOffset, dialoguePanelOriginalPos.y);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale * bounceStrength, animationDuration * 0.6f))
                   .Join(dialoguePanelRect.DOAnchorPosX(dialoguePanelOriginalPos.x, animationDuration * 0.6f))
                   .Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale, animationDuration * 0.4f))
                   .SetEase(easeType);
        }

        private void DisablePlayerControl()
        {
            if (_playerController != null)
            {
                _playerController.enabled = false;
            }
            
            if (_playerAnimation != null)
            {
                _playerAnimation.enabled = false;
            }
        }

        private void EnablePlayerControl()
        {
            if (_playerController != null)
            {
                _playerController.enabled = true;
            }
            
            if (_playerAnimation != null)
            {
                _playerAnimation.enabled = true;
            }
        }
        #endregion

        #region Choice Management
        private void DisplayChoices()
        {
            if (story == null || choicePanel == null) return;
            
            List<Choice> choices = story.currentChoices;

            // Show and animate the choice panel
            choicePanel.SetActive(true);
            RectTransform choicePanelRect = choicePanel.GetComponent<RectTransform>();
            choicePanelRect.localScale = Vector3.zero;
            choicePanelRect.DOScale(1f, animationDuration * 0.5f).SetEase(Ease.OutBack);

            // Hide all buttons first
            foreach (GameObject button in choiceButtons)
            {
                if (button != null)
                {
                    button.SetActive(false);
                }
            }

            // Show and set up choice buttons
            for (int i = 0; i < choices.Count && i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null) continue;
                
                choiceButtons[i].SetActive(true);
                
                RectTransform buttonRect = choiceButtons[i].GetComponent<RectTransform>();
                buttonRect.localScale = Vector3.zero;
                buttonRect.DOScale(1f, animationDuration * 0.5f)
                         .SetEase(Ease.OutBack)
                         .SetDelay(i * 0.1f);

                TextMeshProUGUI choiceText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (choiceText != null)
                {
                    choiceText.text = BanglaTextFixer.ApplyTextFix(choices[i].text);
                }

                int choiceIndex = i;
                Button button = choiceButtons[i].GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => ChooseOption(choiceIndex));
                }
            }
        }

        private void HideChoices()
        {
            if (choicePanel == null) return;
            
            RectTransform choicePanelRect = choicePanel.GetComponent<RectTransform>();
            choicePanelRect.DOScale(0f, animationDuration * 0.3f)
                          .SetEase(Ease.InBack)
                          .OnComplete(() => choicePanel.SetActive(false));
        }
        #endregion

        #region Dialogue Ending
        private IEnumerator EndDialogue()
        {
            if (dialoguePanelRect == null) yield break;
            
            Sequence endSequence = DOTween.Sequence();
            
            endSequence.Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale * 1.1f, animationDuration * 0.3f))
                      .Append(dialoguePanelRect.DOScale(0f, animationDuration * 0.3f))
                      .Join(dialoguePanelRect.DOAnchorPosX(slideOffset, animationDuration * 0.3f))
                      .OnComplete(() =>
                      {
                          if (dialoguePanel != null) dialoguePanel.SetActive(false);
                          if (choicePanel != null) choicePanel.SetActive(false);
                      });

            IsDialogueOpen = false;
            EnablePlayerControl();
            yield return new WaitForSeconds(animationDuration);
        }
        #endregion
    }
}