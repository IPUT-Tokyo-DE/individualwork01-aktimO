using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordDictionary", menuName = "ScriptableObjects/WordDictionary")]
public class WordDictionary : ScriptableObject
{
    public static WordDictionary Instance { get; private set; }

    public List<TopicEntry> topics;

    private void OnEnable()
    {
        // �C���X�^���X���܂����݂��Ȃ��ꍇ�ɂ̂ݐݒ�
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("WordDictionary�̃C���X�^���X�͂��łɑ��݂��Ă��܂��I");
        }
    }
}

[System.Serializable]
public class TopicEntry
{
    public string topicName;
    public List<string> wordList = new();
    public List<string> availableCharacters = new();

    public void GenerateAvailableCharacters()
    {
        HashSet<string> chars = new();

        foreach (string word in wordList)
        {
            foreach (char c in word)
            {
                chars.Add(c.ToString());
            }
        }

        availableCharacters = new List<string>(chars);
    }
}
