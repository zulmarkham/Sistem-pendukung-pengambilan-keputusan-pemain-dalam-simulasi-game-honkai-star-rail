using System.Collections.Generic;
   
public enum TankHealerAction
{
    BasicAttack,
    Skill,
    Ultimate
}

public struct TankHealerRuleEntry
{
    public int sp;
    public int en;
    public int hp;
    public TankHealerAction action;

    public TankHealerRuleEntry(int sp, int en, int hp, TankHealerAction action)
    {
        this.sp = sp;
        this.en = en;
        this.hp = hp;
        this.action = action;
    }
}

public static class FuzzyRuleTankHealer
{
    // 27 Rules (3x3x3)
    public static List<TankHealerRuleEntry> RuleSet = new List<TankHealerRuleEntry>()
    {
        // SP Low
        new TankHealerRuleEntry(0,0,0,TankHealerAction.Skill),
        new TankHealerRuleEntry(0,0,1,TankHealerAction.BasicAttack),
        new TankHealerRuleEntry(0,0,2,TankHealerAction.BasicAttack),

        new TankHealerRuleEntry(0,1,0,TankHealerAction.Skill),
        new TankHealerRuleEntry(0,1,1,TankHealerAction.BasicAttack),
        new TankHealerRuleEntry(0,1,2,TankHealerAction.BasicAttack),

        new TankHealerRuleEntry(0,2,0,TankHealerAction.Ultimate),
        new TankHealerRuleEntry(0,2,1,TankHealerAction.Ultimate),
        new TankHealerRuleEntry(0,2,2,TankHealerAction.Ultimate),

        // SP Medium
        new TankHealerRuleEntry(1,0,0,TankHealerAction.Skill),
        new TankHealerRuleEntry(1,0,1,TankHealerAction.Skill),
        new TankHealerRuleEntry(1,0,2,TankHealerAction.BasicAttack),

        new TankHealerRuleEntry(1,1,0,TankHealerAction.Skill),
        new TankHealerRuleEntry(1,1,1,TankHealerAction.Skill),
        new TankHealerRuleEntry(1,1,2,TankHealerAction.BasicAttack),

        new TankHealerRuleEntry(1,2,0,TankHealerAction.Ultimate),
        new TankHealerRuleEntry(1,2,1,TankHealerAction.Ultimate),
        new TankHealerRuleEntry(1,2,2,TankHealerAction.Ultimate),

        // SP High
        new TankHealerRuleEntry(2,0,0,TankHealerAction.Skill),
        new TankHealerRuleEntry(2,0,1,TankHealerAction.Skill),
        new TankHealerRuleEntry(2,0,2,TankHealerAction.BasicAttack),

        new TankHealerRuleEntry(2,1,0,TankHealerAction.Skill),
        new TankHealerRuleEntry(2,1,1,TankHealerAction.Skill),
        new TankHealerRuleEntry(2,1,2,TankHealerAction.Skill),

        new TankHealerRuleEntry(2,2,0,TankHealerAction.Ultimate),
        new TankHealerRuleEntry(2,2,1,TankHealerAction.Ultimate),
        new TankHealerRuleEntry(2,2,2,TankHealerAction.Ultimate)
    };
}
