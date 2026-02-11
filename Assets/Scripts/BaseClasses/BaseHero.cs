using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseHero : BaseClass
{
    public float currentShield;
    public string characterName;
    public float energy;
  
    public float currentEnergy;
    
    public enum combatType
    {

        ICE,
        FIRE,
        LIGHTNING,
        WIND,
        IMAGINARY,
        PHYSICAL,
        QUANTUM

    }

    public enum roleType
    {

        DPS,
        TANKHEALER,
        SUPPORT
    }

    public combatType element;
    public roleType role;
    public List<BaseAttack> SkillAttack = new List<BaseAttack>();
    public List<BaseAttack> UltimateAttack = new List<BaseAttack>();


    public float atkBuffMultiplier = 1f;

    public void ApplyCharacterData(CharacterData data)
    {
        characterName = data.name;

        baseHP = data.baseHP;
        baseATK = data.baseATK;
        baseSpeed = data.baseSpeed;
        energy = data.energy;

        element = data.element;
        role = data.role;

        name = data.name;
    }

    public void InitBattleState()
    {
        currentHP = baseHP;
        currentEnergy = Random.Range(20, 30);
        currentATK = baseATK + atkBuffMultiplier;
        currentSpeed = baseSpeed;
        currentShield = 0f;

        baseAVunit();
        currentActionValue = baseActionValue;
    }
    public void RecalculateATK()
    {
        currentATK = baseATK * atkBuffMultiplier;
    }





   
}
