using System.Collections.Generic;

public enum HeroAction { BasicAttack, Skill, Ultimate }

public struct RuleEntry
{
    public int sp, en, hp;
    public HeroAction action;

    public RuleEntry(int sp, int en, int hp, HeroAction action)
    {
        this.sp = sp;
        this.en = en;
        this.hp = hp;
        this.action = action;
    }
}

public static class FuzzyRuleDPS
{
    public static List<RuleEntry> RuleSet = new List<RuleEntry>()
    {
        // SP: 0=Low 1=Med 2=High   Energy:0/1/2  HP:0/1/2
        new RuleEntry(0,0,0,HeroAction.BasicAttack),
        new RuleEntry(0,0,1,HeroAction.BasicAttack),
        new RuleEntry(0,0,2,HeroAction.BasicAttack),
        new RuleEntry(0,1,0,HeroAction.BasicAttack),
        new RuleEntry(0,1,1,HeroAction.BasicAttack),
        new RuleEntry(0,1,2,HeroAction.BasicAttack),
        new RuleEntry(0,2,0,HeroAction.Ultimate),
        new RuleEntry(0,2,1,HeroAction.Ultimate),
        new RuleEntry(0,2,2,HeroAction.Ultimate),

        new RuleEntry(1,0,0,HeroAction.Skill),
        new RuleEntry(1,0,1,HeroAction.Skill),
        new RuleEntry(1,0,2,HeroAction.Skill),
        new RuleEntry(1,1,0,HeroAction.Skill),
        new RuleEntry(1,1,1,HeroAction.Skill),
        new RuleEntry(1,1,2,HeroAction.Skill),
        new RuleEntry(1,2,0,HeroAction.Ultimate),
        new RuleEntry(1,2,1,HeroAction.Ultimate),
        new RuleEntry(1,2,2,HeroAction.Ultimate),

        new RuleEntry(2,0,0,HeroAction.Skill),
        new RuleEntry(2,0,1,HeroAction.Skill),
        new RuleEntry(2,0,2,HeroAction.Skill),
        new RuleEntry(2,1,0,HeroAction.Skill),
        new RuleEntry(2,1,1,HeroAction.Skill),
        new RuleEntry(2,1,2,HeroAction.Skill),
        new RuleEntry(2,2,0,HeroAction.Ultimate),
        new RuleEntry(2,2,1,HeroAction.Ultimate),
        new RuleEntry(2,2,2,HeroAction.Ultimate),
    };
}
