using System.Text;
using UnityEngine;

public struct TankHealerDecisionResult
{
    public TankHealerAction Action;
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

public class FuzzyTankHealer
{
    float ZBasic(float alpha)
    {
        return 33f - (33f * alpha);
    }

    // Skill → 34–66 (monoton naik)
    float ZSkill(float alpha)
    {
        return 34f + (32f * alpha);
    }

    // Ultimate → 67–100 (monoton naik)
    float ZUlt(float alpha)
    {
        return 67f + (33f * alpha);
    }

    public TankHealerDecisionResult Decide(
        int currentSP, int maxSP,
        float energyValue,
        float hpHeroCurrent, float hpHeroMax,
        bool canUseSkill, bool canUseUlt)
    {
        float hpPercent = (hpHeroMax > 0f)
            ? Mathf.Clamp01(hpHeroCurrent / hpHeroMax) * 100f
            : 0f;

        Debug.Log("===== FUZZY TANK/HEALER DECISION =====");
        Debug.Log($"Input → SP:{currentSP}/{maxSP}, Energy:{energyValue}, HP:{hpPercent:F1}%");
        Debug.Log($"Can Use → Skill:{canUseSkill}, Ultimate:{canUseUlt}");

        float[] muSP = {
            FuzzyMembershipTankHealer.MuSPLow(currentSP),
            FuzzyMembershipTankHealer.MuSPMed(currentSP),
            FuzzyMembershipTankHealer.MuSPHigh(currentSP)
        };

        float[] muE = {
            FuzzyMembershipTankHealer.MuEnergyLow(energyValue),
            FuzzyMembershipTankHealer.MuEnergyMed(energyValue),
            FuzzyMembershipTankHealer.MuEnergyHigh(energyValue)
        };

        float[] muHP = {
            FuzzyMembershipTankHealer.MuHPLow(hpPercent),
            FuzzyMembershipTankHealer.MuHPMed(hpPercent),
            FuzzyMembershipTankHealer.MuHPHigh(hpPercent)
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

        foreach (var r in FuzzyRuleTankHealer.RuleSet)
        {
            float alpha = Mathf.Min(muSP[r.sp], Mathf.Min(muE[r.en], muHP[r.hp]));
            if (alpha <= 0f) continue;

            float zRule = 0f;

            switch (r.action)
            {
                case TankHealerAction.BasicAttack:
                    zRule = ZBasic(alpha);
                    maxBasic = Mathf.Max(maxBasic, alpha);
                    break;

                case TankHealerAction.Skill:
                    if (!canUseSkill) continue;
                    zRule = ZSkill(alpha);
                    maxSkill = Mathf.Max(maxSkill, alpha);
                    break;

                case TankHealerAction.Ultimate:
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

        TankHealerAction finalAction =
            (zFinal <= 33f) ? TankHealerAction.BasicAttack :
            (zFinal <= 66f) ? TankHealerAction.Skill :
                              TankHealerAction.Ultimate;

        Debug.Log($"Alpha Max → Basic:{maxBasic:F2}, Skill:{maxSkill:F2}, Ult:{maxUlt:F2}");
        Debug.Log($"Defuzz → SumAlpha:{sumAlpha:F2}, SumAlphaZ:{sumAlphaZ:F2}");
        Debug.Log($"Crisp Z → {zFinal:F2}");
        Debug.Log($"FINAL DECISION → {finalAction}");
        Debug.Log("================================");

        return new TankHealerDecisionResult
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

            role = "Tank / Healer"
        };
    }
}
