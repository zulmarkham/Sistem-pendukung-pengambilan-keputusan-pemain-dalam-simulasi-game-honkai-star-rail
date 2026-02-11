// FuzzyRuleSupport.cs
using System.Collections.Generic;

/// <summary>
/// Support rule set: entries are (spCategory, energyCategory, hpCategory, action)
/// category mapping: 0 = Low, 1 = Med, 2 = High
/// Actions follow SupportAction enum below.
/// </summary>
public enum SupportAction { BasicAttack, Skill, Ultimate }

public struct SupportRuleEntry
{
    public int sp, en, hp;
    public SupportAction action;
    public SupportRuleEntry(int sp, int en, int hp, SupportAction action)
    {
        this.sp = sp; this.en = en; this.hp = hp; this.action = action;
    }
}   

public static class FuzzyRuleSupport
{
    public static  List<SupportRuleEntry> RuleSet = new List<SupportRuleEntry>()
    {
        // SP = Low (0)
        new SupportRuleEntry(0,0,0, SupportAction.BasicAttack), // α1
        new SupportRuleEntry(0,0,1, SupportAction.BasicAttack), // α2
        new SupportRuleEntry(0,0,2, SupportAction.BasicAttack), // α3
        new SupportRuleEntry(0,1,0, SupportAction.BasicAttack), // α4
        new SupportRuleEntry(0,1,1, SupportAction.BasicAttack), // α5
        new SupportRuleEntry(0,1,2, SupportAction.BasicAttack), // α6
        new SupportRuleEntry(0,2,0, SupportAction.Ultimate),    // α7
        new SupportRuleEntry(0,2,1, SupportAction.Ultimate),    // α8
        new SupportRuleEntry(0,2,2, SupportAction.Ultimate),    // α9

        // SP = Med (1)
        new SupportRuleEntry(1,0,0, SupportAction.Skill),       // α10
        new SupportRuleEntry(1,0,1, SupportAction.Skill),       // α11
        new SupportRuleEntry(1,0,2, SupportAction.Skill),       // α12
        new SupportRuleEntry(1,1,0, SupportAction.Skill),       // α13
        new SupportRuleEntry(1,1,1, SupportAction.Skill),       // α14
        new SupportRuleEntry(1,1,2, SupportAction.Skill),       // α15
        new SupportRuleEntry(1,2,0, SupportAction.Ultimate),    // α16
        new SupportRuleEntry(1,2,1, SupportAction.Ultimate),    // α17
        new SupportRuleEntry(1,2,2, SupportAction.Ultimate),    // α18

        // SP = High (2)
        new SupportRuleEntry(2,0,0, SupportAction.Skill),       // α19
        new SupportRuleEntry(2,0,1, SupportAction.Skill),       // α20
        new SupportRuleEntry(2,0,2, SupportAction.Skill),       // α21
        new SupportRuleEntry(2,1,0, SupportAction.Skill),       // α22
        new SupportRuleEntry(2,1,1, SupportAction.Skill),       // α23
        new SupportRuleEntry(2,1,2, SupportAction.Skill),       // α24
        new SupportRuleEntry(2,2,0, SupportAction.Ultimate),    // α25
        new SupportRuleEntry(2,2,1, SupportAction.Ultimate),    // α26
        new SupportRuleEntry(2,2,2, SupportAction.Ultimate)     // α27
    };
}
