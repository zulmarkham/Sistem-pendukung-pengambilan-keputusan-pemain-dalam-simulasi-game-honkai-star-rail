using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string name;
    public BaseHero.roleType role;
    public BaseHero.combatType element;

    public float baseATK;
    public float baseHP;
    public float baseSpeed;
    public float energy;
}
