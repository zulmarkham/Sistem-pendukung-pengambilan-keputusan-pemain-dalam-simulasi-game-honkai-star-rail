using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BaseClass
{

    public BaseClass()
    {
        baseAVunit();
    }
    public string name;

    public float baseHP;
    public float currentHP;
    public float baseATK;
    public float currentATK;
    
    public float baseSpeed; 
    public float currentSpeed;
    [InspectorReadOnly] public float baseActionValue;
    public float currentActionValue; 
    public bool isTurn;

    public List<BaseAttack> Attacks = new List<BaseAttack>();

    public void baseAVunit()
    {
        if (baseSpeed != 0)
        {
            baseActionValue = 10000f / baseSpeed;
        }
        else
        {
            baseActionValue = 0;
        }
        return;
    }

}
