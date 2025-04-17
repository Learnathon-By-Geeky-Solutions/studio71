using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TextProcessing
{
    [System.Serializable]
    public class ReplaceCharacterData
    {
        public char CharacterToReplace;
        public char ReplacedPrefixCharacter;
        public char ReplacedSuffixCharacter;
    }

    public class BanglaTextFixer : Singleton.SingletonPersistent
    {
        public new static BanglaTextFixer Instance { get; private set; }

        [SerializeField]
        private string characterPrefixFix;
        public string CharacterPrefixFix
        {
            get => characterPrefixFix;
            set => characterPrefixFix = value;
        }

        [SerializeField]
        private List<ReplaceCharacterData> characterToReplace;
        public List<ReplaceCharacterData> CharacterToReplace
        {
            get => characterToReplace;
            set => characterToReplace = value;
        }

        [SerializeField]
        private string characterToIgnore;
        public string CharacterToIgnore
        {
            get => characterToIgnore;
            set => characterToIgnore = value;
        }

        protected override void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public string FixBanglaText(string text)
        {
            return FixTextOrder(text);
        }

        private string FixTextOrder(string inputText)
        {
            List<char> newString = new List<char>();

            foreach (var c in inputText)
            {
                Debug.Log($"Found character {c} and ASCII code {(int)c}");

                if (IsCharacterMatched(CharacterPrefixFix, c))
                {
                    int indent = FindSwapIndex(newString);
                    newString.Insert(indent, c);
                }
                else
                {
                    var characterMatched = CharacterToReplace.FirstOrDefault(x => x.CharacterToReplace.Equals(c));
                    if (characterMatched != null)
                    {
                        int indent = FindSwapIndex(newString);
                        newString.Add(characterMatched.ReplacedSuffixCharacter);
                        newString.Insert(indent, characterMatched.ReplacedPrefixCharacter);
                    }
                    else
                    {
                        newString.Add(c);
                    }
                }
            }

            return new string(newString.ToArray());
        }

        private static bool IsCharacterMatched(string checkedAgainst, char c)
        {
            return checkedAgainst.Contains(c);
        }

        private int FindSwapIndex(List<char> sourceText)
        {
            int indent = 0;
            int startingIndex = sourceText.Count - 1;

            for (int i = startingIndex; i >= 0; i--)
            {
                if (i == 0) break;
                if (IsCharacterMatched(CharacterToIgnore, sourceText[i])) continue;
                if (IsCharacterMatched(CharacterToIgnore, sourceText[i - 1]) && !IsCharacterMatched(CharacterPrefixFix, sourceText[i - 1])) continue;
                indent = i;
                break;
            }

            return indent;
        }

        /// <summary>
        /// Safely applies Bangla text fixing using the Singleton instance.
        /// Returns the original text if the instance is not available.
        /// </summary>
        /// <param name="originalText">The text to fix.</param>
        /// <returns>The fixed text, or the original text if fixing is not possible.</returns>
        public static string ApplyTextFix(string originalText)
        {
            if (string.IsNullOrEmpty(originalText)) 
            {
                return ""; // Return empty for null/empty input
            }
            
            if (Instance != null) 
            {
                return Instance.FixBanglaText(originalText); // Use the instance method
            }
            else 
            {
                Debug.LogWarning("BanglaTextFixer instance not found. Returning original text.");
                return originalText; // Return original if instance is missing
            }
        }
    }
}