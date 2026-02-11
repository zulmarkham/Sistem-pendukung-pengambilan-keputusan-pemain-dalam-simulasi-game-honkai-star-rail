using UnityEngine;

public class loaddata : MonoBehaviour
{
    public TextAsset textFile;
    public string selectedCharacterName;

    [Header("Character Stats")]
    public string name;
    public float baseHP;
    public float currentHP;
    public float baseDEF;
    public float baseATK;

    public void LoadSelectedCharacter()
    {
        if (textFile == null || string.IsNullOrEmpty(selectedCharacterName))
        {
            Debug.LogWarning("TextAsset atau nama karakter belum diisi.");
            return;
        }

        string[] lines = textFile.text.Split('\n');
        System.Collections.Generic.Dictionary<string, string> currentData = new System.Collections.Generic.Dictionary<string, string>();
        bool inCharacterBlock = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line)) continue;

            if (line == "[Character]")
            {
                if (currentData.ContainsKey("name") && currentData["name"] == selectedCharacterName)
                {
                    AssignCharacterData(currentData);
                    return;
                }

                currentData.Clear();
                inCharacterBlock = true;
                continue;
            }

            if (inCharacterBlock && line.Contains(":"))
            {
                string[] parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    currentData[key] = value;
                }
            }
        }

        // Cek blok terakhir
        if (currentData.ContainsKey("name") && currentData["name"] == selectedCharacterName)
        {
            AssignCharacterData(currentData);
        }
        else
        {
            Debug.LogWarning("Karakter tidak ditemukan.");
        }
    }

    void AssignCharacterData(System.Collections.Generic.Dictionary<string, string> data)
    {
        name = data["name"];
        baseHP = float.Parse(data["baseHP"]);
        currentHP = baseHP;
        baseDEF = float.Parse(data["baseDEF"]);
        baseATK = float.Parse(data["baseATK"]);

        Debug.Log($"Loaded {name} | HP: {baseHP} | DEF: {baseDEF} | ATK: {baseATK}");
    }
}
