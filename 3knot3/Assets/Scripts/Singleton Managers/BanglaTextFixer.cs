using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Singleton;

 public class BanglaTextFixer : SingletonPersistent<BanglaTextFixer>
    {
       
    
        public new static BanglaTextFixer Instance { get; private set; }

        public string CharacterPrefixFix;
        public List<ReplaceCharacterData> CharacterToReplace;
        public string CharacterToIgnore;

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
                else if (CharacterToReplace.FirstOrDefault(x => x.CharacterToReplace.Equals(c)) != null)
                {
                    var characterMatched = CharacterToReplace.FirstOrDefault(x => x.CharacterToReplace.Equals(c));
                    int indent = FindSwapIndex(newString);
                    newString.Add(characterMatched.ReplacedSuffixCharacter);
                    newString.Insert(indent, characterMatched.ReplacedPrefixCharacter);
                }
                else
                {
                    newString.Add(c);
                }
            }

            return new string(newString.ToArray());
        }

        private bool IsCharacterMatched(string checkedAgainst, char c)
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
    }

    [System.Serializable]
    public class ReplaceCharacterData
    {
        public char CharacterToReplace;
        public char ReplacedPrefixCharacter;
        public char ReplacedSuffixCharacter;
    }
