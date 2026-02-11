using UnityEngine;

public static class FuzzyMembershipDPS
{
    // HP Monster membership (0-100 %)
    public static float MuHPLow(float hp)
    {
        if (hp <= 25f) return 1f;
        if (hp >= 50f) return 0f;
        return (50f - hp) / (50f - 25f);
    }

    public static float MuHPMed(float hp)
    {
        if (hp <= 25f || hp >= 100f) return 0f;
        if (hp >= 50f && hp <= 75f) return 1f;
        if (hp > 25f && hp < 50f) return (hp - 25f) / (50f - 25f);
        return (100f - hp) / (100f - 75f);
    }

    public static float MuHPHigh(float hp)
    {
        if (hp <= 75f) return 0f;
        if (hp >= 100f) return 1f;
        return (hp - 75f) / (100f - 75f);
    }

    // Energy membership (0-120)
    public static float MuEnergyLow(float e)
    {
        if (e <= 30f) return 1f;
        if (e >= 60f) return 0f;
        return (60f - e) / (60f - 30f);
    }

    public static float MuEnergyMed(float e)
    {
        if (e <= 30f || e >= 120f) return 0f;
        if (e >= 60f && e <= 90f) return 1f;
        if (e > 30f && e < 60f) return (e - 30f) / (60f - 30f);
        return (120f - e) / (120f -90f);
    }

    public static float MuEnergyHigh(float e)
    {
        if (e <= 90f) return 0f;
        if (e >= 120f) return 1f;
        return (e - 90f) / (120f - 90f);
    }

    // Skill Point membership (integer 0-5+)
    public static float MuSPLow(int sp)
    {
        if (sp <= 1) return 1f;
        if (sp >= 5) return 0f;
        return (5f - sp) / 4f;
    }

    public static float MuSPMed(int sp)
    {
        if (sp <= 1 || sp >= 5) return 0f;
        if (sp <= 3) return (sp - 1f) / 2f;
        return (5f - sp) / 2f;
    }

    public static float MuSPHigh(int sp)
    {
        if (sp <= 1) return 0f;
        if (sp >= 5) return 1f;
        return (sp - 1f) / 4f;
    }
}
