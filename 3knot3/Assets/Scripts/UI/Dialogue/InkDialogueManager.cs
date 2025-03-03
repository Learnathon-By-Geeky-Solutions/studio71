using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using DG.Tweening;
using SingletonManagers;
using TextProcessing;
namespace dialogue{
public class InkDialogueManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI ActorName;
    [SerializeField] private Image Avatar;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject choicePanel;
    [SerializeField] private GameObject[] choiceButtons;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float slideOffset = 1000f;
    [SerializeField] private Ease easeType = Ease.OutQuint;
    [SerializeField] private float bounceStrength = 1.1f;

    private RectTransform dialoguePanelRect;
    private Vector2 dialoguePanelOriginalPos;
    private Vector3 dialoguePanelOriginalScale;
    private int currentCharacterId = -1;

    [System.Serializable]
    public class CharacterData
    {
        public int id;
        public string characterName;
        public Sprite characterSprite;
    }

    [SerializeField] private CharacterData[] characters;
    private Dictionary<int, CharacterData> characterDictionary;
    private Story story;
    public static bool IsDialogueOpen { get; private set; } = false;
    private bool canContinueToNextLine = true;

    void Awake()
    {
        characterDictionary = new Dictionary<int, CharacterData>();
        foreach (var character in characters)
        {
            characterDictionary[character.id] = character;
        }

        dialoguePanelRect = dialoguePanel.GetComponent<RectTransform>();
        dialoguePanelOriginalPos = dialoguePanelRect.anchoredPosition;
        dialoguePanelOriginalScale = dialoguePanelRect.localScale;
    }

    void Start()
    {
        dialoguePanel.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
    }
    private void OnEnable()
    {
        InputHandler.Instance.OnInteract += LetsContinueStory;
    }
    private void OnDisable()
    {
        InputHandler.Instance.OnInteract -= LetsContinueStory;
    }
    void LetsContinueStory()
    {
        if (IsDialogueOpen && canContinueToNextLine)
        {
            ContinueStory();
        }
    }
    

    public void StartDialogue(TextAsset inkJSON)
    {
        if (inkJSON != null)
        {
            currentCharacterId = -1;
            story = new Story(inkJSON.text);

            dialoguePanel.SetActive(true);
            dialoguePanelRect.localScale = Vector3.zero;
            dialoguePanelRect.anchoredPosition = new Vector2(-slideOffset, dialoguePanelOriginalPos.y);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale * bounceStrength, animationDuration * 0.6f))
                   .Join(dialoguePanelRect.DOAnchorPosX(dialoguePanelOriginalPos.x, animationDuration * 0.6f))
                   .Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale, animationDuration * 0.4f))
                   .SetEase(easeType);

            IsDialogueOpen = true;
            ContinueStory();
        }
    }

    public void ContinueStory()
    {
        if (story.canContinue)
        {
            string text = story.Continue();
            text = BanglaTextFixer.Instance.FixBanglaText(text);
            HandleTags(story.currentTags);
            dialogueText.text = text;

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
        ActorName.text = BanglaTextFixer.Instance.FixBanglaText(character.characterName);
        Avatar.sprite = character.characterSprite;
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

    void DisplayChoices()
    {
        List<Choice> choices = story.currentChoices;

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
            RectTransform choicePanelRect = choicePanel.GetComponent<RectTransform>();
            choicePanelRect.localScale = Vector3.zero;
            choicePanelRect.DOScale(1f, animationDuration * 0.5f).SetEase(Ease.OutBack);
        }

        foreach (GameObject button in choiceButtons)
        {
            button.SetActive(false);
        }

        for (int i = 0; i < choices.Count && i < choiceButtons.Length; i++)
        {
            choiceButtons[i].SetActive(true);
            
            RectTransform buttonRect = choiceButtons[i].GetComponent<RectTransform>();
            buttonRect.localScale = Vector3.zero;
            buttonRect.DOScale(1f, animationDuration * 0.5f)
                     .SetEase(Ease.OutBack)
                     .SetDelay(i * 0.1f);

            TextMeshProUGUI choiceText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (choiceText != null)
            {
                choiceText.text = BanglaTextFixer.Instance.FixBanglaText(choices[i].text);
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

    void HideChoices()
    {
        if (choicePanel != null)
        {
            RectTransform choicePanelRect = choicePanel.GetComponent<RectTransform>();
            choicePanelRect.DOScale(0f, animationDuration * 0.3f)
                          .SetEase(Ease.InBack)
                          .OnComplete(() => choicePanel.SetActive(false));
        }
    }

    public void ChooseOption(int choiceIndex)
    {
        story.ChooseChoiceIndex(choiceIndex);
        canContinueToNextLine = true;
        HideChoices();
        ContinueStory();
    }

    public IEnumerator EndDialogue()
    {
        Sequence endSequence = DOTween.Sequence();
        
        endSequence.Append(dialoguePanelRect.DOScale(dialoguePanelOriginalScale * 1.1f, animationDuration * 0.3f))
                  .Append(dialoguePanelRect.DOScale(0f, animationDuration * 0.3f))
                  .Join(dialoguePanelRect.DOAnchorPosX(slideOffset, animationDuration * 0.3f))
                  .OnComplete(() =>
                  {
                      dialoguePanel.SetActive(false);
                      if (choicePanel != null) choicePanel.SetActive(false);
                  });

        IsDialogueOpen = false;
        yield return new WaitForSeconds(animationDuration);
    }
}
}