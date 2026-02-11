using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SkillPoints
{
        [InspectorReadOnly]public int maxSkillPoints = 5;
        [InspectorReadOnly]public int minSkillPoints = 0;
        [InspectorReadOnly]public int currentSkillPoints;

    public SkillPoints ()
    {
        currentSkillPoints = 0;
    }

    public void SkillPointInit (int startValue)
    {
        currentSkillPoints = Mathf.Clamp(startValue, minSkillPoints, maxSkillPoints);
    }

    public bool useSkillPoint (int amount)
    {
        if (currentSkillPoints - amount < minSkillPoints)
        {
            return false;
        }

        currentSkillPoints -= amount;
        return true;
    }

    public void gainSkillPoint (int amount)
    {
        currentSkillPoints = Mathf.Clamp(currentSkillPoints + amount, minSkillPoints, maxSkillPoints);
    }
}
