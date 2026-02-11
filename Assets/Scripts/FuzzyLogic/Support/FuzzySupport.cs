using System.Text;
using UnityEngine;

public struct SupportDecisionResult
{
    public SupportAction Action;
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

public class FuzzySupport
{
    float ZBasic(float alpha)
    {
        return 33f - (33f * alpha);
    }

    // Skill Support (monoton naik)
    float ZSkill(float alpha)
    {
        return 34f + (32f * alpha);
    }

    // Ultimate Support (monoton naik)
    float ZUlt(float alpha)
    {
        return 67f + (33f * alpha);
    }

    public SupportDecisionResult Decide(
        int currentSP, int maxSP,
        float energyValue,
        float hpMonsterCurrent, float hpMonsterMax,
        bool canUseSkill, bool canUseUlt)
    {
        float hpPercent = (hpMonsterMax > 0f)
            ? Mathf.Clamp01(hpMonsterCurrent / hpMonsterMax) * 100f
            : 0f;


        Debug.Log("===== FUZZY SUPPORT DECISION =====");
        Debug.Log($"Input → SP:{currentSP}/{maxSP}, Energy:{energyValue}, HP:{hpPercent:F1}%");
        Debug.Log($"Can Use → Skill:{canUseSkill}, Ultimate:{canUseUlt}");

        float[] muSP = {
            FuzzyMembershipSupport.MuSPLow(currentSP),
            FuzzyMembershipSupport.MuSPMed(currentSP),
            FuzzyMembershipSupport.MuSPHigh(currentSP)
        };

        float[] muE = {
            FuzzyMembershipSupport.MuEnergyLow(energyValue),
            FuzzyMembershipSupport.MuEnergyMed(energyValue),
            FuzzyMembershipSupport.MuEnergyHigh(energyValue)
        };

        float[] muHP = {
            FuzzyMembershipSupport.MuHPLow(hpPercent),
            FuzzyMembershipSupport.MuHPMed(hpPercent),
            FuzzyMembershipSupport.MuHPHigh(hpPercent)
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

        foreach (var r in FuzzyRuleSupport.RuleSet)
        {
            // α = min(µSP, µEnergy, µHP)
            float alpha = Mathf.Min(muSP[r.sp], Mathf.Min(muE[r.en], muHP[r.hp]));
            if (alpha <= 0f) continue;

            float zRule = 0f;

            switch (r.action)
            {
                case SupportAction.BasicAttack:
                    zRule = ZBasic(alpha);
                    maxBasic = Mathf.Max(maxBasic, alpha);
                    break;

                case SupportAction.Skill:
                    if (!canUseSkill) continue;
                    zRule = ZSkill(alpha);
                    maxSkill = Mathf.Max(maxSkill, alpha);
                    break;

                case SupportAction.Ultimate:
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

        SupportAction finalAction =
            (zFinal <= 33f) ? SupportAction.BasicAttack :
            (zFinal <= 66f) ? SupportAction.Skill :
                        SupportAction.Ultimate;

        Debug.Log($"Alpha Max → Basic:{maxBasic:F2}, Skill:{maxSkill:F2}, Ult:{maxUlt:F2}");
        Debug.Log($"Defuzz → SumAlpha:{sumAlpha:F2}, SumAlphaZ:{sumAlphaZ:F2}");
        Debug.Log($"Crisp Z → {zFinal:F2}");
        Debug.Log($"FINAL DECISION → {finalAction}");
        Debug.Log("================================");

        return new SupportDecisionResult
        {
            Action = finalAction,
            CrispZ = zFinal ,

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

            role = "Support"
        };

    }
}
