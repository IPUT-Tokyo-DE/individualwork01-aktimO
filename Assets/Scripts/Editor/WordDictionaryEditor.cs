// Scripts/Editor/WordDictionaryEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WordDictionary))]
public class WordDictionaryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WordDictionary dictionary = (WordDictionary)target;

        if (GUILayout.Button("使用文字を自動生成（全お題）"))
        {
            foreach (var topic in dictionary.topics)
            {
                topic.GenerateAvailableCharacters();
            }

            EditorUtility.SetDirty(dictionary);
            Debug.Log("使用文字が自動生成されました。");
        }
    }
}
