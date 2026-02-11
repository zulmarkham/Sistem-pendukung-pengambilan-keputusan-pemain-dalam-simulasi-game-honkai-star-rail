#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(loaddata))]
public class CharacterLoaderEditor : Editor
{
    List<string> characterNames = new List<string>();
    int selectedIndex = 0;

    public override void OnInspectorGUI()
    {
        loaddata loader = (loaddata)target;

        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("textFile"));

        if (loader.textFile != null)
        {
            ParseCharacterNames(loader.textFile.text);

            if (characterNames.Count > 0)
            {
                selectedIndex = characterNames.IndexOf(loader.selectedCharacterName);
                if (selectedIndex < 0) selectedIndex = 0;

                selectedIndex = EditorGUILayout.Popup("Selected Character", selectedIndex, characterNames.ToArray());
                loader.selectedCharacterName = characterNames[selectedIndex];

                if (GUILayout.Button("Load Character"))
                {
                    loader.LoadSelectedCharacter();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Tidak ada karakter ditemukan dalam file.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Character Stats", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Name", loader.name);
        EditorGUILayout.FloatField("Base HP", loader.baseHP);
        EditorGUILayout.FloatField("Current HP", loader.currentHP);
        EditorGUILayout.FloatField("Base DEF", loader.baseDEF);
        EditorGUILayout.FloatField("Base ATK", loader.baseATK);

        serializedObject.ApplyModifiedProperties();
    }

    void ParseCharacterNames(string text)
    {
        characterNames.Clear();
        string[] lines = text.Split('\n');
        foreach (string line in lines)
        {
            if (line.Trim().StartsWith("name:"))
            {
                string name = line.Split(':')[1].Trim();
                characterNames.Add(name);
            }
        }
    }
}
#endif
