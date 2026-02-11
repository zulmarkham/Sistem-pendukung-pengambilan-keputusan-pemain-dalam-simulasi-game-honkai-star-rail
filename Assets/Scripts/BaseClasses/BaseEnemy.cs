using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseEnemy: BaseClass
{



    
    public float toughness;

    public enum enemyWeakness {
        
        PHYSICAL,
        FIRE,
        ICE,
        QUANTUM,
        LIGHTNING,
        IMAGINARY,
        WIND

    }

    public enum enemyType {

        COMMON,
        UNCOMMON,
        BOSS

    }

    public enemyWeakness weakness;
    public enemyType type;

}
