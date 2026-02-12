using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseAttack : MonoBehaviour
{
    public string attackName;
    public float attackDamage;
    public bool isUltimate;


    public enum AttackCategory
    {
        Damage,
        Heal,
        Shield,
        Buff
    }

    public enum AttackTarget
    {
        Enemy,
        Ally,
        Self
    }

    public AttackCategory category = AttackCategory.Damage;
    public AttackTarget targetType = AttackTarget.Enemy;

}
