using UnityEngine;

public static class FuzzyMembershipTankHealer
{
    // SP membership (1-5 scale)
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

    // Energy membership (0-110+)
    public static float MuEnergyLow(float e)
    {
        if (e <= 10f) return 1f;
        if (e >= 50f) return 0f;
        return (50f - e) / 40f;
    }

    public static float MuEnergyMed(float e)
    {
        if (e <= 20f || e >= 90f) return 0f;
        if (e == 55f) return 1f;
        if (e < 55f) return (e - 20f) / 35f;
        return (90f - e) / 35f;
    }

    public static float MuEnergyHigh(float e)
    {
        if (e <= 70f) return 0f;
        if (e >= 110f) return 1f;
        return (e - 70f) / 40f;
    }

    // HP percent membership (0..100)
    public static float MuHPLow(float hp)
    {
        if (hp <= 20f) return 1f;
        if (hp >= 50f) return 0f;
        return (50f - hp) / 30f;
    }

    public static float MuHPMed(float hp)
    {
        if (hp <= 30f || hp >= 90f) return 0f;
        if (hp == 60f) return 1f;
        if (hp < 60f) return (hp - 30f) / 30f;
        return (90f - hp) / 30f;
    }

    public static float MuHPHigh(float hp)
    {
        if (hp <= 70f) return 0f;
        if (hp >= 100f) return 1f;
        return (hp - 70f) / 30f;
    }
}
