using System.Text;
using UnityEngine;

public class DPSDecisionResult
{
    public HeroAction Action;
    public float CrispZ;
    public float AlphaBasic;
    public float AlphaSkill;
    public float AlphaUlt;

    public float[] MuSPUI;
    public float[] MuEnergyUI;
    public float[] MuHPUI;
    public float HpPercentUI;

    public string RuleFiredText;
    public float SumAlphaUI;
    public float SumAlphaZUI;

    public bool CanUseSkillUI;
    public bool CanUseUltUI;

    public string role;
}

public class FuzzyDPS
{
    float ZBasic(float alpha)
    {
        // output monoton menurun (0–33)
        return 33f - (33f * alpha);
    }

    float ZSkill(float alpha)
    {
        // output monoton naik (34–66)
        return 34f + (32f * alpha);
    }

    float ZUlt(float alpha)
    {
        // output monoton naik (67–100)
        return 67f + (33f * alpha);
    }


    public DPSDecisionResult Decide(
        int currentSP, int maxSP,
        float energyValue,
        float hpMonsterCurrent, float hpMonsterMax,
        bool canUseSkill, bool canUseUlt)
    {
        float hpPercent = (hpMonsterMax > 0f)
            ? Mathf.Clamp01(hpMonsterCurrent / hpMonsterMax) * 100f
            : 0f;

        Debug.Log("===== FUZZY DPS DECISION =====");
        Debug.Log($"Input → SP:{currentSP}/{maxSP}, Energy:{energyValue}, EnemyHP:{hpPercent:F1}%");
        Debug.Log($"Can Use → Skill:{canUseSkill}, Ultimate:{canUseUlt}");

        float[] muSP = {
            FuzzyMembershipDPS.MuSPLow(currentSP),
            FuzzyMembershipDPS.MuSPMed(currentSP),
            FuzzyMembershipDPS.MuSPHigh(currentSP)
        };

        float[] muE = {
            FuzzyMembershipDPS.MuEnergyLow(energyValue),
            FuzzyMembershipDPS.MuEnergyMed(energyValue),
            FuzzyMembershipDPS.MuEnergyHigh(energyValue)
        };

        float[] muHP = {
            FuzzyMembershipDPS.MuHPLow(hpPercent),
            FuzzyMembershipDPS.MuHPMed(hpPercent),
            FuzzyMembershipDPS.MuHPHigh(hpPercent)
        };

        Debug.Log($"Membership SP     → Low:{muSP[0]:F2}, Med:{muSP[1]:F2}, High:{muSP[2]:F2}");
        Debug.Log($"Membership Energy → Low:{muE[0]:F2}, Med:{muE[1]:F2}, High:{muE[2]:F2}");
        Debug.Log($"Membership HP     → Low:{muHP[0]:F2}, Med:{muHP[1]:F2}, High:{muHP[2]:F2}");

        float sumAlpha = 0f;
        float sumAlphaZ = 0f;

        float maxBasic = 0f;
        float maxSkill = 0f;
        float maxUlt = 0f;

        StringBuilder ruleText = new StringBuilder();

        foreach (var r in FuzzyRuleDPS.RuleSet)
        {
            float alpha = Mathf.Min(muSP[r.sp], Mathf.Min(muE[r.en], muHP[r.hp]));
            if (alpha <= 0f) continue;
            
            float zRule = 0f;   

            switch (r.action)
            {
                case HeroAction.BasicAttack:
                    zRule = ZBasic(alpha);
                    maxBasic = Mathf.Max(maxBasic, alpha);
                    break;

                case HeroAction.Skill:
                    if (!canUseSkill) continue;
                    zRule = ZSkill(alpha);
                    maxSkill = Mathf.Max(maxSkill, alpha);
                    break;

                case HeroAction.Ultimate:
                    if (!canUseUlt) continue;
                    zRule = ZUlt(alpha);
                    maxUlt = Mathf.Max(maxUlt, alpha);
                    break;
            }

            sumAlpha += alpha;
            sumAlphaZ += alpha * zRule;

            string ruleLine = $"SP:{r.sp} Energy:{r.en} HP:{r.hp} | {r.action} | α={alpha:F2} | z={zRule:F2}";

            Debug.Log($"Rule Fired → {ruleLine}");
            ruleText.AppendLine(ruleLine);
        }

        float zFinal = (sumAlpha <= 0f) ? 0f : sumAlphaZ / sumAlpha;

        HeroAction finalAction =
            (zFinal <= 33f) ? HeroAction.BasicAttack :
            (zFinal <= 66f) ? HeroAction.Skill :
                              HeroAction.Ultimate;

        Debug.Log($"Alpha Max → Basic:{maxBasic:F2}, Skill:{maxSkill:F2}, Ult:{maxUlt:F2}");
        Debug.Log($"Defuzz → SumAlpha:{sumAlpha:F2}, SumAlphaZ:{sumAlphaZ:F2}");
        Debug.Log($"Crisp Z → {zFinal:F2}");
        Debug.Log($"FINAL DECISION → {finalAction}");
        Debug.Log("================================");

        return new DPSDecisionResult
        {
            Action = finalAction,
            CrispZ = zFinal,

            AlphaBasic = maxBasic,
            AlphaSkill = maxSkill,
            AlphaUlt = maxUlt,

            MuSPUI = muSP,
            MuEnergyUI = muE,
            MuHPUI = muHP,
            HpPercentUI = hpPercent,

            RuleFiredText = ruleText.ToString(),
            SumAlphaUI = sumAlpha,
            SumAlphaZUI = sumAlphaZ,

            CanUseSkillUI = canUseSkill,
            CanUseUltUI = canUseUlt,

            role = "DPS"
        };
    }
}
