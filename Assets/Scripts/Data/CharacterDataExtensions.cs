using System.Collections.Generic;

public static class CharacterDataExtensions
{
    public static List<string> GetCharacterNames(this List<CharacterData> list)
    {
        List<string> names = new List<string>();
        foreach (var c in list)
        {
            if (!string.IsNullOrEmpty(c.name))
                names.Add(c.name);
        }
        return names;
    }
}
