using System.Collections.Generic;
using UnityEngine;

public class CharacterDatabase : MonoBehaviour
{
    public static CharacterDatabase Instance;

    [Header("CSV File (TextAsset)")]
    public TextAsset characterCSV;

    public List<CharacterData> characters = new List<CharacterData>();
    private Dictionary<string, CharacterData> lookup = new Dictionary<string, CharacterData>();

    void Awake()
    {
        Instance = this;
        LoadCSV();
    }

#if UNITY_EDITOR
    // Load ulang saat nilai berubah di Inspector (edit mode)
    private void OnValidate()
    {
        if (!Application.isPlaying)
            LoadCSV();
    }
#endif

    public CharacterData GetCharacter(string name)
    {
        if (lookup.TryGetValue(name, out var data))
            return data;

        return null;
    }

    public void LoadCSV()
    {
        lookup.Clear();
        characters.Clear();

        if (characterCSV == null)
        {
            Debug.LogWarning("[Editor] CSV file missing!");
            return;
        }

        string[] rows = characterCSV.text.Split('\n');

        for (int i = 1; i < rows.Length; i++)
        {
            string row = rows[i].Trim();
            if (string.IsNullOrEmpty(row)) continue;

            string[] col = row.Split(',');

            if (col.Length < 7) continue;

            CharacterData cha = new CharacterData();
            cha.name = col[0].Trim();

            // Parse Role (case-insensitive)
            if (!System.Enum.TryParse(col[1].Trim(), true, out cha.role))
            {
                Debug.LogWarning($"[CSV Parsing] Invalid role '{col[1]}' for {cha.name}. Default = DPS");
                cha.role = BaseHero.roleType.DPS;
            }

            // Parse Element (case-insensitive)
            if (!System.Enum.TryParse(col[2].Trim(), true, out cha.element))
            {
                Debug.LogWarning($"[CSV Parsing] Invalid element '{col[2]}' for {cha.name}. Default = PHYSICAL");
                cha.element = BaseHero.combatType.PHYSICAL;
            }

            // Stat parsing
            cha.baseATK = float.Parse(col[3]);
            cha.baseHP = float.Parse(col[4]);
            cha.baseSpeed = float.Parse(col[5]);
            cha.energy = float.Parse(col[6]);

            characters.Add(cha);
            lookup[cha.name] = cha;
        }

        Debug.Log($"[Editor] Loaded {characters.Count} characters into database.");
    }
}
