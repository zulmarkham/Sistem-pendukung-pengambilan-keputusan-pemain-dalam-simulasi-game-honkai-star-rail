using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackButton : MonoBehaviour
{
    public BaseAttack skillAttackToPerform;
    public void castSkillAttack()
    {
        GameObject.Find("BattleManager").GetComponent<BattleStateMachine>().Input3(skillAttackToPerform);
    }

}
