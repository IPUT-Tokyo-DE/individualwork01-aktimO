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

        if (GUILayout.Button("�g�p���������������i�S����j"))
        {
            foreach (var topic in dictionary.topics)
            {
                topic.GenerateAvailableCharacters();
            }

            EditorUtility.SetDirty(dictionary);
            Debug.Log("�g�p������������������܂����B");
        }
    }
}
