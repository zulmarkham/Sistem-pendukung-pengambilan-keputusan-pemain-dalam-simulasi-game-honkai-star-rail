#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[CustomEditor(typeof(HeroStateMachine))]
public class HeroStateMachineEditor : Editor
{
    SerializedProperty heroProp;
    SerializedProperty heroNameProp;

    void OnEnable()
    {
        heroProp = serializedObject.FindProperty("hero");
        if (heroProp != null)
            heroNameProp = heroProp.FindPropertyRelative("characterName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        HeroStateMachine sm = (HeroStateMachine)target;

        DrawDropdown(sm);

        // Draw the hero field and other properties so inspector remains usable
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Hero Data", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(heroProp, true);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDropdown(HeroStateMachine sm)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Select Character (Editor)", EditorStyles.boldLabel);

        string[] names = TryGetCharacterNames();
        if (names == null || names.Length == 0)
        {
            EditorGUILayout.HelpBox("No character CSV found. Put CSV at Resources/Data/data_character.csv or add CharacterDatabase to scene and assign CSV.", MessageType.Warning);
            return;
        }

        if (heroNameProp == null)
        {
            EditorGUILayout.HelpBox("Cannot find hero.characterName property. Ensure 'public BaseHero hero;' exists.", MessageType.Error);
            return;
        }

        int currentIndex = Mathf.Max(0, System.Array.IndexOf(names, heroNameProp.stringValue));
        int newIndex = EditorGUILayout.Popup("Character", currentIndex, names);

        if (newIndex != currentIndex)
        {
            // record change for undo
            Undo.RecordObject(sm, "Change Character");
            heroNameProp.stringValue = names[newIndex];
            serializedObject.ApplyModifiedProperties();

            // Try apply character data immediately
            CharacterData data = TryGetCharacterData(names[newIndex]);
            if (data != null)
            {
                sm.hero.ApplyCharacterData(data);

                // Ensure Unity knows component changed (serializes the new values)
                EditorUtility.SetDirty(sm);
#if UNITY_2020_1_OR_NEWER
                // Force inspector refresh
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();
#endif
                Debug.Log($"[Editor] Applied character data: {data.name}");
            }
            else
            {
                Debug.LogWarning($"[Editor] Character data not found for: {names[newIndex]}");
            }
        }
    }

    // Tries to find names: prefer runtime CharacterDatabase, else read CSV from Resources (editor-safe)
    private string[] TryGetCharacterNames()
    {
        // 1) Runtime DB (if present in scene and already loaded)
        var db = FindObjectOfType<CharacterDatabase>();
        if (db != null && db.characters != null && db.characters.Count > 0)
            return db.characters.Select(c => c.name).ToArray();

        // 2) Editor fallback: try Resources first
#if UNITY_EDITOR
        TextAsset csv = Resources.Load<TextAsset>("Data/data_character");
        if (csv == null) csv = Resources.Load<TextAsset>("Data/Characters");
        if (csv != null)
        {
            List<string> names = new List<string>();
            string[] lines = csv.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                string[] cols = line.Split(',');
                if (cols.Length > 0)
                {
                    string nm = cols[0].Trim();
                    if (!string.IsNullOrEmpty(nm) && !names.Contains(nm))
                        names.Add(nm);
                }
            }
            if (names.Count > 0) return names.ToArray();
        }
#endif

        return new string[0];
    }

    // Try to retrieve CharacterData either from runtime DB or by parsing the CSV in Resources
    private CharacterData TryGetCharacterData(string name)
    {
        // Try runtime DB first
        var db = FindObjectOfType<CharacterDatabase>();
        if (db != null)
        {
            var d = db.GetCharacter(name);
            if (d != null) return d;
        }

        // Editor fallback: parse CSV from Resources
#if UNITY_EDITOR
        TextAsset csv = Resources.Load<TextAsset>("Data/data_character");
        if (csv == null) csv = Resources.Load<TextAsset>("Data/Characters");
        if (csv == null) return null;

        string[] lines = csv.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            string[] cols = line.Split(',');
            if (cols.Length < 7) continue;
            if (cols[0].Trim() == name)
            {
                CharacterData cd = new CharacterData();
                cd.name = cols[0].Trim();
                // role & element parsing (case-insensitive)
                System.Enum.TryParse(cols[1].Trim(), true, out cd.role);
                System.Enum.TryParse(cols[2].Trim(), true, out cd.element);

                float.TryParse(cols[3].Trim(), out cd.baseATK);
                float.TryParse(cols[4].Trim(), out cd.baseHP);
                float.TryParse(cols[5].Trim(), out cd.baseSpeed);
                float.TryParse(cols[6].Trim(), out cd.energy);

                return cd;
            }
        }
#endif
        return null;
    }
}
#endif
